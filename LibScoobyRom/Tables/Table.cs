// Table.cs: Table base class, provides common features.

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
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Util;
using Extensions;
using Tables;

namespace Tables.Denso
{
	/// <summary>
	/// Common functionality for 2D and 3D Table types.
	/// </summary>
	public abstract class Table
	{
		// Axis item count restrictions. Count < 2 or high does not make sense.
		// This restriction helps avoiding false positives.
		public const int CountMin = 2;
		public const int CountMax = 255;

		public const char DelimiterRomRaider = '\t';

		// All 2D- and 3D-Table structs contain pointers. If a pointer points to a rather odd position,
		// the suggested table struct will be discarded. This restriction helps avoiding false positives.
		static protected int posMax = (1024 + 512) * 1024;
		static protected int posMin = 8 * 1024;

		/// <summary>
		/// Gets or sets the pointer maximum, improving table detection.
		/// Set to maximum expected pointer position or file size if in doubt.
		/// </summary>
		/// <value>
		/// The position max.
		/// </value>
		public static int PosMax {
			get { return posMax; }
			set { posMax = value; }
		}

		/// <summary>
		/// Gets or sets the pointer minimum, improving table detection.
		/// Set to minimum expected position or 0 if in doubt.
		/// </summary>
		/// <value>
		/// The position minimum.
		/// </value>
		public static int PosMin {
			get { return posMin; }
			set { posMin = value; }
		}

		public const int FloatSize = 4;
		public const float FloatUndefined = float.NaN;

		// floats like x.xxxxxxxxE-40 etc. suggest these are invalid, not to be included
		public const float FloatMin = (float)1E-12;
		public const float FloatMax = (float)1E+12;

		public const string ExpressionVarName = "x";
		public const string ExpressionVarNameXdf = "X";

		public static string endian = "big";

		#region Fields

		protected int countX;
		protected TableType tableType;
		protected bool typeUncertain = false;
		protected float multiplier, offset;
		protected int location;
		protected Range rangeX, rangeY;
		protected bool hasMAC = false;

		// axis are always floats
		protected float[] valuesX;

		// metadata
		protected string title, category, description, nameX, unitX, unitY;
		protected bool selected;

		#endregion Fields

		/// <summary>
		/// Struct position in file, not part of ROM struct content
		/// </summary>
		public int Location {
			get { return location; }
			set { location = value; }
		}

		/// <summary>
		/// Gets the size of the record (structure) in bytes.
		/// </summary>
		/// <value>The size of the record in bytes.</value>
		public abstract int RecordSize { get; }

		public int CountX {
			get { return countX; }
			set { countX = value; }
		}

		public float[] ValuesX {
			get { return this.valuesX; }
		}

		// valid object has increasing axis values
		public float Xmin {
			get { return valuesX != null ? valuesX [0] : FloatUndefined; }
		}

		// valid object has increasing axis values
		public float Xmax {
			get { return valuesX != null ? valuesX [this.valuesX.Length - 1] : FloatUndefined; }
		}

		/// <summary>
		/// Can be be wrong! Not certain from parsing record.
		/// </summary>
		public TableType TableType {
			get { return tableType; }
			set { tableType = value; }
		}

		public bool TypeUncertain {
			get { return this.typeUncertain; }
		}

		/// <summary>
		/// If Multiplier and Offset floats are available (valid).
		/// </summary>
		public bool HasMAC {
			get { return this.hasMAC; }
		}

		// these two floats are optional, usually for type non-float but not always
		public float Multiplier {
			get { return hasMAC ? multiplier : FloatUndefined; }
			set { multiplier = value; }
		}

		public float Offset {
			get { return hasMAC ? offset : FloatUndefined; }
			set { offset = value; }
		}

		public Range RangeX {
			get { return rangeX; }
			set { rangeX = value; }
		}

		public Range RangeY {
			get { return rangeY; }
			set { rangeY = value; }
		}

		#region metadata properties

		public string NameX {
			get { return nameX; }
			set { nameX = value; }
		}

