// RomRaiderEditStamp.cs: Parse RomRaider Edit Stamp.

/* Copyright (C) 2011-2017 SubaruDieselCrew
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


namespace Subaru
{
	public sealed class RomRaiderEditStamp
	{

		public struct RomRaiderEditStampData
		{
			public DateTime Date { get; set; }

			public sbyte Count { get; set; }

			public override string ToString ()
			{
				return Date.ToString ("yyyy-MM-dd") + " v" + Count.ToString ();
			}
		}


		readonly int pos;
		readonly System.IO.Stream stream;

		public RomRaiderEditStamp (RomType romType, System.IO.Stream stream) : this (GetPos (romType), stream)
		{
		}

		public RomRaiderEditStamp (int pos, System.IO.Stream stream)
		{
			this.pos = pos;
			this.stream = stream;
		}

		public int Position {
			get { return pos; }
		}

		public RomRaiderEditStampData? Read ()
		{
			if (pos < 0)
				return null;
			RomRaiderEditStampData stamp = new RomRaiderEditStampData();
			try {
				stream.Position = pos;
				// example: "16 03 24 02" --> "2016-03-24 v2"
				// yy, MM, dd
				stamp.Date = Util.BinaryHelper.ParseDatePackedNaturalBCD (stream);
				int count = stream.ReadByte ();
				if (count < 0 || count > sbyte.MaxValue)
					count = 0;
				stamp.Count = (sbyte)count;
			} catch {
				return null;
			}
			return stamp;
		}

		public static int GetPos (RomType romType)
		{
			int pos = RomChecksumming.GetTablePos (romType);
			if (pos >= 0)
				pos += RomChecksumming.ChecksumTableRecordCount * RomChecksumming.SizeOfNativeStruct;
			return pos;
		}
	}
}
