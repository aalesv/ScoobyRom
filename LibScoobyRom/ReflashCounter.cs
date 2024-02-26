// ReflashCounter.cs: Parse reflash counter.

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


namespace Subaru
{
	public sealed class ReflashCounter
	{
		readonly int pos;
		readonly System.IO.Stream stream;


		public ReflashCounter (RomType romType, System.IO.Stream stream) : this (GetPos (romType), stream)
		{
		}

		public ReflashCounter (int pos, System.IO.Stream stream)
		{
			this.pos = pos;
			this.stream = stream;
		}

		public int Position {
			get { return pos; }
		}

		public int? Read ()
		{
			const int Size = 4;

			if (pos < 0)
				return null;

			try {
				byte[] bytes = new byte[Size];
				stream.Position = pos;
				if (stream.Read (bytes, 0, Size) != Size)
					return null;
				// example: "00 01 FF FE" --> reflash count = 1

				ushort value = Util.BinaryHelper.UInt16BigEndian (bytes, 0);
				ushort valueNegated = Util.BinaryHelper.UInt16BigEndian (bytes, 2);

				return (value == 0xFFFF - valueNegated) ? (int?)value : null;
			} catch {
				return null;
			}
		}

		public static int GetPos (RomType romType)
		{
			switch (romType) {
			case RomType.SH7055:
				return 0x7FB00;
			case RomType.SH7058:
				return 0xFFB00;
			case RomType.SH7059:
				return 0x17FB00;
			default:
				return -1;
			}
		}
	}
}