		public string Title {
			get { return title; }
			set { title = value; }
		}

		public string Category {
			get { return category; }
			set { category = value; }
		}

		public bool Selected {
			get { return selected; }
			set { selected = value; }
		}

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public string UnitX {
			get { return unitX; }
			set { unitX = value; }
		}

		public string UnitY {
			get { return unitY; }
			set { unitY = value; }
		}

		#endregion metadata properties

		public virtual void Reset ()
		{
			tableType = TableType.Undefined;
			rangeX = Range.Zero;
			rangeY = Range.Zero;
			hasMAC = false;
			typeUncertain = false;
			multiplier = FloatUndefined;
			offset = FloatUndefined;
		}

		public abstract bool IsRecordValid ();

		public abstract bool ReadValidateValues (System.IO.Stream stream);

		public abstract void ChangeTypeToAndReload (TableType newType, System.IO.Stream stream);

		public abstract XElement RRXml ();

		public abstract XElement TunerProXdf (int categoryID);

		public abstract string CopyTableRomRaider ();

		public virtual bool HasMetadata {
			get { return !string.IsNullOrEmpty (title) || !string.IsNullOrEmpty (category) || !string.IsNullOrEmpty (description) || !string.IsNullOrEmpty (nameX) || !string.IsNullOrEmpty (unitX); }
		}

		public abstract bool IsDataConst { get; }

		protected static void ThrowInvalidTableType (TableType tableType)
		{
			throw new ArgumentOutOfRangeException ("Invalid TableType: " + tableType.ToString ());
		}

		#region ReadValues

		public static Array ReadValues (System.IO.Stream stream, Range range, TableType tableType)
		{
			switch (tableType) {
			case TableType.Float:
				return ReadValuesFloat (stream, range);
			case TableType.UInt8:
				return ReadValuesUInt8 (stream, range);
			case TableType.UInt16:
				return ReadValuesUInt16 (stream, range);
			case TableType.Int8:
				return ReadValuesInt8 (stream, range);
			case TableType.Int16:
				return ReadValuesInt16 (stream, range);
			case TableType.UInt32:
				return ReadValuesUInt32 (stream, range);
			default:
				ThrowInvalidTableType (tableType);
				return null;
			}
		}

		// Used for reading axis values (always float) and values (in case of type float)
		public static float[] ReadValuesFloat (System.IO.Stream stream, Range range)
		{
			stream.Seek (range.Pos, System.IO.SeekOrigin.Begin);
			int count = range.Size / FloatSize;
			float[] array = new float[count];

			for (int i = 0; i < array.Length; i++) {
				array [i] = stream.ReadSingleBigEndian ();
			}
			return array;
		}

		public static byte[] ReadValuesUInt8 (System.IO.Stream stream, Range range)
		{
			stream.Seek (range.Pos, System.IO.SeekOrigin.Begin);
			int count = range.Size;
			byte[] buf = new byte[count];
			stream.Read (buf, 0, count);
			return buf;
		}

		public static sbyte[] ReadValuesInt8 (System.IO.Stream stream, Range range)
		{
			stream.Seek (range.Pos, System.IO.SeekOrigin.Begin);
			int count = range.Size;
			byte[] buf = new byte[count];
			stream.Read (buf, 0, count);

			// Array.Copy won't work with different Array types like byte[] and sbyte[]
			sbyte[] array = new sbyte[buf.Length];
			for (int i = 0; i < array.Length; i++) {
				array [i] = (sbyte)buf [i];
			}
			return array;
		}

		public static ushort[] ReadValuesUInt16 (System.IO.Stream stream, Range range)
		{
			stream.Seek (range.Pos, System.IO.SeekOrigin.Begin);
			int count = range.Size / 2;
			ushort[] array = new ushort[count];

			for (int i = 0; i < array.Length; i++) {
				array [i] = (ushort)stream.ReadInt16BigEndian ();
			}
			return array;
		}

