// PlotIconBase.cs: Common functionality for drawing icons using Florence/NPlot.

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

// measured on Linux x64, mono, Release build 2015-08: ~600 ms vs. ~680 ms using MemoryStream
// --> raw conversion not worth the effort, also depends on internal bitmap data representation
// Small performance difference does not matter as background task can be used.
// Tested working fine on Linux x64 and Windows 10 x64.
//#define BitmapToPixbufConversionRaw

using System.Drawing;
using Florence;
using Gdk;

namespace ScoobyRom
{
	/// <summary>
	/// Common functionality for drawing icons using NPlot.
	/// Methods are not thread safe!
	/// </summary>
	public abstract class PlotIconBase
	{
		protected readonly RectSizing rectSizing;

		protected const int MemoryStreamCapacity = 2048;
		public const int FrameWidth = 1;
		public const int Padding = FrameWidth;

		#if !BitmapToPixbufConversionRaw
		// for conversion purposes only: System.Drawing.Bitmap -> Gdk.Pixbuf

		protected System.IO.MemoryStream memoryStream;

		// Png uses transparent background; Bmp & Gif use black background; ImageFormat.Tiff adds unneeded Exif
		// MemoryBmp makes PNG on Linux!
		// should be fast as only used temporarily for conversion
		// Linux x64 tested 2015-08: ImageFormat.Bmp is fastest
		static readonly System.Drawing.Imaging.ImageFormat imageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
		#endif

		protected int padding;
		protected Pen framePen = new Pen (System.Drawing.Color.Black, FrameWidth);
		protected Gdk.Pixbuf constDataIcon;

		// reuse objects where possible to improve performance
		protected readonly PlotSurface2D plotSurface = new PlotSurface2D ();
		protected System.Drawing.Bitmap bitmap_cache;

		public RectSizing IconSizing {
			get { return rectSizing; }
		}


		public PlotIconBase (int width, int height)
		{
			rectSizing = new RectSizing (width, height);
			Init ();
		}

		protected virtual void Init ()
		{
			// could also use pre-defined wrapper with internal bitmap: NPlot.Bitmap.PlotSurface2D
			this.bitmap_cache = new System.Drawing.Bitmap (rectSizing.Width, rectSizing.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			// black/transparent (depending on image format) frame
			this.padding = Padding;

			constDataIcon = DrawConstDataIcon ();
		}

		abstract public Gdk.Pixbuf CreateIcon (Tables.Denso.Table table);

		public void ZoomIn ()
		{
			rectSizing.ZoomIn ();
			Init ();
		}

		public void ZoomOut ()
		{
			rectSizing.ZoomOut ();
			Init ();
		}

		public void ZoomReset ()
		{
			rectSizing.ZoomReset ();
			Init ();
		}

		// reuse icon, very useful for performance as many tables have const values
		public Gdk.Pixbuf ConstDataIcon {
			get { return constDataIcon; }
		}

		protected Gdk.Pixbuf DrawConstDataIcon ()
		{
			using (var surface = new Cairo.ImageSurface (Cairo.Format.Argb32, rectSizing.Width, rectSizing.Height)) {
				using (Cairo.Context cr = new Cairo.Context (surface)) {
					// background
					cr.SetSourceRGB (0.7, 0.7, 0.7);
					cr.Paint ();

					// text
					cr.SetSourceRGB (1, 0, 0);
					// simple Cairo text API instead of Pango
					//cr.MoveTo (10, 0.3 * height);
					//cr.SetFontSize (20);
					//cr.ShowText ("const");

					using (var layout = Pango.CairoHelper.CreateLayout (cr)) {
						// font size 12 seems suitable for iconHeight 48 pixels
						float fontSize = 12 * rectSizing.Height / 48f;
						layout.FontDescription = Pango.FontDescription.FromString ("Sans " + fontSize.ToString ());
						layout.SetText ("const");
						layout.Width = rectSizing.Width;
						layout.Alignment = Pango.Alignment.Center;
						int lwidth, lheight;
						layout.GetPixelSize (out lwidth, out lheight);
						// 0, 0 = left top
						//cr.MoveTo (0.5 * (width - lwidth), 0.5 * (height - lheight));
						cr.MoveTo (0.5 * rectSizing.Width, 0.5 * (rectSizing.Height - lheight));
						Pango.CairoHelper.ShowLayout (cr, layout);
					}
				}
				return new Gdk.Pixbuf (surface.Data, Gdk.Colorspace.Rgb, true, 8, rectSizing.Width, rectSizing.Height, surface.Stride, null);
			}
		}

		// free some KiB depending on icon size and image format
		public void CleanupTemp ()
		{
			#if !BitmapToPixbufConversionRaw
			if (memoryStream != null) {
				memoryStream.Dispose ();
				memoryStream = null;
			}
			#endif
		}

		protected Gdk.Pixbuf DrawAndConvert ()
		{
			// Things like Padding needs to be set each time after Clear()
			plotSurface.SurfacePadding = padding;

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage (bitmap_cache)) {
				plotSurface.Draw (g, rectSizing.Bounds);
				// draw frame
				g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
				g.DrawRectangle (framePen, 0, 0, rectSizing.Width - FrameWidth, rectSizing.Height - FrameWidth);
			}

			// Florence/NPlot library uses System.Drawing (.NET Base Class Library) types
			// have to convert result (System.Drawing.Bitmap) to Gdk.Pixbuf for Gtk usage

			#if BitmapToPixbufConversionRaw
			return PixbufFromBitmap (bitmap_cache);
			#else
			if (memoryStream == null)
				memoryStream = new System.IO.MemoryStream (MemoryStreamCapacity);
			memoryStream.Position = 0;
			bitmap_cache.Save (memoryStream, imageFormat);
			memoryStream.Position = 0;
			return new Gdk.Pixbuf (memoryStream);
			#endif
		}

		#if BitmapToPixbufConversionRaw
		
		// working but sensitive to internal bitmap data format
		// http://mono.1490590.n4.nabble.com/Current-way-of-creating-a-Pixbuf-from-an-RGB-Array-td1545766.html
		// https://stackoverflow.com/questions/19187737/converting-a-bgr-bitmap-to-rgb
		public Gdk.Pixbuf PixbufFromBitmap (Bitmap bitmap)
		{
			int width = bitmap.Width;
			int height = bitmap.Height;

			System.Drawing.Imaging.BitmapData bitmapData = bitmap.LockBits (new System.Drawing.Rectangle (0, 0, width, height),
				                                               System.Drawing.Imaging.ImageLockMode.ReadOnly,
				                                               System.Drawing.Imaging.PixelFormat.Format24bppRgb);

			int stride = bitmapData.Stride;
			int size = height * stride;
			byte[] rawData = new byte[size];
			// using unsafe pointers on locked bitmap data would avoid copy
			System.Runtime.InteropServices.Marshal.Copy (bitmapData.Scan0, rawData, 0, size);
			bitmap.UnlockBits (bitmapData);

			// BGR (Microsoft's internal format) to RGB conversion necessary
			// following conversion does not slow down processing many icons noticably - no need to use unsafe pointers etc.
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					int i = y * stride + 3 * x;
					byte tmp = rawData [i];
					rawData [i] = rawData [i + 2];
					rawData [i + 2] = tmp;
				}
			}

			return new Gdk.Pixbuf (rawData, Colorspace.Rgb, false, 8, width, height, stride);
		}

		#endif
	}
}