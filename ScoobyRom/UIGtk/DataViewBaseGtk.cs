// DataViewBaseGtk.cs: Base class for Gtk.TreeView UI.

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


using System;
using System.Collections.Generic;
using Gtk;
using Tables;
using Tables.Denso;

namespace ScoobyRom
{
	public abstract class DataViewBaseGtk
	{
		public event EventHandler<ActionEventArgs> Activated;

		// needed model for ComboBox cells, sharable and const
		protected static readonly ListStore tableTypesModel = new ListStore (typeof(string));

		protected static readonly string[] AllowedHexPrefixes = new string[] { "0x", "$" };

		protected DataViewModelBaseGtk viewModel;

		protected Gtk.TreeModel treeModel;
		protected Gtk.TreeView treeView;

		protected CellRendererText cellRendererText, cellRendererTextEditable;
		//, cellRendererTextMono;
		protected CellRendererToggle cellRendererToggle;
		protected CellRendererCombo cellRendererCombo;
		protected CellRendererPixbuf cellRendererPixbuf;

		// not worth taking enum, Gtk methods need int anyway
		protected readonly Dictionary<TreeViewColumn, int> columnsDict = new Dictionary<TreeViewColumn, int> (25);
		protected bool showIcons = false;

		static DataViewBaseGtk ()
		{
			// populate strings to be shown in ComboBox
			foreach (string item in TableTypes.GetStrings ()) {
				tableTypesModel.AppendValues (item);
			}
		}

		public DataViewBaseGtk ()
		{
		}

		protected void TreeView_KeyPressEvent (object o, KeyPressEventArgs args)
		{
			const Gdk.ModifierType modifier = Gdk.ModifierType.ControlMask;
			Gdk.Key key = args.Event.Key;

			if ((args.Event.State & modifier) != 0) {
				if (key == Gdk.Key.Key_0 || key == Gdk.Key.KP_0) {
					ResetIconSize ();
					return;
				} else if (key == Gdk.Key.plus || key == Gdk.Key.KP_Add) {
					IncreaseIconSize ();
					return;
				} else if (key == Gdk.Key.minus || key == Gdk.Key.KP_Subtract) {
					DecreaseIconSize ();
					return;
				}
			}
		}

		protected abstract int ColumnNrIcon { get; }

		protected abstract int ColumnNrObj { get; }

		public bool ShowIcons {
			get { return this.showIcons; }
			set {
				showIcons = value;
				viewModel.IconsVisible = value;
				AjustIconCol ();

				if (value) {
					viewModel.RequestIcons ();
				}
			}
		}

		public void IncreaseIconSize ()
		{
			this.viewModel.IncreaseIconSize ();
			AjustIconCol ();
		}

		public void DecreaseIconSize ()
		{
			this.viewModel.DecreaseIconSize ();
			AjustIconCol ();
		}

		public void ResetIconSize ()
		{
			this.viewModel.ResetIconSize ();
			AjustIconCol ();
		}

		protected void AjustIconCol ()
		{
			var col = GetColumn (ColumnNrIcon);
			col.Visible = showIcons;

			if (!showIcons) {
				// HACK best solution to reduce row heights so far
				treeView.ColumnsAutosize ();
			} else {
				var iconSizing = this.viewModel.IconSizing;
				cellRendererPixbuf.SetFixedSize (iconSizing.Width, iconSizing.Height);

				// need some more width for PixBuf to be completely visible
				// HACK icon column FixedWidth
				col.FixedWidth = iconSizing.Width + 10;
				col.Sizing = TreeViewColumnSizing.Fixed;
			}
		}

		public Tables.Denso.Table Selected {
			get {
				Tables.Denso.Table table = null;
				TreeSelection selection = treeView.Selection;
				TreeModel model;
				TreeIter iter;

				// The iter will point to the selected row
				if (selection.GetSelected (out model, out iter)) {
					// Depth begins at 1 !
					//TreePath path = model.GetPath (iter);
					table = (Tables.Denso.Table)model.GetValue (iter, ColumnNrObj);
				}
				return table;
			}
		}

