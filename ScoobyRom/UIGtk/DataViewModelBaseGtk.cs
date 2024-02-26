// DataViewModelBaseGtk.cs: Gtk.TreeModel common functionality for UI

/* Copyright (C) 2011-2015 SubaruDieselCrew
 *
 * This file is part of ScoobyRom.
 *
 * ScoobyRom is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ScoobyRom is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ScoobyRom.  If not, see <http://www.gnu.org/licenses/>.
 */


#define UseBackGroundTask

using System;
using System.Threading;
using System.Threading.Tasks;
using Gtk;

namespace ScoobyRom
{
	// sort of ViewModel in M-V-VM (Model-View-ViewModel pattern)
	public abstract class DataViewModelBaseGtk
	{
		protected CancellationTokenSource tokenSource = new CancellationTokenSource ();
		protected Task task;

		// generates icons
		protected PlotIconBase plotIcon;

		protected readonly Data data;

		// main TreeStore, most of the core data is being copied into this
		public ListStore store;

		protected bool iconsVisible, iconsCached;

		public TreeModel TreeModel {
			get { return this.store; }
		}

		public RectSizing IconSizing {
			get { return this.plotIcon.IconSizing; }
		}

		protected abstract int ColumnNrIcon { get; }

		protected abstract int ColumnNrObj { get; }

		protected abstract int ColumnNrToggle { get; }

		public DataViewModelBaseGtk (Data data, PlotIconBase plotIcon)
		{
			this.data = data;
			this.plotIcon = plotIcon;
			InitStore ();
		}

		protected void OnDataItemsChanged (object sender, EventArgs e)
		{
			this.store.Clear ();
			iconsCached = false;
			PopulateData ();
		}

		public void ChangeTableType (Tables.Denso.Table table, Tables.TableType newType)
		{
			data.ChangeTableType (table, newType);
		}

		public bool IconsVisible {
			get { return iconsVisible; }
			set { iconsVisible = value; }
		}


		/// <summary>
		/// Creates icons if not done already, otherwise returns immediatly.
		/// Icon creation happens in background.
		/// </summary>
		public void RequestIcons ()
		{
			if (iconsCached)
				return;
			RefreshIcons ();
		}

		public void IncreaseIconSize ()
		{
			plotIcon.ZoomIn ();
			RefreshIcons ();
		}

		public void DecreaseIconSize ()
		{
			plotIcon.ZoomOut ();
			RefreshIcons ();
		}

		public void ResetIconSize ()
		{
			plotIcon.ZoomReset ();
			RefreshIcons ();
		}

		public void RefreshIcons ()
		{
			iconsCached = false;

			#if !UseBackGroundTask

			tokenSource = new CancellationTokenSource ();
			var token = tokenSource.Token;
			CreateAllIcons (token);

			#else

			if (task != null && !task.IsCompleted) {
				tokenSource.Cancel ();
				// "It is not necessary to wait on tasks that have canceled."
				// https://msdn.microsoft.com/en-us/library/dd537607%28v=vs.100%29.aspx
				// but wait to let previous task finish current icon
				//task.Wait (500);
			}
			tokenSource = new CancellationTokenSource ();
			var token = tokenSource.Token;
			task = Task.Factory.StartNew (() => CreateAllIcons (token), token);

			#endif
		}

		protected void CreateAllIcons (CancellationToken ct)
		{
			int objColumnNr = ColumnNrObj;
			int iconColumnNr = ColumnNrIcon;

			ForEach (delegate(TreeIter iter) {
				if (ct.IsCancellationRequested)
					return false;

				var table = (Tables.Denso.Table)store.GetValue (iter, objColumnNr);
				Gdk.Pixbuf pixbuf = plotIcon.CreateIcon (table);

				// update model reference in GUI Thread to make sure UI display is ok
				// HACK Application.Invoke causes wrong iters ???
				// IA__gtk_list_store_set_value: assertion 'VALID_ITER (iter, list_store)' failed
				//Application.Invoke (delegate { store.SetValue (iter, iconColumnNr, pixbuf); });
				store.SetValue (iter, iconColumnNr, pixbuf);
				return false;
			});
			iconsCached = true;
		}

		public abstract void SetNodeContentTypeChanged (TreeIter iter, Tables.Denso.Table table);

		protected void CreateSetNewIcon (TreeIter iter, Tables.Denso.Table table)
		{
			store.SetValue (iter, ColumnNrIcon, plotIcon.CreateIcon (table));
		}

		#region TreeStore event handlers

		// called for each changed column!
		protected void HandleTreeStoreRowChanged (object o, RowChangedArgs args)
		{
			//Console.WriteLine ("TreeStoreRowChanged");
			UpdateModel (args.Iter);
		}

		// not called when treeView.Reorderable = true !!!
		// called when clicking column headers
		//		void HandleTreeStoreRowsReordered (object o, RowsReorderedArgs args)
		//		{
		//			Console.WriteLine ("TreeStore3D: RowsReordered");
		//		}

		#endregion TreeStore event handlers

		abstract protected void InitStore ();

		abstract protected void PopulateData ();

		abstract protected void UpdateModel (TreeIter iter);

		protected bool FindIter (Tables.Denso.Table table, out TreeIter iter)
		{
			// not using ForEach - more complicated due to returning iter
			if (!store.GetIterFirst (out iter))
				return false;

			Tables.Denso.Table currentTable;
			do {
				currentTable = (Tables.Denso.Table)store.GetValue (iter, ColumnNrObj);
				if (currentTable.Equals (table)) {
					return true;
				}
			} while (store.IterNext (ref iter));
			iter = TreeIter.Zero;
			return false;
		}

		/// <summary>
		/// Calls func on each node in model in a depth-first fashion.
		/// If func returns true, then the tree ceases to be walked, and this method returns.
		/// like Gtk.TreeModel.Foreach method which uses delegate bool TreeModelForeachFunc (ITreeModel model, TreePath path, TreeIter iter)
		/// Tested: slightly faster on Linux than Gtk.TreeModel.Foreach.Foreach, probably because of additional arguments.
		/// </summary>
		/// <param name="func">Func.</param>
		public void ForEach (Func<TreeIter, bool> func)
		{
			TreeIter iter;
			if (!store.GetIterFirst (out iter))
				return;
			do {
				if (func (iter))
					break;
			} while (store.IterNext (ref iter));
		}

		protected void SetHandleRowChanged (bool on)
		{
			if (on) {
				store.RowChanged += HandleTreeStoreRowChanged;
			} else {
				store.RowChanged -= HandleTreeStoreRowChanged;
			}
		}

		protected void Toggle (TreeIter iter, bool on)
		{
			store.SetValue (iter, ColumnNrToggle, on);
		}

		public void ToggleAll (bool on)
		{
			ForEach (delegate(TreeIter iter) {
				if (IsToggled (iter) != on) {
					Toggle (iter, on);
				}
				return false;
			});
		}

		protected bool IsToggled (TreeIter iter)
		{
			return (bool)store.GetValue (iter, ColumnNrToggle);
		}

		protected Tables.Denso.Table GetTable (TreeIter iter)
		{
			return (Tables.Denso.Table)store.GetValue (iter, ColumnNrObj);
		}

		/* not needed
		public List<Subaru.Tables.Table> GetToggled (bool toggled)
		{
			List<Subaru.Tables.Table> list = new List<Subaru.Tables.Table> (64);
			ForEach (delegate(TreeIter iter) {
				if (IsToggled (iter) == toggled) {
					list.Add (GetTable (iter));
				}
				return false;
			});
			return list;
		}
		*/
	}
}