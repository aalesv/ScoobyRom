// TableWidget3D.cs: Builds a Gtk.Table showing 3D table data values.

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

namespace GtkWidgets
{
	public sealed class TableWidget3D : TableWidgetBase
	{
		const int DataColLeft = 2;
		const int DataRowTop = 3;

		readonly int countY;
		string titleMarkup;
		string axisYMarkup = "Y Axis [-]";
		readonly float[] axisY;
		readonly float axisYmin, axisYmax;

		/// <summary>
		///	Create Gtk.Table visualising 3D table data.
		/// </summary>
		public TableWidget3D (Util.Coloring coloring, float[] axisX, float[] axisY, float[] valuesZ,
		                      float axisXmin, float axisXmax, float axisYmin, float axisYmax, float valuesZmin, float valuesZmax)
			: base (coloring, axisX, valuesZ, axisXmin, axisXmax, valuesZmin, valuesZmax)
		{
			this.axisY = axisY;

			this.axisYmin = axisYmin;
			this.axisYmax = axisYmax;

			this.countY = this.axisY.Length;

			if (axisX.Length * axisY.Length != valuesZ.Length)
				throw new ArgumentException ("x.Length * y.Length != z.Length");

			this.cols = this.countX + DataColLeft;
			this.rows = this.countY + DataRowTop;
		}

		public string TitleMarkup {
			get { return this.titleMarkup; }
			set { titleMarkup = value; }
		}

		public string AxisYMarkup {
			get { return this.axisYMarkup; }
			set { axisYMarkup = value; }
		}

		public override Gtk.Widget Create ()
		{
			var table = new Gtk.Table ((uint)rows, (uint)cols, false);

			//var fontDescription = new Pango.FontDescription ();
			//fontDescription.Family = "mono";

			Gtk.Label title = new Label ();
			title.Markup = this.titleMarkup;
			// label starting at left with SetAlignment also needs AttachOptions.Fill for it to work
			title.SetAlignment (0f, 0.5f);
			table.Attach (title, 0, (uint)cols, 0, 1, AttachOptions.Fill, AttachOptions.Shrink, 0, 0);

			// add some spacing so cell content won't touch
			table.ColumnSpacing = table.RowSpacing = 0;

			const uint AxisPadX = 2;
			const uint AxisPadY = 2;

			// x axis
			for (uint i = 0; i < countX; i++) {
				Gtk.Label label = new Label ();
				float val = axisX [i];
				label.Text = val.ToString ();

				BorderWidget widget = new BorderWidget (CalcAxisXColor (val));
				widget.Add (label);

				table.Attach (widget, DataColLeft + i, DataColLeft + 1 + i, DataRowTop - 1, DataRowTop, AttachOptions.Shrink, AttachOptions.Shrink, AxisPadX, 2 * AxisPadY);
			}

			// y axis
			for (uint i = 0; i < countY; i++) {
				Gtk.Label label = new Label ();
				float val = axisY [i];
				label.Text = val.ToString ();
				label.SetAlignment (1f, 0f);

				BorderWidget widget = new BorderWidget (CalcAxisYColor (val));
				widget.Add (label);

				table.Attach (widget, DataColLeft - 1, DataColLeft, DataRowTop + i, DataRowTop + 1 + i, AttachOptions.Fill, AttachOptions.Shrink, 2 * AxisPadX, AxisPadY);
			}

			// values
			int countZ = values.Length;
			for (uint i = 0; i < countZ; i++) {
				float val = values [i];
				Gtk.Widget label = new Label (val.ToString (this.formatValues));
				//label.ModifyFont (fontDescription);

				BorderWidget widget = new BorderWidget (CalcValueColor (val));

				// ShadowType differences might be minimal
				if (val >= this.valuesMax)
					widget.ShadowType = ShadowType.EtchedOut;
				else if (val <= this.valuesMin)
					widget.ShadowType = ShadowType.EtchedIn;
				widget.Add (label);

				uint row = DataRowTop + i / (uint)this.countX;
				uint col = DataColLeft + i % (uint)this.countX;

				table.Attach (widget, col, col + 1, row, row + 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
			}

			// x axis name
			Gtk.Label titleX = new Gtk.Label ();
			titleX.Markup = "<b>" + this.axisXMarkup + "</b>";
			//titleX.SetAlignment (0.5f, 0.5f);
			table.Attach (titleX, DataColLeft, (uint)cols, DataRowTop - 2, DataRowTop - 1, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			// y axis name
			Gtk.Label titleY = new Gtk.Label ();
			// Turning on any wrap property causes 0 angle!
			//titleY.Wrap = true;
			//titleY.LineWrap = true;
			//titleY.LineWrapMode = Pango.WrapMode.WordChar;
			titleY.Angle = 90;
			titleY.Markup = "<b>" + this.axisYMarkup + "</b>";

			//titleY.SetAlignment (0.5f, 0.5f);
			table.Attach (titleY, 0, 1, DataRowTop, (uint)rows, AttachOptions.Shrink, AttachOptions.Shrink, 0, 0);

			//table.Homogeneous = true;
			return table;
		}

		Cairo.Color CalcAxisYColor (float val)
		{
			double factor = (val - axisYmin) / (axisYmax - axisYmin);
			// should be able to handle division by zero (NaN)
			return coloring.GetColor (factor);
		}
	}
}