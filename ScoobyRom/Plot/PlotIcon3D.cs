// PlotIcon3D.cs: Create color bitmaps using Florence/NPlot.

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


using Florence;

namespace ScoobyRom
{
	/// <summary>
	/// Creates Florence/NPlot ImagePlots (heatmap) without any annotation, useful for icons.
	/// Methods are not thread safe!
	/// </summary>
	public sealed class PlotIcon3D : PlotIconBase
	{
		public PlotIcon3D (int width, int height) : base (width, height)
		{
		}

		public override Gdk.Pixbuf CreateIcon (Tables.Denso.Table table)
		{
			var t = (Tables.Denso.Table3D)table;

			if (t.IsDataConst)
				return ConstDataIcon;

			Plot3D.Draw (plotSurface, t);
			return DrawAndConvert ();
		}
	}
}