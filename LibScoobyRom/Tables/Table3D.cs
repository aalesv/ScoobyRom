// Table3D.cs: Class for 3D table record and its data, Subaru ROM specific.

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
using System.Xml.Linq;
using Util;
using Extensions;
using Tables;

namespace Tables.Denso
{
	// Native ROM struct size is 28 bytes in cases where there are two MAC floats,
	// 20 bytes without the two MAC floats
	public sealed class Table3D : Table
	{
		const int CountXMax = CountMax;
		const int CountYMax = CountMax;

		// Temporary singleton for slightly better parsing performance. Not thread-safe!
		static readonly Table3D s_tableInfo3D = new Table3D ();

		public static Table3D TryParseValid (System.IO.Stream stream)
		{
			s_tableInfo3D.Reset ();
			s_tableInfo3D.location = (int)stream.Position;

			s_tableInfo3D.countX = stream.ReadInt16BigEndian ();
			s_tableInfo3D.countY = stream.ReadInt16BigEndian ();
			s_tableInfo3D.rangeX.Pos = stream.ReadInt32BigEndian ();
			s_tableInfo3D.rangeY.Pos = stream.ReadInt32BigEndian ();
			s_tableInfo3D.rangeZ.Pos = stream.ReadInt32BigEndian ();
			// first byte matters, rest probably just alignment (zeroes), read all four in one go by little endian
			s_tableInfo3D.tableType = (TableType)(stream.ReadInt32LittleEndian ());
			// 20 bytes read so far

			// Float type never has MAC floats so far, makes sense.
			// Shortcut, does not hurt (valid) results though.
			//if (TableType != TableType.Float)
			{
				// most but not all non-float tables have MAC floats:
				s_tableInfo3D.multiplier = stream.ReadSingleBigEndian ();
				s_tableInfo3D.offset = stream.ReadSingleBigEndian ();

				long afterRecord = stream.Position;

				if (s_tableInfo3D.IsRecordValid ()) {
					if (!s_tableInfo3D.hasMAC) {
						// must back off to adjust stream position for next possible struct
						afterRecord -= 2 * FloatSize;
					}

					bool valuesOk = s_tableInfo3D.ReadValidateValues (stream);
//					if (!valuesOk) {
//						Console.Error.WriteLine ("3D: Error in values");
//					}

					stream.Position = afterRecord;

					return valuesOk ? s_tableInfo3D.Copy () : null;
				} else
					return null;
			}
		}

		// Native ROM struct size is 28 bytes in cases where there are two MAC floats,
		// 20 bytes without the two MAC floats

		// additional fields
		int countY;
		float[] valuesY;
		Range rangeZ;
		// can hold various array types like byte[] or ushort[]
		Array valuesZ;
		float[] valuesZasFloats;
		float valuesZmin, valuesZmax, valuesZavg;

		// metadata
		string nameY, unitZ;

		public override void Reset ()
		{
			base.Reset ();
			rangeZ = Range.Zero;
			valuesY = null;
			valuesZ = null;
			valuesZasFloats = null;
			valuesZmin = FloatUndefined;
			valuesZmax = FloatUndefined;
			valuesZavg = FloatUndefined;
		}

		public Table3D Copy ()
		{
			Table3D c = new Table3D ();
			c.location = location;

			c.countX = countX;
			c.countY = countY;

			c.tableType = tableType;
			c.typeUncertain = typeUncertain;

			c.rangeX = rangeX;
			c.rangeY = rangeY;
			c.rangeZ = rangeZ;

			c.hasMAC = hasMAC;
			c.multiplier = Multiplier;
			c.offset = Offset;

			// copy array references only for best performance, no deep copy
			c.valuesX = valuesX;
			c.valuesY = valuesY;
			c.valuesZ = valuesZ;

			c.valuesZasFloats = valuesZasFloats;
			c.valuesZmin = Zmin;
			c.valuesZmax = Zmax;
			c.valuesZavg = valuesZavg;

			// metadata
			c.title = title ?? string.Empty;
			c.category = category ?? string.Empty;
			c.selected = selected;
			c.description = description ?? string.Empty;
			c.nameX = nameX ?? string.Empty;
			c.nameY = nameY ?? string.Empty;
			c.unitX = unitX ?? string.Empty;
			c.unitY = unitY ?? string.Empty;
			c.unitZ = unitZ ?? string.Empty;

			return c;
		}

