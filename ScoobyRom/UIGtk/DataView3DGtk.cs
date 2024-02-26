// DataView3DGtk.cs: Gtk.TreeView based UI.

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
	public sealed class DataView3DGtk : DataViewBaseGtk
	{
		static ColumnNr3D[] ColumnsOrder = new ColumnNr3D[] {
			ColumnNr3D.Category,
			ColumnNr3D.Toggle,
			ColumnNr3D.Icon,
			ColumnNr3D.Title,
			ColumnNr3D.UnitZ,
			ColumnNr3D.Type,
			ColumnNr3D.Location,
			ColumnNr3D.NameX,
			ColumnNr3D.UnitX,
			ColumnNr3D.NameY,
			ColumnNr3D.UnitY,
			ColumnNr3D.CountX,
			ColumnNr3D.CountY,
			ColumnNr3D.CountZ,
			ColumnNr3D.Xmin,
			ColumnNr3D.Xmax,
			ColumnNr3D.Ymin,
			ColumnNr3D.Ymax,
			ColumnNr3D.Zmin,
			ColumnNr3D.Zavg,
			ColumnNr3D.Zmax,
			ColumnNr3D.Multiplier,
			ColumnNr3D.Offset,
			ColumnNr3D.XPos,
			ColumnNr3D.YPos,
			ColumnNr3D.ZPos,
			ColumnNr3D.Description,
			ColumnNr3D.Obj
		};

		private DataView3DGtk ()
		{
		}

		public DataView3DGtk (DataView3DModelGtk viewModel, TreeView treeView)
		{
			this.viewModel = viewModel;
			// not wrapping with TreeModelSort causes model to be reordered when sorting a column
			// 		event "RowsReordered" is being raised
			// However SetValue methods are not implemented in TreeModelSort,
			// 		cannot be overridden, only for read-only data!?
			//this.treeStore = new TreeModelSort (viewModel.TreeStore);
			this.treeModel = viewModel.TreeModel;

			this.treeView = treeView;

			InitTreeView ();

			treeView.KeyPressEvent += TreeView_KeyPressEvent;
		}

		public DataViewModelBaseGtk DataViewModelGtk {
			get { return this.viewModel; }
		}

		protected override int ColumnNrIcon {
			get { return (int)ColumnNr3D.Icon; }
		}

		protected override int ColumnNrObj {
			get { return (int)ColumnNr3D.Obj; }
		}

		void InitTreeView ()
		{
			InitCellRenderers ();

			#region Columns

			// must be appended/inserted in correct order
			foreach (ColumnNr3D colNr in ColumnsOrder) {
				TreeViewColumn column = CreateColumn (colNr);
				// null means column is not being used on view
				if (column == null)
					continue;

				columnsDict.Add (column, (int)colNr);

				// necessary: SortColumnId
				// Sorting on PixBuf column -> Gtk-WARNING **: Attempting to sort on invalid type GdkPixbuf
				// default: -1
				if (column.SortColumnId < 0 && colNr != ColumnNr3D.Icon)
					column.SortColumnId = (int)colNr;

				// if ... treeView.ExpanderColumn = col;

				// Reorderable: default false
				column.Reorderable = true;
				// Resizable: default false
				column.Resizable = true;

				// GrowOnly really faster than Autosize?
				// Autosize is slow!
				// column.Sizing = TreeViewColumnSizing.Autosize;
			}
			AjustIconCol ();

			#endregion Columns


			#region TreeView

			// FixedHeightMode is not feasable here though it would improve performance.
			/* Gtk+ doc: Fixed height mode speeds up GtkTreeView by assuming that all rows have the same height.
				Only enable this option if all rows are the same height and all columns are of
				 type GTK_TREE_VIEW_COLUMN_FIXED. */
			// TreeViewColumnSizing.Fixed needed is required by TreeView.FixedHeightMode.
			// treeView.FixedHeightMode = true;

			treeView.Selection.Mode = SelectionMode.Browse;
			// Allows to reorder rows in the view (this enables the internal drag and drop of TreeView rows).
			// NO EVENT???
			//treeView.Reorderable = false;
			//treeView.EnableTreeLines = true;

			// Horizontal grid lines not optimal with tree lines
			// !!! Displaying grid lines degrades performance enormously when there are lots of items !!!
			//treeView.EnableGridLines = TreeViewGridLines.Both;

			// additional indentation, defaults to 0
			//treeView.LevelIndentation = 10;

			// RulesHint: hint to the theme engine to draw rows in alternating colors
			// Sorted column usually has stronger alternating colors than the others.
			// Theme "Oxygen-Molecule" does very slight coloring (without sort)!
			// Does not seem to hurt perf
			treeView.RulesHint = true;

			// ??? Indicates if Rubberbanding multi-selection is supported.
			//treeView.RubberBanding = true;

			// TooltipColumn: works ok (no effect on PixBuf column), not needed so far
			// treeView.TooltipColumn = (int)ColumnNr3D.Unit;

			// reading treeView.SearchEqualFunc gets null and null is not allowed  (i.e. to use default one)
			treeView.EnableSearch = true;
			treeView.SearchColumn = (int)ColumnNr3D.Title;
			treeView.SearchEqualFunc = TreeViewSearchFunc;
			//treeView.HeadersClickable = true;

			// TreeView events
//			treeView.KeyPressEvent += HandleTreeViewKeyPressEvent;
//			treeView.KeyReleaseEvent += HandleTreeViewKeyReleaseEvent;
			treeView.CursorChanged += OnCursorChanged;
			//treeView.ScrollEvent += HandleTreeViewScrollEvent;
			treeView.RowActivated += HandleTreeViewRowActivated;
			//treeView.KeyPressEvent += HandleTreeViewKeyPressEvent;

			treeView.Model = treeModel;

			#endregion TreeView

		}

		void InitCellRenderers ()
		{
			cellRendererText = new CellRendererText ();
			//cellRendererText.WrapMode = Pango.WrapMode.WordChar;
			// no effect
			//cellRendererText.Alignment = Pango.Alignment.Center;

			cellRendererTextEditable = new CellRendererText ();
			cellRendererTextEditable.Editable = true;
			cellRendererTextEditable.Edited += HandleCellRendererTextEditableEdited;
			// does not update with column width !
			// might add problems e.g. scrolling up by key
			//cellRendererTextEditable.WrapWidth = 300;
			// This property has no effect unless the wrap-width property is set.
			//cellRendererTextEditable.WrapMode = Pango.WrapMode.WordChar;
			// no effect?
			//cellRendererTextEditable.SingleParagraphMode = true;



			// TODO check tri-state; bool? in TreeStore does not work
			cellRendererToggle = new CellRendererToggle ();
			// radio instead of check mark drags more attention, check mark is a bit light
			//cellRendererToggle.Radio = true;
			//cellRendererToggle.Inconsistent = true;
			// checkbox, false means read-only
			//cellRendererToggle.Activatable = true;
			// default: 12 (?)
			//cellRendererToggle.IndicatorSize = 20;
			cellRendererToggle.Toggled += CellRendererToggled;

			// Warning: icon size appears fixed, does not take DPI into account
			// Verified on both Linux and Windows via screenshot.
			cellRendererPixbuf = new CellRendererPixbuf ();
			// SetFixedSize crops bitmaps if too small
			// cellRendererPixbuf.SetFixedSize (64, 48);
			// not useful here, follows mouse, darkens icon; default is false
			// cellRendererPixbuf.FollowState = true;

			cellRendererCombo = new CellRendererCombo ();
			// only pre-defined types are allowed
			cellRendererCombo.HasEntry = false;
			cellRendererCombo.Editable = true;
			cellRendererCombo.Model = tableTypesModel;
			//cellRendererCombo.Mode = CellRendererMode.Editable;
			cellRendererCombo.TextColumn = 0;
			cellRendererCombo.Edited += HandleCellRendererComboEdited;
		}

		TreeViewColumn CreateColumn (ColumnNr3D colNr)
		{
			TreeViewColumn col = null;
			// new TreeViewColumn (...):
			// 		Property name is necessary and must be lower case, specified via attributes, see doc!
			// 		Column number at end is necessary, otherwise content not being displayed.
			// "markup" instead of "text" allows e.g. "<i>italic</i>"
			switch (colNr) {
			case ColumnNr3D.Toggle:
				col = CreateToggleColumn ((int)colNr);
				break;
			case ColumnNr3D.Icon:
				col = CreateIconColumn ((int)colNr);
				break;
			case ColumnNr3D.Type:
				col = CreateTypeColumn ((int)colNr);
				break;
			case ColumnNr3D.Title:
				col = CreateTextEditableColumn ("Title", (int)colNr);
				break;
			case ColumnNr3D.Category:
				col = CreateTextEditableColumn ("Category", (int)colNr);
				break;
			case ColumnNr3D.NameX:
				col = CreateTextEditableColumn ("NameX", (int)colNr);
				break;
			case ColumnNr3D.NameY:
				col = CreateTextEditableColumn ("NameY", (int)colNr);
				break;
			case ColumnNr3D.UnitX:
				col = CreateTextEditableColumn ("UnitX", (int)colNr);
				break;
			case ColumnNr3D.UnitY:
				col = CreateTextEditableColumn ("UnitY", (int)colNr);
				break;
			case ColumnNr3D.UnitZ:
				col = CreateTextEditableColumn ("UnitZ", (int)colNr);
				break;
			case ColumnNr3D.Description:
				col = CreateTextEditableColumn ("Description", (int)colNr);
				break;
			case ColumnNr3D.CountX:
				col = CreateTextColumn ("CX", (int)colNr);
				break;
			case ColumnNr3D.CountY:
				col = CreateTextColumn ("CY", (int)colNr);
				break;
			case ColumnNr3D.CountZ:
				col = CreateTextColumn ("CZ", (int)colNr);
				break;
			case ColumnNr3D.Xmin:
				col = CreateFloatColumn ("Xmin", (int)colNr);
				break;
			case ColumnNr3D.Xmax:
				col = CreateFloatColumn ("Xmax", (int)colNr);
				break;
			case ColumnNr3D.Ymin:
				col = CreateFloatColumn ("Ymin", (int)colNr);
				break;
			case ColumnNr3D.Ymax:
				col = CreateFloatColumn ("Ymax", (int)colNr);
				break;
			case ColumnNr3D.Zmin:
				col = CreateFloatColumn ("Zmin", (int)colNr);
				break;
			case ColumnNr3D.Zavg:
				col = CreateFloatColumn ("Zavg", (int)colNr);
				break;
			case ColumnNr3D.Zmax:
				col = CreateFloatColumn ("Zmax", (int)colNr);
				break;
			case ColumnNr3D.Multiplier:
				col = CreateFloatColumn ("Multiplier", (int)colNr);
				break;
			case ColumnNr3D.Offset:
				col = CreateFloatColumn ("Offset", (int)colNr);
				break;
			case ColumnNr3D.Location:
				col = CreateHexColumn ("Record", (int)colNr);
				break;
			case ColumnNr3D.XPos:
				col = CreateHexColumn ("XPos", (int)colNr);
				break;
			case ColumnNr3D.YPos:
				col = CreateHexColumn ("YPos", (int)colNr);
				break;
			case ColumnNr3D.ZPos:
				col = CreateHexColumn ("ZPos", (int)colNr);
				break;
			}
			// only show columns supposed to be displayed
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
			switch ((ColumnNr3D)column) {
			case ColumnNr3D.Location:
			case ColumnNr3D.XPos:
			case ColumnNr3D.YPos:
			case ColumnNr3D.ZPos:
				return EqualFuncHex (key, (int)content);
			case ColumnNr3D.Type:
				return EqualFuncTableType (key, (Tables.TableType)content);
			case ColumnNr3D.CountX:
			case ColumnNr3D.CountY:
			case ColumnNr3D.CountZ:
				return EqualFuncInt (key, (int)content);
			default:
				// cannot search on icon column, must return true = no match.
				return true;
			}
		}
	}
}