		public static short[] ReadValuesInt16 (System.IO.Stream stream, Range range)
		{
			stream.Seek (range.Pos, System.IO.SeekOrigin.Begin);
			int count = range.Size / 2;
			short[] array = new short[count];

			for (int i = 0; i < array.Length; i++) {
				array [i] = stream.ReadInt16BigEndian ();
			}
			return array;
		}

		public static uint[] ReadValuesUInt32 (System.IO.Stream stream, Range range)
		{
			stream.Seek (range.Pos, System.IO.SeekOrigin.Begin);
			int count = range.Size / 4;
			uint[] array = new uint[count];

			for (int i = 0; i < array.Length; i++) {
				array [i] = (uint)stream.ReadInt32BigEndian ();
			}
			return array;
		}

		#endregion ReadValues

		public static bool IsFloatValid (float value)
		{
			if (float.IsNaN (value))
				return false;
			if (value == 0f)
				return true;
			if (value < 0f)
				value = Math.Abs (value);
			return (value >= FloatMin) && (value <= FloatMax);
		}

		/// <summary>
		/// Values must increase steadily (required for ROM interpolation sub to work).
		/// </summary>
		/// <param name="floats">
		/// A <see cref="System.Single[]"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public static bool CheckAxisArray (float[] floats)
		{
			for (int i = 1; i < floats.Length; i++) {
				// not all valid axis have strictly increasing values!!!
				// Ex: MAF-Sensor has a duplicate point (X[32] == X[33] incl. corresponding Y-values)
				// had to relax original condition: if (floats[i - 1] >= floats[i])
				if (floats [i - 1] > floats [i])
					return false;
			}
			if (!CheckFloatArray (floats))
				return false;
			return true;
		}

		public static bool CheckFloatArray (float[] floats)
		{
			foreach (float v in floats) {
				if (!IsFloatValid (v))
					return false;
			}
			return true;
		}

		#region Values as float[]

		protected float[] ValuesAsFloats (Array array)
		{
			switch (tableType) {
			case TableType.Float:
				return ValuesFromTypeFloat (array);
			case TableType.UInt8:
				return ValuesFromTypeUInt8 (array);
			case TableType.UInt16:
				return ValuesFromTypeUInt16 (array);
			case TableType.Int8:
				return ValuesFromTypeInt8 (array);
			case TableType.Int16:
				return ValuesFromTypeInt16 (array);
			case TableType.UInt32:
				return ValuesFromTypeUInt32 (array);
			default:
				ThrowInvalidTableType (tableType);
				return null;
			}
		}

