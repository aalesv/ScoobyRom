// DataView2DGtk.cs: Gtk.TreeView based UI.

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


using Gtk;

namespace ScoobyRom
{
	public sealed class DataView2DGtk : DataViewBaseGtk
	{
		static ColumnNr2D[] ColumnsOrder = new ColumnNr2D[] {
			ColumnNr2D.Category,
			ColumnNr2D.Toggle,
			ColumnNr2D.Icon,
			ColumnNr2D.Title,
			ColumnNr2D.UnitY,
			ColumnNr2D.Type,
			ColumnNr2D.Location,
			ColumnNr2D.NameX,
			ColumnNr2D.UnitX,
			ColumnNr2D.CountX,
			ColumnNr2D.Xmin,
			ColumnNr2D.Xmax,
			ColumnNr2D.Ymin,
			ColumnNr2D.Yavg,
			ColumnNr2D.Ymax,
			ColumnNr2D.Multiplier,
			ColumnNr2D.Offset,
			ColumnNr2D.XPos,
			ColumnNr2D.YPos,
			ColumnNr2D.Description,
			ColumnNr2D.Obj
		};

		private DataView2DGtk ()
		{
		}

		public DataView2DGtk (DataView2DModelGtk viewModel, TreeView treeView)
		{
			this.viewModel = viewModel;
			this.treeModel = viewModel.TreeModel;
			this.treeView = treeView;

			InitTreeView ();

			treeView.KeyPressEvent += TreeView_KeyPressEvent;
		}

		protected override int ColumnNrIcon {
			get { return (int)ColumnNr2D.Icon; }
		}

		protected override int ColumnNrObj {
			get { return (int)ColumnNr2D.Obj; }
		}

		void InitTreeView ()
		{
			InitCellRenderers ();

			#region Columns

			// must be appended/inserted in correct order
			foreach (ColumnNr2D colNr in ColumnsOrder) {
				TreeViewColumn column = CreateColumn (colNr);
				// null means column is not being used on view
				if (column == null)
					continue;

				columnsDict.Add (column, (int)colNr);

				if (column.SortColumnId < 0 && colNr != ColumnNr2D.Icon)
					column.SortColumnId = (int)colNr;

				column.Reorderable = true;
				column.Resizable = true;
			}
			AjustIconCol ();

			#endregion Columns


			#region TreeView

			treeView.Selection.Mode = SelectionMode.Browse;
			treeView.RulesHint = true;
			treeView.EnableSearch = true;
			treeView.SearchColumn = (int)ColumnNr2D.Title;
			treeView.SearchEqualFunc = TreeViewSearchFunc;
			treeView.CursorChanged += OnCursorChanged;
			treeView.RowActivated += HandleTreeViewRowActivated;
			treeView.Model = treeModel;

			#endregion TreeView

		}

		void InitCellRenderers ()
		{
			cellRendererText = new CellRendererText ();

			cellRendererTextEditable = new CellRendererText ();
			cellRendererTextEditable.Editable = true;
			cellRendererTextEditable.Edited += HandleCellRendererTextEditableEdited;

			cellRendererToggle = new CellRendererToggle ();
			cellRendererToggle.Toggled += CellRendererToggled;

			cellRendererPixbuf = new CellRendererPixbuf ();

			cellRendererCombo = new CellRendererCombo ();
			cellRendererCombo.HasEntry = false;
			cellRendererCombo.Editable = true;
			cellRendererCombo.Model = tableTypesModel;
			cellRendererCombo.TextColumn = 0;
			cellRendererCombo.Edited += HandleCellRendererComboEdited;
		}

		TreeViewColumn CreateColumn (ColumnNr2D colNr)
		{
			TreeViewColumn col = null;
			switch (colNr) {
			case ColumnNr2D.Toggle:
				col = CreateToggleColumn ((int)colNr);
				break;
			case ColumnNr2D.Icon:
				col = CreateIconColumn ((int)colNr);
				break;
			case ColumnNr2D.Type:
				col = CreateTypeColumn ((int)colNr);
				break;
			case ColumnNr2D.Category:
				col = CreateTextEditableColumn ("Category", (int)colNr);
				break;
			case ColumnNr2D.Title:
				col = CreateTextEditableColumn ("Title", (int)colNr);
				break;
			case ColumnNr2D.NameX:
				col = CreateTextEditableColumn ("NameX", (int)colNr);
				break;
			case ColumnNr2D.UnitX:
				col = CreateTextEditableColumn ("UnitX", (int)colNr);
				break;
			case ColumnNr2D.UnitY:
				col = CreateTextEditableColumn ("UnitY", (int)colNr);
				break;
			case ColumnNr2D.Description:
				col = CreateTextEditableColumn ("Description", (int)colNr);
				break;
			case ColumnNr2D.CountX:
				col = CreateTextColumn ("Count", (int)colNr);
				break;
			case ColumnNr2D.Xmin:
				col = CreateFloatColumn ("Xmin", (int)colNr);
				break;
			case ColumnNr2D.Xmax:
				col = CreateFloatColumn ("Xmax", (int)colNr);
				break;
			case ColumnNr2D.Ymin:
				col = CreateFloatColumn ("Ymin", (int)colNr);
				break;
			case ColumnNr2D.Yavg:
				col = CreateFloatColumn ("Yavg", (int)colNr);
				break;
			case ColumnNr2D.Ymax:
				col = CreateFloatColumn ("Ymax", (int)colNr);
				break;
			case ColumnNr2D.Multiplier:
				col = CreateFloatColumn ("Multiplier", (int)colNr);
				break;
			case ColumnNr2D.Offset:
				col = CreateFloatColumn ("Offset", (int)colNr);
				break;
			case ColumnNr2D.Location:
				col = CreateHexColumn ("Record", (int)colNr);
				break;
			case ColumnNr2D.XPos:
				col = CreateHexColumn ("XPos", (int)colNr);
				break;
			case ColumnNr2D.YPos:
				col = CreateHexColumn ("YPos", (int)colNr);
				break;
			}
			if (col != null)
				treeView.AppendColumn (col);
			return col;
		}

		// It seems if using own TreeViewSearchEqualFunc one cannot use default function anymore.
		// Function getter returns null and null is not allowed.
		// Workaround: define own functions for all needed column types.

		// FALSE if the row does MATCH, otherwise true !!!
		bool TreeViewSearchFunc (TreeModel model, int column, string key, TreeIter iter)
		{
			object content = model.GetValue (iter, column);

			GLib.GType gt = model.GetColumnType (column);
			if (gt == GLib.GType.Float)
				return EqualFuncFloat (key, (float)content);
			else if (gt == GLib.GType.String)
				return EqualFuncString (key, (string)content);

			// type int needs further info!
			switch ((ColumnNr2D)column) {
			case ColumnNr2D.Location:
			case ColumnNr2D.XPos:
			case ColumnNr2D.YPos:
				return EqualFuncHex (key, (int)content);
			case ColumnNr2D.Type:
				return EqualFuncTableType (key, (Tables.TableType)content);
			case ColumnNr2D.CountX:
				return EqualFuncInt (key, (int)content);
			default:
				// cannot search on icon column, must signal true = no match.
				return true;
			}
		}
	}
}
