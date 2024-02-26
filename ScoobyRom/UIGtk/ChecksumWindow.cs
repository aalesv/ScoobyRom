// ChecksumWindow.cs: Gtk.Window displaying ROM checksums and CVN.

/* Copyright (C) 2011-2017 SubaruDieselCrew
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
using Subaru;

namespace ScoobyRom
{
	public partial class ChecksumWindow : Gtk.Window
	{
		enum ColNr
		{
			Index,
			Start,
			End,
			Size,
			SumTable,
			Icon,
			SumCalc
		}

		Data data;

		ListStore store;
		readonly Dictionary<TreeViewColumn, ColNr> columnsDict = new Dictionary<TreeViewColumn, ColNr> (7);
		readonly Gdk.Pixbuf[] pixbufs = new Gdk.Pixbuf[2];

		CellRendererText cellRendererText;
		CellRendererPixbuf cellRendererPixbuf;

		public ChecksumWindow () : base (Gtk.WindowType.Toplevel)
		{
			this.Build ();
			this.Icon = MainClass.AppIcon;
			Init ();
		}

		void Init ()
		{
			// index, (start, end, size, tableSum), icon, calcSum
			store = new ListStore (typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(Gdk.Pixbuf), typeof(int));

			treeviewCSums.RulesHint = true;
			treeviewCSums.Model = store;

			var fontDesc = new Pango.FontDescription ();
			fontDesc.Family = MainClass.MonospaceFont;

			cellRendererText = new CellRendererText ();
			cellRendererText.FontDesc = fontDesc;

			cellRendererPixbuf = new CellRendererPixbuf ();

			AddTextColumn ("#", ColNr.Index);
			AddTextColumn ("Start", ColNr.Start);
			AddTextColumn ("Last", ColNr.End);
			AddTextColumn ("Size (dec)", ColNr.Size);
			AddTextColumn ("Checksum", ColNr.SumTable);
			AddColumn (new TreeViewColumn (null, cellRendererPixbuf, "pixbuf", ColNr.Icon), ColNr.Icon);
			AddTextColumn ("Calculated", ColNr.SumCalc);

			InitIcons ();
		}

		void InitIcons ()
		{
			// could use this.RenderIcon(...) but those icons can be less appealing (grey check mark, red cross)
			var image = new Gtk.Image ();
			pixbufs [0] = image.RenderIcon (Gtk.Stock.No, IconSize.SmallToolbar, null);
			pixbufs [1] = image.RenderIcon (Gtk.Stock.Yes, IconSize.SmallToolbar, null);
			image.Destroy ();
		}

		TreeViewColumn AddColumn (TreeViewColumn column, ColNr colNr)
		{
			// Reorderable: default false
			column.Reorderable = true;
			// Resizable: default false
			column.Resizable = true;

			if (colNr != ColNr.Icon)
				column.SortColumnId = (int)colNr;

			treeviewCSums.AppendColumn (column);
			columnsDict.Add (column, colNr);
			return column;
		}

		TreeViewColumn CreateTextColumn (string name, ColNr colNr)
		{
			var col = new TreeViewColumn (name, cellRendererText, "text", (int)colNr);
			col.SetCellDataFunc (cellRendererText, TreeCellDataFunc);
			return col;
		}

		TreeViewColumn AddTextColumn (string name, ColNr colNr)
		{
			return AddColumn (CreateTextColumn (name, colNr), colNr);
		}

		public void SetData (Data data)
		{
			this.data = data;
			data.RomChanged += Data_RomChanged;
			Update ();
		}

		void Data_RomChanged (object sender, EventArgs e)
		{
			Update ();
		}

		void Update ()
		{
			store.Clear ();
			if (data == null)
				return;

			try {
				var rcs = data.Rom.RomChecksumming;
				var ilist = rcs.ReadTableRecords ();
				for (int i = 0; i < ilist.Count; i++) {
					var item = ilist [i];
					int sum = rcs.CalcChecksumValue (item);
					int iconIndex = item.Checksum == sum ? 1 : 0;
					store.AppendValues (i, item.StartAddress, item.EndAddress, item.BlockSize, item.Checksum, pixbufs [iconIndex], sum);
				}

				labelCVN8.Markup = "<tt>" + RomChecksumming.CVN8Str (rcs.CalcCVN8 ()) + "</tt>";
				// pre-select for copy & paste
				labelCVN8.SelectRegion (0, -1);
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
				labelCVN8.Markup = "<b>Checksumming error.</b>";
			}
		}

		#region Tree Cell Data Functions

		// These should be fast as they are called a lot, even for measuring hidden columns.

		void TreeCellDataFunc (TreeViewColumn treeViewColumn, CellRenderer renderer, TreeModel treeModel, TreeIter iter)
		{
			// need col number to get value from store
			ColNr colNr = columnsDict [treeViewColumn];

			string formatStr;
			switch (colNr) {
			case ColNr.SumTable:
			case ColNr.SumCalc:
				formatStr = "{0:X8}";
				break;
			case ColNr.Start:
			case ColNr.End:
				formatStr = "{0,6:X}";
				break;
			case ColNr.Size:
				formatStr = "{0,9:#,###}";
				break;
			case ColNr.Index:
				formatStr = "{0,2:0}";
				break;
			default:
				formatStr = "{0}";
				break;
			}

			int nr = (int)store.GetValue (iter, (int)colNr);
			cellRendererText.Text = string.Format (formatStr, nr);
		}

		#endregion Tree Cell Data Functions
	}
}