		protected float[] ValuesFromTypeFloat (Array array)
		{
			float[] srcFloat = (float[])array;
			var floats = new float[srcFloat.Length];
			if (hasMAC) {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = srcFloat [i] * multiplier + offset;
				}
			} else {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = srcFloat [i];
				}
			}
			return floats;
		}

		protected float[] ValuesFromTypeUInt8 (Array array)
		{
			byte[] srcUInt8 = (byte[])array;
			var floats = new float[srcUInt8.Length];
			if (hasMAC) {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = srcUInt8 [i] * multiplier + offset;
				}
			} else {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = srcUInt8 [i];
				}
			}
			return floats;
		}

		protected float[] ValuesFromTypeUInt16 (Array array)
		{
			ushort[] src = (ushort[])array;
			var floats = new float[src.Length];
			if (hasMAC) {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i] * multiplier + offset;
				}
			} else {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i];
				}
			}
			return floats;
		}

		protected float[] ValuesFromTypeInt8 (Array array)
		{
			sbyte[] src = (sbyte[])array;
			var floats = new float[src.Length];
			if (hasMAC) {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i] * multiplier + offset;
				}
			} else {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i];
				}
			}
			return floats;
		}

		protected float[] ValuesFromTypeInt16 (Array array)
		{
			short[] src = (short[])array;
			var floats = new float[src.Length];
			if (hasMAC) {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i] * multiplier + offset;
				}
			} else {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i];
				}
			}
			return floats;
		}

		protected float[] ValuesFromTypeUInt32 (Array array)
		{
			uint[] src = (uint[])array;
			var floats = new float[src.Length];
			if (hasMAC) {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i] * multiplier + offset;
				}
			} else {
				for (int i = 0; i < floats.Length; i++) {
					floats [i] = src [i];
				}
			}
			return floats;
		}

		#endregion Values as float[]

		public static string HexNum (int value)
		{
			return "0x" + value.ToString ("X");
		}

		public string Expression {
			get { return GenerateExpression (ExpressionVarName); }
		}

		public string ExpressionReverse {
			get { return GenerateExpressionReverse (ExpressionVarName); }
		}

		public string GenerateExpression (string varName)
		{
			if (!hasMAC || (multiplier == 1f && offset == 0f))
				return varName;
			StringBuilder sb = new StringBuilder ();
			sb.Append (varName);
			if (multiplier != 1f) {
				sb.Append ('*');
				sb.Append (multiplier.ToString (CultureInfo.InvariantCulture));
			}
			if (offset != 0f) {
				if (offset > 0f)
					sb.Append ('+');
				sb.Append (offset.ToString (CultureInfo.InvariantCulture));
			}
			return sb.ToString ();
		}

		public string GenerateExpressionReverse (string varName)
		{
			// tested: 0.09999999f to double yields 0.0999999940395355
			if (!hasMAC || (multiplier == 1f && offset == 0f))
				return varName;
			bool needParantheses = multiplier != 1f && offset != 0f;

			StringBuilder sb = new StringBuilder ();
			if (needParantheses)
				sb.Append ('(');

			sb.Append (varName);
			if (offset != 0f) {
				if (offset < 0f)
					sb.Append ('+');
				sb.Append ((-offset).ToString (CultureInfo.InvariantCulture));
			}
			if (needParantheses)
				sb.Append (')');

			if (multiplier != 1f) {
				sb.Append ('/');
				sb.Append (multiplier.ToString (CultureInfo.InvariantCulture));
			}
			return sb.ToString ();
		}

		public static XComment CommentValuesStats (float min, float max)
		{
			return new XComment (string.Format (CultureInfo.InvariantCulture, " {0} to {1} ", min, max));
		}

		public static XComment CommentValuesStats (float min, float max, float avg)
		{
			return new XComment (string.Format (CultureInfo.InvariantCulture, " min: {0}  max: {1}  average: {2} ",
				min.ToString (), max.ToString (), avg.ToString ()));
		}

		public static XElement RRXmlScaling (string units, string expr, string to_byte, string format, float fineincrement, float coarseincrement)
		{
			return new XElement ("scaling",
				new XAttribute ("units", units),
				new XAttribute ("expression", expr),
				new XAttribute ("to_byte", to_byte),
				new XAttribute ("format", format),
				new XAttribute ("fineincrement", fineincrement),
				new XAttribute ("coarseincrement", coarseincrement));
		}

		public XElement RRXmlAxis (AxisType axisType, string name, string unit, TableType tableType, Range range, float[] axis, float min, float max)
		{
			return new XElement ("table",
				new XAttribute ("type", axisType.RRStr ()),
				new XAttribute ("name", name),
				new XAttribute ("storagetype", "float"),
				new XAttribute ("storageaddress", HexNum (range.Pos)),
				CommentValuesStats (min, max),
				RRXmlScaling (unit, ExpressionVarName, ExpressionVarName, "0.00", 1f, 5f));
		}

		public string TitleForExport {
			get { return string.IsNullOrWhiteSpace (this.title) ? string.Format ("Record 0x{0:X}", this.location) : this.title; }
		}

		public abstract string CategoryForExport { get; }

		#region XDF

		protected static XElement CategoryXdf (int categoryID)
		{
			return new XElement ("CATEGORYMEM",
				new XAttribute ("index", 0),
				new XAttribute ("category", categoryID));
		}

		protected static XElement AxisXdf (AxisType axisType, TableType tableType, int count, int address, string units)
		{
			return new XElement ("XDFAXIS",
				new XAttribute ("id", axisType.XdfStr ()),
				new XAttribute ("uniqueid", HexNum (address)),
				EmbeddedDataXdf (tableType, 0, count, address),
				new XElement ("units", units),
				new XElement ("indexcount", count.ToString ()),
				new XElement ("decimalpl", "3"),
				new XElement ("embedinfo",
					new XAttribute ("type", "1")),
				new XElement ("datatype", "0"),
				new XElement ("unittype", "0"),
				new XElement ("MATH",
					new XAttribute ("equation", ExpressionVarNameXdf),
					new XElement ("VAR",
						new XAttribute ("id", ExpressionVarNameXdf)))
			);
		}

		protected static XElement ZAxisXdf (TableType tableType, int colcount, int rowcount, int address, string units, string equation)
		{
			const int DecimalPl = 3;
			return new XElement ("XDFAXIS",
				new XAttribute ("id", AxisType.Z.XdfStr ()),
				new XAttribute ("uniqueid", HexNum (address)),
				EmbeddedDataXdf (tableType, colcount, rowcount, address),
				new XElement ("units", units),
				new XElement ("decimalpl", DecimalPl),
				new XElement ("outputtype", "1"),
				new XElement ("MATH",
					new XAttribute ("equation", equation),
					new XElement ("VAR",
						new XAttribute ("id", ExpressionVarNameXdf)))
			);
		}

		protected static XElement EmbeddedDataXdf (TableType tableType, int colcount, int rowcount, int address)
		{
			// <EMBEDDEDDATA mmedtypeflags="0x10000" mmedaddress="0xB94A4" mmedelementsizebits="32" mmedcolcount="40" mmedmajorstridebits="0" mmedminorstridebits="0" />
			var el = new XElement ("EMBEDDEDDATA",
				         new XAttribute ("mmedtypeflags", HexNum (mmedtypeflagsXdf (tableType, MajorOrderXdf.Row))),
				         new XAttribute ("mmedaddress", HexNum (address)),
				         new XAttribute ("mmedelementsizebits", 8 * tableType.ValueSize ()));

			if (colcount > 0)
				el.Add (new XAttribute ("mmedcolcount", colcount));
			if (rowcount > 0)
				el.Add (new XAttribute ("mmedrowcount", rowcount));

			el.Add (new XAttribute ("mmedmajorstridebits", "0"),
				new XAttribute ("mmedminorstridebits", "0"));
			return el;
		}

		protected static int mmedtypeflagsXdf (TableType tableType, MajorOrderXdf majorOrder)
		{
			// unsigned: 0, signed: 1, LSB first: 2
			int flags = 0;

			switch (tableType) {
			case TableType.Int8:
			case TableType.Int16:
				flags = 0x1;
				break;

			case TableType.Float:
				flags = 0x10000;
				break;
			}

			// Major Order: Default = "Row"; "Column": 4
			if (majorOrder == MajorOrderXdf.Column)
				flags |= 0x4;
			return flags;
		}

		protected static XElement EmptyXAxisXdf ()
		{
			// indexcount = 1 is required:
			// <XDFAXIS id="x" uniqueid="0x0">
			//   <indexcount>1</indexcount>
			// </XDFAXIS>
			return new XElement ("XDFAXIS",
				new XAttribute ("id", AxisType.X.XdfStr ()),
				new XAttribute ("uniqueid", HexNum (0)),
				new XElement ("indexcount", "1"));
		}

		#endregion XDF

		public static void CalcMinMaxAverage (float[] values, out float minimum, out float maximum, out float average)
		{
			float min = FloatUndefined;
			float max = FloatUndefined;
			float sum = FloatUndefined;
			for (int i = 0; i < values.Length; i++) {
				float v = values [i];
				if (i == 0) {
					min = v;
					max = v;
					sum = v;
				} else {
					if (v < min)
						min = v;
					if (v > max)
						max = v;
					sum += v;
				}
			}
			minimum = min;
			maximum = max;
			average = sum / values.Length;
		}

		protected void CheckMAC ()
		{
			hasMAC = IsFloatValid (multiplier) && multiplier != 0f && IsFloatValid (offset);
		}
	}
}
