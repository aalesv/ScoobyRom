// BinaryHelper.cs: Value types out of bytes.

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
using System.Collections.Generic;

namespace Util
{
	public static class BinaryHelper
	{
		public static short Int16BigEndian (IList<byte> bytes, int index)
		{
			if (index + 1 >= bytes.Count || index < 0)
				throw new ArgumentOutOfRangeException ();
			return (short)(bytes [index + 1] | bytes [index] << 8);
		}

		public static ushort UInt16BigEndian (IList<byte> bytes, int index)
		{
			return (ushort)Int16BigEndian (bytes, index);
		}

		public static int Int32BigEndian (IList<byte> bytes, int index)
		{
			if (index + 3 >= bytes.Count || index < 0)
				throw new ArgumentOutOfRangeException ();
			// return (bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24); // normal = little-endian
			return bytes [index + 3] | bytes [index + 2] << 8 | bytes [index + 1] << 16 | bytes [index] << 24;
		}

		public static uint UInt32BigEndian (IList<byte> bytes, int index)
		{
			return (uint)Int32BigEndian (bytes, index);
		}

		public static float SingleBigEndian (IList<byte> bytes, int index)
		{
			if (index + 3 >= bytes.Count || index < 0)
				throw new ArgumentOutOfRangeException ();
			byte[] reordered = new byte[4];
			reordered [0] = bytes [index + 3];
			reordered [1] = bytes [index + 2];
			reordered [2] = bytes [index + 1];
			reordered [3] = bytes [index];
			// BitConverter.ToSingle assumes machine (little endian) order
			return BitConverter.ToSingle (reordered, 0);
		}

		/// <summary>
		/// Parses a packed BCD byte.
		/// "Natural BCD (NBCD) (Binary Coded Decimal)", also called "8421" encoding.
		/// http://en.wikipedia.org/wiki/Binary-coded_decimal
		/// Example: input hex 0x98 yields decimal 98.
		/// </summary>
		/// <returns>The parsed value.</returns>
		/// <param name="hexValue">source value, must be in range 0x00..0x99</param>
		public static byte ParsePackedNaturalBCD (byte hexValue)
		{
			byte nibbleLow = (byte)(hexValue & 0x0F);
			byte nibbleHigh = (byte)((hexValue >> 4) & 0x0F);
			if (nibbleLow > 9 || nibbleHigh > 9)
				throw new ArgumentOutOfRangeException ("hexValue", "not a valid packed natural BCD byte");
			//return int.Parse (hexValue.ToString ("X"));
			return (byte)(10 * nibbleHigh + nibbleLow);
		}

		/// <summary>
		/// Parses a date by reading 3 bytes in order "yyMMdd".
		/// </summary>
		/// <returns>The date.</returns>
		/// <param name="stream">Input stream.</param>
		public static DateTime ParseDate (System.IO.Stream stream)
		{
			int year = stream.ReadByte () + 2000;
			int month = stream.ReadByte ();
			int day = stream.ReadByte ();
			return new DateTime (year, month, day);
		}

		/// <summary>
		/// Parses a date by reading 3 packed natural BCD bytes in order "yyMMdd".
		/// </summary>
		/// <returns>The date.</returns>
		/// <param name="stream">Input stream.</param>
		public static DateTime ParseDatePackedNaturalBCD (System.IO.Stream stream)
		{
			int year = ParsePackedNaturalBCD ((byte)stream.ReadByte ()) + 2000;
			int month = ParsePackedNaturalBCD ((byte)stream.ReadByte ());
			int day = ParsePackedNaturalBCD ((byte)stream.ReadByte ());
			return new DateTime (year, month, day);
		}
	}
}
