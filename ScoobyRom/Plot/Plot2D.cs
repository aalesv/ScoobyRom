// Plot2D.cs: Draw line graph using NPlot interface. Does not depend on UI.

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


using System.Drawing;
using Florence;

namespace ScoobyRom
{
	public sealed class Plot2D
	{
		const float PenWidth = 3f;
		const int MarkerSize = 6;

		// not specifying results in SmoothingMode.Default = no antialiasing!
		const System.Drawing.Drawing2D.SmoothingMode SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

		readonly InteractivePlotSurface2D plotSurface2D;

		readonly Font titleFont = new Font (FontFamily.GenericSansSerif, 14, GraphicsUnit.Point);
		readonly Font labelFont = new Font (FontFamily.GenericSansSerif, 13, GraphicsUnit.Point);
		readonly Font tickTextFont = new Font (FontFamily.GenericSansSerif, 12, GraphicsUnit.Point);

		readonly Marker marker;
		readonly Pen pen;

		public Plot2D (InteractivePlotSurface2D plotSurface)
		{
			this.plotSurface2D = plotSurface;

			pen = new Pen (Color.Red, PenWidth);
			marker = new Marker (Marker.MarkerType.FilledCircle, MarkerSize, Color.Blue);
		}

		/// <summary>
		/// Might need Refresh () afterwards!
		/// </summary>
		/// <param name="table2D">
		/// A <see cref="Table2D"/>
		/// </param>
		public void Draw (Tables.Denso.Table2D table2D)
		{
			float[] valuesY = table2D.GetValuesYasFloats ();

			// clear everything. reset fonts. remove plot components etc.
			// including Florence interactions
			this.plotSurface2D.Clear ();

			// Florence interactions, N/A in original NPlot library
			// guideline disadvantage: not optimized - does not use bitmap buffer, refreshes every time a line has to move
			plotSurface2D.AddInteraction (new VerticalGuideline (Color.Gray));
			plotSurface2D.AddInteraction (new HorizontalGuideline (Color.Gray));

			//plotSurface2D.AddInteraction (new PlotSelection (Color.Green));
			plotSurface2D.AddInteraction (new PlotDrag (true, true));

			plotSurface2D.AddInteraction (new AxisDrag ());
			// PlotZoom: mouse wheel zoom
			plotSurface2D.AddInteraction (new PlotZoom ());
			plotSurface2D.AddInteraction (new KeyActions ());

			plotSurface2D.SurfacePadding = 0;
			plotSurface2D.SmoothingMode = SmoothingMode;

			// y-values, x-values (!)
			LinePlot lp = new LinePlot (valuesY, table2D.ValuesX);
			lp.Pen = pen;

			PointPlot pp = new PointPlot (marker);
			pp.AbscissaData = table2D.ValuesX;
			pp.OrdinateData = valuesY;

			Grid myGrid = new Grid ();
			myGrid.VerticalGridType = Grid.GridType.Coarse;
			myGrid.HorizontalGridType = Grid.GridType.Coarse;

			plotSurface2D.Add (myGrid);
			plotSurface2D.Add (pp);
			plotSurface2D.Add (lp);

			plotSurface2D.TitleFont = titleFont;
			plotSurface2D.Title = table2D.Title;

			plotSurface2D.XAxis1.LabelFont = labelFont;
			plotSurface2D.XAxis1.Label = AxisText (table2D.NameX, table2D.UnitX);
			// could use ex: plotSurface2D.YAxis1.NumberFormat = "0.000";
			plotSurface2D.XAxis1.TickTextFont = tickTextFont;

			plotSurface2D.YAxis1.LabelFont = labelFont;
			plotSurface2D.YAxis1.Label = AxisText (table2D.Title, table2D.UnitY);
			plotSurface2D.YAxis1.TickTextFont = tickTextFont;

			// Florence surface has Refresh () method vs. NPlot: Refresh () not part of surface interface
			plotSurface2D.Refresh ();
		}

		// "Axisname [Unit]"
		static string AxisText (string name, string unit)
		{
			if (string.IsNullOrEmpty (name) && string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("{0} [{1}]", name, unit);
		}
	}
}
