// Table2D.cs: Class for 2D table record and its data, Subaru ROM specific.

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
using Extensions;
using Tables;

namespace Tables.Denso
{
	// Native ROM struct size is 20 bytes in cases where there are two MAC floats,
	// 12 bytes without the two MAC floats
	public sealed class Table2D : Table
	{
		// Temporary singleton for slightly better parsing performance. Not thread-safe!
		static readonly Table2D s_tableInfo2D = new Table2D ();


		public static Table2D TryParseValid (System.IO.Stream stream)
		{
			s_tableInfo2D.Reset ();

			s_tableInfo2D.location = (int)stream.Position;

			s_tableInfo2D.countX = stream.ReadInt16BigEndian ();

			// First byte matters, residual byte probably just zero for alignment.
			// By reading as little endian word we read and verify all two bytes in one go.
			s_tableInfo2D.tableType = (TableType)stream.ReadInt16LittleEndian ();

			s_tableInfo2D.rangeX.Pos = stream.ReadInt32BigEndian ();
			s_tableInfo2D.rangeY.Pos = stream.ReadInt32BigEndian ();

			// most but not all non-float tables have MAC floats:
			s_tableInfo2D.multiplier = stream.ReadSingleBigEndian ();
			s_tableInfo2D.offset = stream.ReadSingleBigEndian ();

			long afterRecord = stream.Position;

			if (s_tableInfo2D.IsRecordValid ()) {
				if (!s_tableInfo2D.hasMAC) {
					// must back off stream position for next possible struct
					afterRecord -= 2 * FloatSize;
				}

				bool valuesOk = s_tableInfo2D.ReadValidateValues (stream);
//				if (!valuesOk) {
//					Console.Error.WriteLine ("2D: Error in values");
//				}

				stream.Position = afterRecord;

				return valuesOk ? s_tableInfo2D.Copy () : null;
			} else
				return null;
		}

		// additional fields
		Array valuesY;
		float[] valuesYasFloats;
		float valuesYmin, valuesYmax, valuesYavg;

		public float Ymin {
			get { return valuesYmin; }
		}

		public float Ymax {
			get { return valuesYmax; }
		}

		public float Yavg {
			get { return valuesYavg; }
		}

		public override bool IsDataConst {
			get { return this.valuesYmin == this.valuesYmax; }
		}

		public float[] GetValuesYasFloats ()
		{
			return valuesYasFloats;
		}

		public override void Reset ()
		{
			base.Reset ();
			valuesY = null;
			valuesYasFloats = null;
			valuesYmin = FloatUndefined;
			valuesYmax = FloatUndefined;
			valuesYavg = FloatUndefined;
		}

		public Table2D Copy ()
		{
			Table2D c = new Table2D ();
			c.countX = countX;
			c.tableType = tableType;
			c.typeUncertain = typeUncertain;

			c.hasMAC = hasMAC;
			c.multiplier = Multiplier;
			c.offset = Offset;

			c.location = location;
			c.rangeX = rangeX;
			c.rangeY = rangeY;

			c.valuesX = valuesX;
			c.valuesY = valuesY;
			c.valuesYasFloats = valuesYasFloats;
			c.valuesYmin = valuesYmin;
			c.valuesYmax = valuesYmax;
			c.valuesYavg = valuesYavg;

			// metadata
			c.title = title ?? string.Empty;
			c.category = category ?? string.Empty;
			c.selected = selected;
			c.description = description ?? string.Empty;
			c.nameX = nameX ?? string.Empty;
			c.unitX = unitX ?? string.Empty;
			c.unitY = unitY ?? string.Empty;

			return c;
		}

		public Table2D ()
		{
		}

		public override int RecordSize {
			get { return hasMAC ? 20 : 12; }
		}

		public override string ToString ()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder (200);
			sb.AppendFormat ("[Table2D @ {0:X6} | Selected={1} Count={2} Type={3} | RangeX={4}, RangeY={5} | xMin={6} xMax={7} | yMin={8} yMax={9} yAvg={10}",
				location, selected, CountX, TableType.ToStr (),
				rangeX.ToString (), rangeY.ToString (),
				Xmin.ToString (), Xmax.ToString (),
				Ymin.ToString (), Ymax.ToString (), Yavg.ToString ());
			if (hasMAC) {
				sb.AppendFormat (" | Multiplier={0}, Offset={1}]", Multiplier, Offset);
			} else {
				sb.Append ("]");
			}
			return sb.ToString ();
		}

