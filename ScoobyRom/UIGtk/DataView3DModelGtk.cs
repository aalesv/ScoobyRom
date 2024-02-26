// DataView3DModelGtk.cs: Gtk.TreeModel for UI.

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
	public sealed class DataView3DModelGtk : DataViewModelBaseGtk
	{
		public DataView3DModelGtk (Data data, int iconWidth, int iconHeight) : base (data, new PlotIcon3D (iconWidth, iconHeight))
		{
			data.ItemsChanged3D += OnDataItemsChanged;
		}

		protected override int ColumnNrIcon {
			get { return (int)ColumnNr3D.Icon; }
		}

		protected override int ColumnNrObj {
			get { return (int)ColumnNr3D.Obj; }
		}

		protected override int ColumnNrToggle {
			get { return (int)ColumnNr3D.Toggle; }
		}

		override protected void InitStore ()
		{
			// TODO avoid reflection
			int count = ((ColumnNr3D[])Enum.GetValues (typeof(ColumnNr3D))).Length;
			// WARNING: crashes when an array slot wasn't initialized
			Type[] types = new Type[count];

			// using enum in the store --> Gtk-WARNING **: Attempting to sort on invalid type GtkSharpValue
			// --> use int instead


			types [(int)ColumnNr3D.Category] = typeof(string);
			types [(int)ColumnNr3D.Toggle] = typeof(bool);
			types [(int)ColumnNr3D.Icon] = typeof(Gdk.Pixbuf);
			types [(int)ColumnNr3D.Title] = typeof(string);
			types [(int)ColumnNr3D.Type] = typeof(int);
			types [(int)ColumnNr3D.Location] = typeof(int);

			types [(int)ColumnNr3D.NameX] = typeof(string);
			types [(int)ColumnNr3D.NameY] = typeof(string);

			types [(int)ColumnNr3D.UnitX] = typeof(string);
			types [(int)ColumnNr3D.UnitY] = typeof(string);
			types [(int)ColumnNr3D.UnitZ] = typeof(string);

			types [(int)ColumnNr3D.CountX] = typeof(int);
			types [(int)ColumnNr3D.CountY] = typeof(int);
			types [(int)ColumnNr3D.CountZ] = typeof(int);

			types [(int)ColumnNr3D.Xmin] = typeof(float);
			types [(int)ColumnNr3D.Xmax] = typeof(float);
			types [(int)ColumnNr3D.Ymin] = typeof(float);
			types [(int)ColumnNr3D.Ymax] = typeof(float);
			types [(int)ColumnNr3D.Zmin] = typeof(float);
			types [(int)ColumnNr3D.Zavg] = typeof(float);
			types [(int)ColumnNr3D.Zmax] = typeof(float);

			types [(int)ColumnNr3D.Multiplier] = typeof(float);
			types [(int)ColumnNr3D.Offset] = typeof(float);

			types [(int)ColumnNr3D.XPos] = typeof(int);
			types [(int)ColumnNr3D.YPos] = typeof(int);
			types [(int)ColumnNr3D.ZPos] = typeof(int);

			types [(int)ColumnNr3D.Description] = typeof(string);

			types [(int)ColumnNr3D.Obj] = typeof(object);

			store = new ListStore (types);

			// not called on TreeView-built-in reorder! called a lot when re-populating store
			//store.RowsReordered += HandleTreeStoreRowsReordered;
			store.RowChanged += HandleTreeStoreRowChanged;
		}

		override protected void PopulateData ()
		{
			// performance, would get raised for each new row
			SetHandleRowChanged (false);
			TreeIter newNode;

			foreach (var table3D in data.List3D) {
				// TreeStore: newNode = store.AppendNode ();
				// ListStore:
				newNode = store.Append ();
				SetNodeContent (newNode, table3D);
			}

			SetHandleRowChanged (true);
		}

		public void SetNodeContent (TreeIter iter, Table3D table3D)
		{
			// TODO optimize when columns are final

			store.SetValue (iter, (int)ColumnNr3D.Obj, table3D);

			store.SetValue (iter, (int)ColumnNr3D.Category, table3D.Category);
			store.SetValue (iter, (int)ColumnNr3D.Toggle, false);
			store.SetValue (iter, (int)ColumnNr3D.Title, table3D.Title);
			store.SetValue (iter, (int)ColumnNr3D.UnitZ, table3D.UnitZ);

			store.SetValue (iter, (int)ColumnNr3D.NameX, table3D.NameX);
			store.SetValue (iter, (int)ColumnNr3D.NameY, table3D.NameY);
			store.SetValue (iter, (int)ColumnNr3D.UnitX, table3D.UnitX);
			store.SetValue (iter, (int)ColumnNr3D.UnitY, table3D.UnitY);

			store.SetValue (iter, (int)ColumnNr3D.CountX, table3D.CountX);
			store.SetValue (iter, (int)ColumnNr3D.CountY, table3D.CountY);
			store.SetValue (iter, (int)ColumnNr3D.CountZ, table3D.CountZ);

			store.SetValue (iter, (int)ColumnNr3D.Xmin, table3D.Xmin);
			store.SetValue (iter, (int)ColumnNr3D.Xmax, table3D.Xmax);
			store.SetValue (iter, (int)ColumnNr3D.Ymin, table3D.Ymin);
			store.SetValue (iter, (int)ColumnNr3D.Ymax, table3D.Ymax);

			store.SetValue (iter, (int)ColumnNr3D.Multiplier, table3D.Multiplier);
			store.SetValue (iter, (int)ColumnNr3D.Offset, table3D.Offset);

			store.SetValue (iter, (int)ColumnNr3D.XPos, table3D.RangeX.Pos);
			store.SetValue (iter, (int)ColumnNr3D.YPos, table3D.RangeY.Pos);
			store.SetValue (iter, (int)ColumnNr3D.ZPos, table3D.RangeZ.Pos);
			store.SetValue (iter, (int)ColumnNr3D.Location, table3D.Location);
			store.SetValue (iter, (int)ColumnNr3D.Description, table3D.Description);
			Toggle (iter, table3D.Selected);

			SetNodeContentTypeChanged (iter, table3D);
		}

		public override void SetNodeContentTypeChanged (TreeIter iter, Tables.Denso.Table table)
		{
			var t = (Table3D)table;
			store.SetValue (iter, (int)ColumnNr3D.Type, (int)t.TableType);
			store.SetValue (iter, (int)ColumnNr3D.Zmin, t.Zmin);
			store.SetValue (iter, (int)ColumnNr3D.Zavg, t.Zavg);
			store.SetValue (iter, (int)ColumnNr3D.Zmax, t.Zmax);
			if (iconsVisible)
				CreateSetNewIcon (iter, t);
		}

		protected override void UpdateModel (TreeIter iter)
		{
			Table3D table = store.GetValue (iter, (int)ColumnNr3D.Obj) as Table3D;
			if (table == null)
				return;
			table.Category = (string)store.GetValue (iter, (int)ColumnNr3D.Category);
			table.Title = (string)store.GetValue (iter, (int)ColumnNr3D.Title);
			table.UnitZ = (string)store.GetValue (iter, (int)ColumnNr3D.UnitZ);
			table.NameX = (string)store.GetValue (iter, (int)ColumnNr3D.NameX);
			table.UnitX = (string)store.GetValue (iter, (int)ColumnNr3D.UnitX);
			table.NameY = (string)store.GetValue (iter, (int)ColumnNr3D.NameY);
			table.UnitY = (string)store.GetValue (iter, (int)ColumnNr3D.UnitY);
			table.Description = (string)store.GetValue (iter, (int)ColumnNr3D.Description);
			table.Selected = IsToggled (iter);
		}
	}
}