// TunerProXdf.cs: Export data in TunerPro XDF definition format.

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
	public static class TunerProXdf
	{
		const string Version = "1.50";

		public static void WriteXdfFile (string path, RomMetadata romMetadata, Dictionary <string, int> categories,
		                                 IList<Table2D> list2D, IList<Table3D> list3D)
		{
			// TunerPro (v5.00.8853 2015-11-26) parser does not support UTF-8 language encoding!
			// using Encoding.ASCII results in wrong translation of special characters like "°, ³"
			XmlTextWriter xw = new XmlTextWriter (path, System.Text.Encoding.GetEncoding ("ISO-8859-1"));
			// necessary, otherwise single line
			xw.Formatting = Formatting.Indented;

			// XDF categories start at 1
			var l2D = list2D == null ? null : list2D.Select (t => t.TunerProXdf (categories [t.CategoryForExport] + 1));
			var l3D = list3D == null ? null : list3D.Select (t => t.TunerProXdf (categories [t.CategoryForExport] + 1));

			XDocument doc = TunerProXdfDocument (
				                Header (romMetadata, categories),
				                l2D,
				                l3D
			                );

			doc.WriteTo (xw);
			xw.Close ();
		}

		public static XDocument TunerProXdfDocument (params object[] content)
		{
			return new XDocument (
				new XComment ("TunerPro XDF definition file"),
				new XComment (ScoobyRom.MainClass.GeneratedBy),
				new XElement ("XDFFORMAT", new XAttribute ("version", Version),
					content));
		}

		public static XElement Header (RomMetadata romMetadata, Dictionary <string, int> categories)
		{
			return new XElement ("XDFHEADER",
				new XElement ("deftitle", romMetadata.CalibrationID),
				new XElement ("description", ScoobyRom.MainClass.GeneratedBy),
				new XElement ("author", "unknown"),
				new XElement ("baseoffset", 0),
				// <DEFAULTS datasizeinbits="16" sigdigits="2" outputtype="1" signed="0" lsbfirst="0" float="0" />
				new XElement ("DEFAULTS",
					new XAttribute ("datasizeinbits", 16),
					new XAttribute ("sigdigits", 2),
					new XAttribute ("outputtype", 1),
					new XAttribute ("signed", 0),
					new XAttribute ("lsbfirst", 0),
					new XAttribute ("float", 0)),
				// <REGION type="0xFFFFFFFF" startaddress="0x0" size="0x100000" regionflags="0x0" name="Binary File" desc="This region describes the bin file edited by this XDF" />
				new XElement ("REGION",
					new XAttribute ("type", HexNum (-1)),
					new XAttribute ("startaddress", HexNum (0)),
					new XAttribute ("size", HexNum (romMetadata.Filesize)),
					new XAttribute ("regionflags", HexNum (0)),
					new XAttribute ("name", "Binary File"),
					new XAttribute ("desc", "This region describes the bin file edited by this XDF")),
				CategoriesDef (categories)
			);

		}

		static string HexNum (int num)
		{
			return "0x" + num.ToString ("X");
		}

		static IList <XElement> CategoriesDef (Dictionary <string, int> categories)
		{
			var l = new List <XElement> (categories.Count);
			foreach (var c in categories) {
				l.Add (new XElement ("CATEGORY",
					// TunerPro expects index in hex, tested not working in decimal
					new XAttribute ("index", HexNum (c.Value)),
					new XAttribute ("name", c.Key)));
			}
			return l;
		}
	}
}