		public override bool IsRecordValid ()
		{
			int count = this.countX;
			if (count > CountMax || count < CountMin)
				return false;

			if (!tableType.IsValid ())
				return false;

			if (rangeX.Pos > posMax || rangeX.Pos < posMin || rangeY.Pos > posMax || rangeY.Pos < posMin || rangeX.Pos == rangeY.Pos)
				return false;

			// range checking eliminates a few bad candidates
			rangeX.Size = FloatSize * count;

			// assume smallest value type which is (u)int8 for safe intersect check as type might be wrong
			rangeY.Size = count;
			if (rangeX.Intersects (rangeY))
				return false;

			rangeY.Size = tableType.ValueSize () * count;

			CheckMAC ();

			return true;
		}

		public override void ChangeTypeToAndReload (TableType newType, System.IO.Stream stream)
		{
			this.tableType = newType;
			this.rangeY.Size = tableType.ValueSize () * this.countX;
			this.valuesY = ReadValues (stream, rangeY, tableType);
			this.valuesYasFloats = ValuesAsFloats (this.valuesY);
			/* // using LINQ
			this.valuesYmin = valuesYasFloats.Min ();
			this.valuesYmax = valuesYasFloats.Max ();
			this.valuesYavg = valuesYasFloats.Average ();
			*/
			// manually calc min, max, average; probably faster
			CalcMinMaxAverage (valuesYasFloats, out valuesYmin, out valuesYmax, out valuesYavg);
		}

		public override bool ReadValidateValues (System.IO.Stream stream)
		{
			if (!CheckAxisArray (valuesX = ReadValuesFloat (stream, rangeX)))
				return false;

			ChangeTypeToAndReload (tableType, stream);

			// TODO improve table type intelligence
			if (!hasMAC && (tableType == TableType.Float)) {
				typeUncertain = true;

				// uncertain, e.g. might get all valid zero floats
				if (!CheckFloatArray (valuesYasFloats)) {
					ChangeTypeToAndReload (TableType.UInt16, stream);
				}
			}

			return true;
		}

		public override XElement RRXml ()
		{
			// X-axis is being called "Y Axis" in RR!
			return new XElement ("table",
				new XAttribute ("type", "2D"),
				new XAttribute ("name", TitleForExport),
				new XAttribute ("category", CategoryForExport),
				new XAttribute ("storagetype", tableType.ToRRType ()),
				new XAttribute ("endian", endian),
				new XAttribute ("sizey", countX.ToString ()),
				new XAttribute ("storageaddress", HexNum (rangeY.Pos)),
				CommentValuesStats (valuesYmin, valuesYmax, valuesYavg),
				RRXmlScaling (unitX, Expression, ExpressionReverse, "0.000", 0.01f, 0.1f),
				RRXmlAxis (AxisType.Y, nameX, unitX, TableType.Float, rangeX, valuesX, Xmin, Xmax),
				new XElement ("description", description));
		}

		public override string CategoryForExport {
			get { return string.IsNullOrWhiteSpace (this.category) ? "Unknown 2D" : this.category; }
		}

		public override XElement TunerProXdf (int categoryID)
		{
			return new XElement ("XDFTABLE",
				new XAttribute ("uniqueid", HexNum (location)),
				new XAttribute ("flags", HexNum (0)),
				new XElement ("title", TitleForExport),
				CategoryXdf (categoryID),
				EmptyXAxisXdf (),
				AxisXdf (AxisType.Y, TableType.Float, countX, rangeX.Pos, unitX),
				ZAxisXdf (tableType, 0, countX, rangeY.Pos, unitY, GenerateExpression (ExpressionVarNameXdf))
			);
		}

		public void WriteCSV (System.IO.TextWriter tw)
		{
			// assure decimal point is "." so standard delimiter "," can be used
			System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
			const char delimiter = ',';

			// Spreadsheet apps usually recognize first row as axis description
			tw.Write ("{0} [{1}]", NameX, UnitX);
			tw.Write (delimiter);
			tw.WriteLine ("{0} [{1}]", Title, UnitY);

			float[] valuesY = GetValuesYasFloats ();
			for (int i = 0; i < countX; i++) {
				tw.Write (valuesX [i].ToString (cultureInfo));
				tw.Write (delimiter);
				tw.WriteLine (valuesY [i].ToString (cultureInfo));
			}
		}

		public override string CopyTableRomRaider ()
		{
			var cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
			var sb = new System.Text.StringBuilder ();

			sb.AppendLine ("[Table2D]");

			for (int i = 0; i < countX; i++) {
				if (i > 0) {
					sb.Append (DelimiterRomRaider);
				}
				sb.Append (valuesX [i].ToString (cultureInfo));
			}

			sb.AppendLine ();

			float[] valuesY = GetValuesYasFloats ();
			for (int i = 0; i < countX; i++) {
				if (i > 0) {
					sb.Append (DelimiterRomRaider);
				}
				sb.Append (valuesY [i].ToString (cultureInfo));
			}
			return sb.ToString ();
		}
	}
}
