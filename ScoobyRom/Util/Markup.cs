// Markup.cs: Helper functions for Pango markup format.

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


namespace Util
{
	public static class Markup
	{
		public static string NameUnit (string name, string unit)
		{
			if (string.IsNullOrEmpty (name) && string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("<span weight=\"bold\">{0} <tt>[{1}]</tt></span>", name, unit);
		}

		public static string NameUnit_Large (string name, string unit)
		{
			if (string.IsNullOrEmpty (name) && string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("<span size=\"large\" weight=\"bold\">{0} <tt>[{1}]</tt></span>", name, unit);
		}

		public static string Unit (string unit)
		{
			if (string.IsNullOrEmpty (unit))
				return string.Empty;
			else
				return string.Format ("<span weight=\"bold\"><tt>[{0}]</tt></span>", unit);
		}
	}
}