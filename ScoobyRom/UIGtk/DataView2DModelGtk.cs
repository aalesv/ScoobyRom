// DataView2DModelGtk.cs: Gtk.TreeModel for UI.

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
using Gtk;
using Tables.Denso;

namespace ScoobyRom
{
	// sort of ViewModel in M-V-VM (Model-View-ViewModel pattern)
	public sealed class DataView2DModelGtk : DataViewModelBaseGtk
	{
		public DataView2DModelGtk (Data data, int iconWidth, int iconHeight) : base (data, new PlotIcon2D (iconWidth, iconHeight))
		{
			data.ItemsChanged2D += OnDataItemsChanged;
		}

		protected override int ColumnNrIcon {
			get { return (int)ColumnNr2D.Icon; }
		}

		protected override int ColumnNrObj {
			get { return (int)ColumnNr2D.Obj; }
		}

		protected override int ColumnNrToggle {
			get { return (int)ColumnNr2D.Toggle; }
		}

		public void ChangeTableType (Table2D table2D, Tables.TableType newType)
		{
			data.ChangeTableType (table2D, newType);
		}

		override protected void InitStore ()
		{
			// TODO avoid reflection
			int count = ((ColumnNr2D[])Enum.GetValues (typeof(ColumnNr2D))).Length;
			// WARNING: crashes when an array slot wasn't initialized
			Type[] types = new Type[count];

			// using enum in the store --> Gtk-WARNING **: Attempting to sort on invalid type GtkSharpValue
			// --> use int instead

			types [(int)ColumnNr2D.Category] = typeof(string);
			types [(int)ColumnNr2D.Toggle] = typeof(bool);
			types [(int)ColumnNr2D.Icon] = typeof(Gdk.Pixbuf);
			types [(int)ColumnNr2D.Title] = typeof(string);
			types [(int)ColumnNr2D.Type] = typeof(int);
			types [(int)ColumnNr2D.UnitY] = typeof(string);

			types [(int)ColumnNr2D.NameX] = typeof(string);
			types [(int)ColumnNr2D.UnitX] = typeof(string);

			types [(int)ColumnNr2D.CountX] = typeof(int);

			types [(int)ColumnNr2D.Xmin] = typeof(float);
			types [(int)ColumnNr2D.Xmax] = typeof(float);
			types [(int)ColumnNr2D.Ymin] = typeof(float);
			types [(int)ColumnNr2D.Yavg] = typeof(float);
			types [(int)ColumnNr2D.Ymax] = typeof(float);

			types [(int)ColumnNr2D.Multiplier] = typeof(float);
			types [(int)ColumnNr2D.Offset] = typeof(float);

			types [(int)ColumnNr2D.Location] = typeof(int);
			types [(int)ColumnNr2D.XPos] = typeof(int);
			types [(int)ColumnNr2D.YPos] = typeof(int);

			types [(int)ColumnNr2D.Description] = typeof(string);

			types [(int)ColumnNr2D.Obj] = typeof(object);

			store = new ListStore (types);

			// not called on TreeView-built-in reorder! called a lot when re-populating store
			//store.RowsReordered += HandleTreeStoreRowsReordered;
			store.RowChanged += HandleTreeStoreRowChanged;
		}

		override protected void PopulateData ()
		{
			// performance, would get raised for each new row
			SetHandleRowChanged (false);

			foreach (Table2D table2D in data.List2D) {
				TreeIter newNode = store.Append ();
				SetNodeContent (newNode, table2D);
			}

			SetHandleRowChanged (true);
		}

		public void SetNodeContent (TreeIter iter, Table2D table2D)
		{
			store.SetValue (iter, (int)ColumnNr2D.Obj, table2D);

			store.SetValue (iter, (int)ColumnNr2D.Category, table2D.Category);
			store.SetValue (iter, (int)ColumnNr2D.Toggle, false);
			store.SetValue (iter, (int)ColumnNr2D.Title, table2D.Title);
			store.SetValue (iter, (int)ColumnNr2D.UnitY, table2D.UnitY);

			store.SetValue (iter, (int)ColumnNr2D.NameX, table2D.NameX);
			store.SetValue (iter, (int)ColumnNr2D.UnitX, table2D.UnitX);

			store.SetValue (iter, (int)ColumnNr2D.CountX, table2D.CountX);

			store.SetValue (iter, (int)ColumnNr2D.Xmin, table2D.Xmin);
			store.SetValue (iter, (int)ColumnNr2D.Xmax, table2D.Xmax);

			store.SetValue (iter, (int)ColumnNr2D.Multiplier, table2D.Multiplier);
			store.SetValue (iter, (int)ColumnNr2D.Offset, table2D.Offset);

			store.SetValue (iter, (int)ColumnNr2D.Location, table2D.Location);
			store.SetValue (iter, (int)ColumnNr2D.XPos, table2D.RangeX.Pos);
			store.SetValue (iter, (int)ColumnNr2D.YPos, table2D.RangeY.Pos);
			store.SetValue (iter, (int)ColumnNr2D.Description, table2D.Description);
			Toggle (iter, table2D.Selected);

			SetNodeContentTypeChanged (iter, table2D);
		}

		public override void SetNodeContentTypeChanged (TreeIter iter, Tables.Denso.Table table)
		{
			var t = (Table2D)table;
			store.SetValue (iter, (int)ColumnNr2D.Type, (int)t.TableType);
			store.SetValue (iter, (int)ColumnNr2D.Ymin, t.Ymin);
			store.SetValue (iter, (int)ColumnNr2D.Yavg, t.Yavg);
			store.SetValue (iter, (int)ColumnNr2D.Ymax, t.Ymax);

			if (iconsVisible)
				CreateSetNewIcon (iter, t);
		}

		protected override void UpdateModel (TreeIter iter)
		{
			// prevent loop when content might change in here (i.e. toggle)
			SetHandleRowChanged (false);

			Table2D table = store.GetValue (iter, (int)ColumnNr2D.Obj) as Table2D;
			if (table == null)
				return;

			string nameX = (string)store.GetValue (iter, (int)ColumnNr2D.NameX);
			string unitX = (string)store.GetValue (iter, (int)ColumnNr2D.UnitX);

			nameX = nameX.Trim ();
			unitX = unitX.Trim ();
			if (unitX != table.UnitX || nameX != table.NameX) {
				var shared = data.FindTablesSameAxisX (table);
				if (shared.Count > 0) {
					Console.WriteLine ("AxisX shared {0} times.", shared.Count);

					#if SelectShared

					ToggleAll (false);
					foreach (var t in shared) {
						t.Selected = true;
						TreeIter iterDup;
						if (FindIter (t, out iterDup)) {
							Toggle (iterDup, true);
						}
					}

					#endif
				}
			}

			table.Category = (string)store.GetValue (iter, (int)ColumnNr2D.Category);
			table.Title = (string)store.GetValue (iter, (int)ColumnNr2D.Title);
			table.UnitY = (string)store.GetValue (iter, (int)ColumnNr2D.UnitY);
			table.NameX = nameX;
			table.UnitX = unitX;
			table.Description = (string)store.GetValue (iter, (int)ColumnNr2D.Description);
			table.Selected = IsToggled (iter);

			SetHandleRowChanged (true);
		}
	}
}