		#region Tree Cell Data Functions

		// These should be fast as they are called a lot, even for measuring hidden columns.

		protected void TreeCellDataFuncHex (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			int nr = (int)treeModel.GetValue (iter, columnsDict [treeViewColumn]);
			cellRendererText.Text = nr.ToString ("X");
		}

		// Without own data function floats would be rendered like "100.000000"
		// ToString() only adds decimals where necessary - much better.
		protected void TreeCellDataFuncFloat (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			float nr = (float)treeModel.GetValue (iter, columnsDict [treeViewColumn]);
			cellRendererText.Text = nr.ToString ();
		}

		protected void TreeCellDataFuncTableType (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			var tt = (TableType)treeModel.GetValue (iter, columnsDict [treeViewColumn]);
			cellRendererCombo.Text = tt.ToStr ();
		}

		#endregion Tree Cell Data Functions


		#region CellRenderer event handlers


		protected void HandleCellRendererTextEditableEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (treeModel.GetIter (out iter, new TreePath (args.Path))) {
				treeModel.SetValue (iter, CursorColNr, args.NewText);
				// follow it in case this column is being sorted
				ScrollTo (iter);
			}
		}

		protected void CellRendererToggled (object o, ToggledArgs args)
		{
			int colNr = CursorColNr;
			TreeIter iter;
			if (treeModel.GetIter (out iter, new TreePath (args.Path))) {
				bool toggleOld = (bool)treeModel.GetValue (iter, colNr);
				treeModel.SetValue (iter, colNr, !toggleOld);
			}
		}

		protected void HandleCellRendererComboEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			if (!treeModel.GetIter (out iter, new TreePath (args.Path)))
				return;
			TableType ttNew;
			if (!TableTypes.TryParse (args.NewText, out ttNew))
				return;
			int colNr = CursorColNr;

