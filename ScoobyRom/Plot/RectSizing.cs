// RectSizing.cs: Rectangle (bitmap) size functionality.

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
using System.Drawing;

namespace ScoobyRom
{
	public sealed class RectSizing
	{
		const double ZoomFactorMin = 1.1;
		const double ZoomFactorMax = 2.0;
		const int WidthMin = 24;
		const int HeightMin = 16;

		double zoomFactor = 1.2;
		int width, height, originalWidth;
		System.Drawing.Rectangle bounds;
		/// <summary>
		/// height / width
		/// </summary>
		double aspectRatio;

		public RectSizing () : this (48, 32)
		{
		}

		public RectSizing (int width, int height)
		{
			this.aspectRatio = width / (double)height;
			this.originalWidth = width;
			Calc (width, height);
		}

		public double ZoomFactor {
			get { return zoomFactor; }
			set {
				if (value > ZoomFactorMax && value < ZoomFactorMin)
					return;
				zoomFactor = value;
			}
		}

		public int Width {
			get { return width; }
		}

		public int Height {
			get { return height; }
		}

		public Rectangle Bounds {
			get { return bounds; }
		}

		public void ZoomIn ()
		{
			CalcFromNewWidth (zoomFactor * width);
		}

		public void ZoomOut ()
		{
			CalcFromNewWidth (1.0 / zoomFactor * width);
		}

		public void ZoomReset ()
		{
			CalcFromNewWidth (originalWidth);
		}

		void CalcFromNewWidth (double newWidth)
		{
			int w = Convert.ToInt32 (newWidth);
			int h = Convert.ToInt32 (newWidth / aspectRatio);
			Calc (w, h);
		}

		void Calc (int width, int height)
		{
			if (width < WidthMin) {
				this.width = WidthMin;
				this.height = Convert.ToInt32 (this.width / aspectRatio);
			} else {
				this.width = width;
			}
			if (height < HeightMin) {
				this.height = HeightMin;
				this.width = Convert.ToInt32 (this.height * aspectRatio);
			} else {
				this.height = height;
			}
			this.bounds = new System.Drawing.Rectangle (0, 0, this.width, this.height);
		}
	}
}
