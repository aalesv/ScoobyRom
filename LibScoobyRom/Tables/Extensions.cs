// Extensions.cs: Extension helper methods for binary types.

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


// Not thread safe but slightly more efficient.
#define USE_STATIC_BUFFER

using System;
using System.IO;
using Tables;
using Util;

namespace Extensions
{
	public static class Extensions
	{
		#if USE_STATIC_BUFFER
		static readonly byte[] buf = new byte[4];
		#endif

		public static short ReadInt16LittleEndian (this Stream stream)
		{
			const int Length = 2;
			#if !USE_STATIC_BUFFER
			byte[] buf = new byte[Length];
			#endif
			stream.Read (buf, 0, Length);
			// could also use: return BitConverter.ToInt16 (buf, 0);
			return (short)((buf [1] << 8) | buf [0]);
		}

		public static short ReadInt16BigEndian (this Stream stream)
		{
			const int Length = 2;
			#if !USE_STATIC_BUFFER
			byte[] buf = new byte[Length];
			#endif
			stream.Read (buf, 0, Length);
			return BinaryHelper.Int16BigEndian (buf, 0);
		}

		public static int ReadInt32LittleEndian (this Stream stream)
		{
			const int Length = 4;
			#if !USE_STATIC_BUFFER
			byte[] buf = new byte[Length];
			#endif
			stream.Read (buf, 0, Length);
			return BitConverter.ToInt32 (buf, 0);
		}

		public static int ReadInt32BigEndian (this Stream stream)
		{
			const int Length = 4;
			#if !USE_STATIC_BUFFER
			byte[] buf = new byte[Length];
			#endif
			stream.Read (buf, 0, Length);
			return BinaryHelper.Int32BigEndian (buf, 0);
		}

		public static float ReadSingleBigEndian (this Stream stream)
		{
			const int Length = 4;
			#if !USE_STATIC_BUFFER
			byte[] buf = new byte[Length];
			#endif
			stream.Read (buf, 0, Length);
			return BinaryHelper.SingleBigEndian (buf, 0);
		}

		public static bool IsValid (this TableType tableType)
		{
			return (tableType.ValueSize () > 0);
		}

		public static int ValueSize (this TableType tableType)
		{
			switch (tableType) {
			case TableType.Float:
			case TableType.UInt32:
				return 4;
			case TableType.UInt8:
			case TableType.Int8:
				return 1;
			case TableType.UInt16:
			case TableType.Int16:
				return 2;
			default:
				// unknown, invalid
				return 0;
			}
		}

		public static string XdfStr (this AxisType axisType)
		{
			switch (axisType) {
			case AxisType.X:
				return "x";
			case AxisType.Y:
				return "y";
			case AxisType.Z:
				return "z";
			default:
				throw new ArgumentOutOfRangeException ("axisType");
			}
		}

		public static string RRStr (this AxisType axisType)
		{
			switch (axisType) {
			case AxisType.X:
				return "X Axis";
			case AxisType.Y:
				return "Y Axis";
			default:
				throw new ArgumentOutOfRangeException ("axisType");
			}
		}
	}
}