		/// <summary>
		/// Number of rows.
		/// </summary>
		public int CountY {
			get { return countY; }
			set { countY = value; }
		}

		public float[] ValuesY {
			get { return this.valuesY; }
		}

		// valid object has increasing axis values
		public float Ymin {
			get { return valuesY != null ? valuesY [0] : FloatUndefined; }
		}

		// valid object has increasing axis values
		public float Ymax {
			get { return valuesY != null ? valuesY [this.valuesY.Length - 1] : FloatUndefined; }
		}

		public float[] GetValuesZasFloats ()
		{
			return valuesZasFloats;
		}

		public float Zmax {
			get { return this.valuesZmax; }
		}

		public float Zmin {
			get { return this.valuesZmin; }
		}

		public float Zavg {
			get { return this.valuesZavg; }
		}

		public Array ValuesZ {
			get { return this.valuesZ; }
		}

		/// <summary>
		/// Number of Z-avalues = CountX * CountY.
		/// </summary>
		public int CountZ {
			get { return countX * countY; }
		}

		public Range RangeZ {
			get { return this.rangeZ; }
			set { rangeZ = value; }
		}

		public string NameY {
			get { return this.nameY; }
			set { nameY = value; }
		}

		public string UnitZ {
			get { return this.unitZ; }
			set { unitZ = value; }
		}

		public Table3D ()
		{
		}

		public override int RecordSize {
			get { return hasMAC ? 28 : 20; }
		}

		public override bool HasMetadata {
			get { return base.HasMetadata || !string.IsNullOrEmpty (nameY) || !string.IsNullOrEmpty (unitY); }
		}

		public override bool IsDataConst {
			get { return this.valuesZmin == this.valuesZmax; }
		}

		public override string ToString ()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder (200);
			sb.AppendFormat ("[Table3D @ {0:X6} | Selected={1} CountX={2} CountY={3} CountZ={4} Type={5} | RangeX={6}, RangeY={7}, RangeZ={8} | ",
				location, selected, countX.ToString (), countY.ToString (), CountZ.ToString (), tableType.ToStr (),
				rangeX.ToString (), rangeY.ToString (), rangeZ.ToString ());

			sb.AppendFormat (" | Xmin={0} Xmax={1} | Ymin={2} Ymax={3} | Zmin={4} Zmax={5} Zavg={6}",
				Xmin, Xmax, Ymin, Ymax, Zmin, Zmax, Zavg);

			if (hasMAC) {
				sb.AppendFormat (" | Multiplier={0}, Offset={1}]", Multiplier, Offset);
			} else {
				sb.Append ("]");
			}
			return sb.ToString ();
		}

		public override bool IsRecordValid ()
		{
			if (countX > CountMax || countX < CountMin || countY > CountMax || countY < CountMin)
				return false;

			if (!tableType.IsValid ())
				return false;

			if (rangeX.Pos > posMax || rangeY.Pos > posMax || rangeZ.Pos > posMax)
				return false;
			if (rangeX.Pos < posMin || rangeY.Pos < posMin || rangeZ.Pos < posMin)
				return false;
			if (rangeX.Pos == rangeY.Pos || rangeX.Pos == rangeZ.Pos || rangeY.Pos == rangeZ.Pos)
				return false;


			// range checking eliminates a few bad candidates
			rangeX.Size = FloatSize * countX;
			rangeY.Size = FloatSize * countY;

			int countZ = CountZ;
			// assume smallest value type which is (u)int8 for safe intersect check as type might be wrong
			rangeZ.Size = countZ;
			if (rangeX.Intersects (rangeY) || rangeX.Intersects (rangeZ) || rangeY.Intersects (rangeZ))
				return false;

			rangeZ.Size = tableType.ValueSize () * countZ;

			CheckMAC ();

			return true;
		}