			// so far there's only ComboBox for TableType column
			var ttOld = (TableType)treeModel.GetValue (iter, colNr);
			if (ttOld != ttNew) {
				treeModel.SetValue (iter, colNr, (int)ttNew);
				OnTableTypeChanged (iter, ttNew);
				// follow it in case this column is being sorted
				ScrollTo (iter);
			}
		}

		protected void OnTableTypeChanged (TreeIter iter, TableType newTableType)
		{
			var table = (Tables.Denso.Table)treeModel.GetValue (iter, ColumnNrObj);
			viewModel.ChangeTableType (table, newTableType);
			viewModel.SetNodeContentTypeChanged (iter, table);
		}


		#endregion CellRenderer event handlers


		#region TreeView Search Functions

		// key = entered text in search (entry) widget

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncHex (string key, int content)
		{
			const System.Globalization.NumberStyles numberStyles = System.Globalization.NumberStyles.HexNumber;

			foreach (string prefix in AllowedHexPrefixes) {
				int index = key.IndexOf (prefix, StringComparison.InvariantCulture);
				if (index >= 0) {
					key = key.Substring (index + prefix.Length);
					break;
				}
			}
			int parsed;
			if (int.TryParse (key, numberStyles, System.Globalization.NumberFormatInfo.InvariantInfo, out parsed))
				return parsed != content;
			else
				return true;
		}

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncInt (string key, int content)
		{
			int searchNr;
			if (int.TryParse (key, out searchNr))
				return searchNr != content;
			else
				return true;
		}

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncFloat (string key, float content)
		{
			float searchNr;
			if (float.TryParse (key, out searchNr))
				return searchNr != content;
			else
				return true;
		}

		// like default behavior
		protected static bool EqualFuncString (string key, string content)
		{
			return !content.StartsWith (key, StringComparison.CurrentCultureIgnoreCase);
		}

		// FALSE if the row matches, TRUE otherwise !!!
		protected static bool EqualFuncTableType (string key, TableType content)
		{
			TableType parsed;
			if (TableTypes.TryParse (key, out parsed))
				return parsed != content;
			else
				return true;
		}

		#endregion TreeView Search Functions


		#region TreeView event handlers


		protected void OnCursorChanged (object obj, EventArgs e)
		{
			treeView.SearchColumn = CursorColNr;
		}

		//		void HandleTreeViewKeyPressEvent (object o, KeyPressEventArgs args)
		//		{
		//			Gdk.Key key = args.Event.Key;
		//			Console.WriteLine (key.ToString());
		//			if (key == (Gdk.Key.p | Gdk.Key.Control_L))
		//				Console.WriteLine ("p");
		//		}

		// double click or Enter key
		protected void HandleTreeViewRowActivated (object o, RowActivatedArgs args)
		{
			var table = Selected;
			if (table != null && Activated != null) {
				Activated (this, new ActionEventArgs (table));
			}
		}

		#endregion TreeView event handlers


		protected TreeViewColumn GetColumn (int col)
		{
			foreach (var kvp in columnsDict) {
				if (kvp.Value == col)
					return kvp.Key;
			}
			return null;
			// LinQ would be overkill
			//var pair = columnsDict.Where (kvp => kvp.Value == col).First();
		}

		/// <summary>
		/// Scroll vertically to keep row in view when sorting is active and sorted column data changes.
		/// Otherwise would need to manually scroll in order bring it back into view.
		/// (TreePath usually changes, TreeIter does not.)
		/// </summary>
		/// <param name="iter">
		/// A <see cref="TreeIter"/>
		/// </param>
		protected void ScrollTo (TreeIter iter)
		{
			// ScrollToCell needs TreePath
			// If column is null, then no horizontal scrolling occurs.
			treeView.ScrollToCell (treeModel.GetPath (iter), null, false, 0, 0);
		}

		// not perfect yet, might have to wait after all icons have been updated
		protected void ScrollToSelected ()
		{
			TreeSelection selection = treeView.Selection;
			TreeModel model;
			TreeIter iter;
			// The iter will point to the selected row
			if (selection.GetSelected (out model, out iter)) {
				ScrollTo (iter);
			}
		}

		protected TreeViewColumn CursorColumn {
			get {
				TreePath path;
				TreeViewColumn column;
				treeView.GetCursor (out path, out column);
				return column;
			}
		}

		protected int CursorColNr {
			get {
				TreeViewColumn column = CursorColumn;
				return column != null ? columnsDict [CursorColumn] : 0;
			}
		}

		#region CreateColumn

		protected TreeViewColumn CreateTextColumn (string displayName, int colNr)
		{
			return new TreeViewColumn (displayName, cellRendererText, "text", colNr);
		}

		protected TreeViewColumn CreateTextEditableColumn (string displayName, int colNr)
		{
			return new TreeViewColumn (displayName, cellRendererTextEditable, "text", colNr);
		}

		protected TreeViewColumn CreateFloatColumn (string displayName, int colNr)
		{
			var col = new TreeViewColumn (displayName, cellRendererText, "text", colNr);
			col.SetCellDataFunc (cellRendererText, TreeCellDataFuncFloat);
			return col;
		}

		protected TreeViewColumn CreateHexColumn (string displayName, int colNr)
		{
			var col = new TreeViewColumn (displayName, cellRendererText, "text", colNr);
			col.SetCellDataFunc (cellRendererText, TreeCellDataFuncHex);
			return col;
		}

		protected TreeViewColumn CreateIconColumn (int colNr)
		{
			var col = new TreeViewColumn ("Icon", cellRendererPixbuf, "pixbuf", colNr);
			col.Visible = showIcons;
			//col.MaxWidth = 64;
			// might help perf
			//col.Sizing = TreeViewColumnSizing.Fixed;
			//col.FixedWidth = 64;
			return col;
		}

		protected TreeViewColumn CreateTypeColumn (int colNr)
		{
			var col = new TreeViewColumn ("Type", cellRendererCombo, "text", colNr);
			col.SetCellDataFunc (cellRendererCombo, TreeCellDataFuncTableType);
			return col;
		}

		protected TreeViewColumn CreateToggleColumn (int colNr)
		{
			return new TreeViewColumn ("S", cellRendererToggle, "active", colNr);
		}

		#endregion CreateColumn
	}
}
