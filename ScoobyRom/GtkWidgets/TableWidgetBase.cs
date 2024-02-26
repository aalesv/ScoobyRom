// TableWidgetBase.cs: Builds a Gtk.Table showing table data values.

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
	public abstract class TableWidgetBase
	{
		const int DataColLeft = 1;
		const int DataRowTop = 1;

		protected int countX, cols, rows;
		protected string axisXMarkup = "X Axis [-]";
		protected string valuesMarkup = "Y Axis [-]";
		protected string formatValues = "0.000";
		protected readonly float[] axisX, values;
		protected readonly float axisXmin, axisXmax, valuesMax, valuesMin;
		protected readonly Util.Coloring coloring;

		// could also use LINQ to calc min/max since performance does not matter here
		public TableWidgetBase (Util.Coloring coloring, float[] axisX, float[] values, float axisXmin, float axisXmax, float valuesMin, float valuesMax)
		{
			this.coloring = coloring;
			this.axisX = axisX;
			this.axisXmin = axisXmin;
			this.axisXmax = axisXmax;
			this.values = values;
			this.valuesMin = valuesMin;
			this.valuesMax = valuesMax;
			this.countX = this.axisX.Length;
		}

		public string AxisXMarkup {
			get { return this.axisXMarkup; }
			set { axisXMarkup = value; }
		}

		public string ValuesMarkup {
			get { return this.valuesMarkup; }
			set { valuesMarkup = value; }
		}

		public string HeaderAxisMarkup { get; set; }

		public string HeaderValuesMarkup { get; set; }

		public string FormatValues {
			get { return this.formatValues; }
			set { formatValues = value; }
		}

		public abstract Gtk.Widget Create ();

		protected Cairo.Color CalcValueColor (float val)
		{
			double factor = (val - valuesMin) / (valuesMax - valuesMin);
			// should be able to handle division by zero (NaN)
			return coloring.GetColor (factor);
		}

		protected Cairo.Color CalcAxisXColor (float val)
		{
			double factor = (val - axisXmin) / (axisXmax - axisXmin);
			// should be able to handle division by zero (NaN)
			return coloring.GetColor (factor);
		}
	}
}