		public override void ChangeTypeToAndReload (TableType newType, System.IO.Stream stream)
		{
			tableType = newType;
			rangeZ.Size = newType.ValueSize () * this.CountZ;
			valuesZ = ReadValues (stream, rangeZ, tableType);

			this.valuesZasFloats = ValuesAsFloats (this.valuesZ);

			/* // using LINQ
			this.valuesZmin = valuesZasFloats.Min ();
			this.valuesZmax = valuesZasFloats.Max ();
			this.valuesZavg = valuesZasFloats.Average ();
			*/
			// manually calc min, max, average; probably faster
			CalcMinMaxAverage (valuesZasFloats, out valuesZmin, out valuesZmax, out valuesZavg);
		}

		public override bool ReadValidateValues (System.IO.Stream stream)
		{
			if (!CheckAxisArray (valuesX = ReadValuesFloat (stream, rangeX)))
				return false;
			if (!CheckAxisArray (valuesY = ReadValuesFloat (stream, rangeY)))
				return false;

			ChangeTypeToAndReload (tableType, stream);

			if (!hasMAC && tableType == TableType.Float) {
				// uncertain, e.g. all floats can be valid (zero)
				if (!CheckFloatArray ((float[])valuesZ)) {
					ChangeTypeToAndReload (TableType.UInt16, stream);
					typeUncertain = true;
				}
			}

			return true;
		}

		public override XElement RRXml ()
		{
			return new XElement ("table",
				new XAttribute ("type", "3D"),
				new XAttribute ("name", TitleForExport),
				new XAttribute ("category", CategoryForExport),
				new XAttribute ("storagetype", tableType.ToRRType ()),
				new XAttribute ("endian", endian),
				new XAttribute ("sizex", countX.ToString ()),
				new XAttribute ("sizey", countY.ToString ()),
				new XAttribute ("storageaddress", HexNum (rangeZ.Pos)),
				CommentValuesStats (valuesZmin, valuesZmax, valuesZavg),
				RRXmlScaling (unitZ, Expression, ExpressionReverse, "0.000", 0.01f, 0.1f),
				RRXmlAxis (AxisType.X, nameX, unitX, TableType.Float, rangeX, valuesX, Xmin, Xmax),
				RRXmlAxis (AxisType.Y, nameY, unitY, TableType.Float, rangeY, valuesY, Ymin, Ymax),
				new XElement ("description", description));
		}

		public override string CategoryForExport {
			get { return string.IsNullOrWhiteSpace (this.category) ? "Unknown 3D" : this.category; }
		}

		public override XElement TunerProXdf (int categoryID)
		{
			return new XElement ("XDFTABLE",
				new XAttribute ("uniqueid", HexNum (location)),
				new XAttribute ("flags", HexNum (0)),
				new XElement ("title", TitleForExport),
				CategoryXdf (categoryID),
				AxisXdf (AxisType.X, TableType.Float, countX, rangeX.Pos, unitX),
				AxisXdf (AxisType.Y, TableType.Float, countY, rangeY.Pos, unitY),
				ZAxisXdf (tableType, countX, countY, rangeZ.Pos, unitZ, GenerateExpression (ExpressionVarNameXdf))
			);
		}

		public override string CopyTableRomRaider ()
		{
			var cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			var sb = new System.Text.StringBuilder ();

			sb.AppendLine ("[Table3D]");

			for (int x = 0; x < countX; x++) {
				// add additional delimiter, i.e. for spreadsheet application
				sb.Append (DelimiterRomRaider);
				sb.Append (valuesX [x].ToString (cultureInfo));
			}

			float[] valuesZ = GetValuesZasFloats ();
			for (int y = 0; y < countY; y++) {
				sb.AppendLine ();
				sb.Append (valuesY [y].ToString (cultureInfo));

				for (int x = 0; x < countX; x++) {
					sb.Append (DelimiterRomRaider);
					sb.Append (valuesZ [y * countX + x].ToString (cultureInfo));
				}
			}
			return sb.ToString ();
		}
	}
}
