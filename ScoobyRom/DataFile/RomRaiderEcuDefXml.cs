// RomRaiderEcuDefXml.cs: Export data in RomRaider ECU definition format.

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


using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tables.Denso;

namespace ScoobyRom.DataFile
{
	public static class RomRaiderEcuDefXml
	{
		public static void WriteRRXmlFile (string path, XElement romid, IList<Table2D> list2D, IList<Table3D> list3D)
		{
			XmlTextWriter xw = new XmlTextWriter (path, System.Text.Encoding.UTF8);
			// necessary, otherwise single line
			xw.Formatting = Formatting.Indented;

			var l2D = list2D == null ? null : list2D.Select (t => t.RRXml ());
			var l3D = list3D == null ? null : list3D.Select (t => t.RRXml ());

			XDocument doc = RRXmlDocument (new XElement ("rom", romid, l2D, l3D));

			doc.WriteTo (xw);
			xw.Close ();
		}

		public static XDocument RRXmlDocument (params object[] content)
		{
			// XDeclaration: null parameters --> "<?xml version="1.0" encoding="utf-8"?>"
			return new XDocument (new XDeclaration (null, null, null),
				new XComment ("RomRaider ECU definition file"),
				new XComment (ScoobyRom.MainClass.GeneratedBy),
				new XElement ("roms", content));
		}
	}
}
