// TextEcuDef.cs: Export data in text format.

/* Copyright (C) 2011-2024 SubaruDieselCrew
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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tables;
using Tables.Denso;
using System.IO;
using System.Text;

namespace ScoobyRom.DataFile
{
	public class TextEcuDef
	{
        public static void WriteCsvFile(string path, RomMetadata romMetadata, IList<Table2D> list2D, IList<Table3D> list3D)
        {
			var list2DSelected = list2D.Where(u => u.Selected);
            var list3DSelected = list3D.Where(u => u.Selected);
            
            //Export all tables with metadata and all selected tables
            WriteCsvFile(path,
				list2D.Where(t => t.HasMetadata).Union(list2DSelected),
				list3D.Where(t => t.HasMetadata).Union(list3DSelected));

        }

		public static void WriteCsvFile(string path, IEnumerable<Table2D> list2D, IEnumerable<Table3D> list3D)
		{
			string[] csvHeader = {	"table_type",
									"category",
									"storageaddress",
									"unit_x",
									"name_x",
									"unit_y",
									"name_y",
									"unit_z",
									"x_len",
									"y_len",
									"data_type",
									"axis_x_storageaddress",
									"axis_y_storageaddress",
									"axis_z_storageaddress",
									"multiplier",
									"offset",
									"name"
									};
			StringBuilder outCsv = new StringBuilder();
			var csvSeparator = ",";
			outCsv.AppendLine(string.Join(csvSeparator, csvHeader));
			foreach (var t in list2D)
			{
				var multiplier =	float.IsNaN(t.Multiplier) 	? 0 : t.Multiplier;
				var offset = 		float.IsNaN(t.Offset) 		? 0 : t.Offset;
				outCsv.AppendLine(string.Join(csvSeparator,
												"table2D",		//table_type
												t.Category,		//category
												t.Location,		//storageaddress
												t.UnitX,		//unit_x
												t.NameX,		//name_x
												"",				//unit_y
												"",				//name_y
												t.UnitY,		//unit_z
												t.CountX,		//x_len
												"0",			//y_len
												t.TableType,	//data_type
												t.RangeX.Pos,	//axis_x_storageaddress
												"0",			//axis_y_storageaddress
												t.RangeY.Pos,	//axis_z_storageaddress
												multiplier,		//multiplier
												offset,			//offset
												t.Title			//name
												));
			}

			foreach (var t in list3D)
			{
				var multiplier =	float.IsNaN(t.Multiplier) 	? 0 : t.Multiplier;
				var offset = 		float.IsNaN(t.Offset) 		? 0 : t.Offset;
				outCsv.AppendLine(string.Join(csvSeparator,
												"table3D",		//table_type
												t.Category,		//category
												t.Location,		//storageaddress
												t.UnitX,		//unit_x
												t.NameX,		//name_x
												t.UnitY,		//unit_y
												t.NameY,		//name_y
												t.UnitZ,		//unit_z
												t.CountX,		//x_len
												t.CountY,		//y_len
												t.TableType,	//data_type
												t.RangeX.Pos,	//axis_x_storageaddress
												t.RangeY.Pos,	//axis_y_storageaddress
												t.RangeZ.Pos,	//axis_z_storageaddress
												multiplier,		//multiplier
												offset,			//offset
												t.Title			//name
												));
			}
			File.WriteAllText(path, outCsv.ToString());
		}
	}
}
