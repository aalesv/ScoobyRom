// Config.cs: Settings, parses app.config.

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
using System.Collections.Specialized;

namespace ScoobyRom
{
	public static class Config
	{
		const int DefaultIconWidth = 48;
		const int DefaultIconHeight = 32;
		const int IconMin = 10;
		const int IconMax = 255;

		const string key_IconWidthStr = "iconWidth";
		const string key_IconHeightStr = "iconHeight";
		const string key_iconsOnByDefault = "iconsOnByDefault";

		// works on Linux at least
		const string gnuplotDefaultPath_Other = "gnuplot";
		// works on Windows when gnuplot EXE is in path (gnuplot installer can do this)
		const string gnuplotDefaultPath_Win32NT = "gnuplot.exe";

		static string platformStr = Environment.OSVersion.Platform.ToString ();
		static string gnuplotPath;
		static bool iconsOnByDefault;
		static int iconWidth = DefaultIconWidth;
		static int iconHeight = DefaultIconHeight;

		/// <summary>
		/// Null if key not found!
		/// </summary>
		public static string GnuplotPath {
			get { return gnuplotPath; }
		}

		public static string PlatformStr {
			get { return platformStr; }
		}

		public static bool IconsOnByDefault {
			get { return iconsOnByDefault; }
		}

		public static int IconWidth {
			get { return iconWidth; }
		}

		public static int IconHeight {
			get { return iconHeight; }
		}

		// should work even if .config file is missing
		static Config ()
		{
			// Get the AppSettings collection.
			// ConfigurationManager requires reference to System.Configuration.dll !
			NameValueCollection appSettings = System.Configuration.ConfigurationManager.AppSettings;
			// Value is null when key not found!

			gnuplotPath = appSettings ["gnuplot_" + Environment.OSVersion.Platform.ToString ()];
			if (gnuplotPath == null) {
				switch (Environment.OSVersion.Platform) {
				case PlatformID.Win32NT:
					gnuplotPath = gnuplotDefaultPath_Win32NT;
					break;
				default:
					gnuplotPath = gnuplotDefaultPath_Other;
					break;
				}
			}

			string val;
			int intValue;

			val = appSettings [key_iconsOnByDefault];
			if (val != null)
				bool.TryParse (val, out iconsOnByDefault);

			val = appSettings [key_IconWidthStr];
			if (val != null && int.TryParse (val, out intValue)) {
				iconWidth = ValueInRange (intValue, IconMin, IconMax);
			}

			val = appSettings [key_IconHeightStr];
			if (val != null && int.TryParse (val, out intValue)) {
				iconHeight = ValueInRange (intValue, IconMin, IconMax);
			}
		}

		static int ValueInRange (int value, int min, int max)
		{
			return Math.Min (max, Math.Max (min, value));
		}
	}
}