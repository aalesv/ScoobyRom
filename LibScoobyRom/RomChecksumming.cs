// RomChecksumming.cs: Calculate checksums and related things.

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
using System.IO;
using Extensions;
using Subaru.File;

namespace Subaru
{
	public class RomChecksumming
	{
		public const int SizeOfNativeStruct = 3 * sizeof(UInt32);
		//public const int ChecksumTableRecordCount = 17;
		public const int ChecksumConstant = 0x5AA5A55A;

		int[] tablePos;
		System.IO.Stream stream;
		List<RomChecksumRecord> checksumRecords;
		RomType romType;

		//Must correspond GetTablePos
		public static int[] ChecksumTableRecordCount (RomType romType)
		{
			switch (romType) {
			case RomType.SH7055:
			case RomType.SH7058:
			case RomType.SH7059:
				return new int[] {17};
			case RomType.SH72531:
				return new int[] {17, 2};
			//TODO check this
			case RomType.SH72543R:
				return new int[] {17};
			case RomType.MPC5746_1552:
			case RomType.MPC5746_3984:
				return new int[] {17, 2};
			default:
				return new int[] {};
			}
		}

		public int[] ChecksumTableRecordCount()
		{
			return ChecksumTableRecordCount(romType);
		}

		public RomChecksumming (RomType romType, System.IO.Stream stream) : this (GetTablePos (romType), stream)
		{
			this.romType = romType;
		}

		public RomChecksumming (int[] tablePos, System.IO.Stream stream)
		{
			this.tablePos = tablePos;
			this.stream = stream;
		}

		//Must correspond ChecksumTableRecordCount
		public static int[] GetTablePos (RomType romType)
		{
			switch (romType) {
			case RomType.SH7055:
				return new int[] {0x7FB80};
			case RomType.SH7058:
				return new int[] {0xFFB80};
			case RomType.SH7059:
				return new int[] {0x17FB80};
			case RomType.SH72531:
				return new int[] {0x13F500, 0x13F5E8};
			//TODO check this
			case RomType.SH72543R:
				return new int[] {0x1FF800};
			case RomType.MPC5746_1552:
				return new int[] {0x183E00, 0x183EE0};
			case RomType.MPC5746_3984:
				return new int[] {0x3E3E00, 0x3E3EE0};
			default:
				return new int[] {};
			}
		}

		public IList<RomChecksumRecord> ReadTableRecords ()
		{
			//return ReadTableRecords (this.tablePos, ChecksumTableRecordCount);
			List<RomChecksumRecord> records = new List<RomChecksumRecord> ();
			for (int i = 0; i < this.tablePos.Length; i++) {
				IList<RomChecksumRecord> r = ReadTableRecords(this.tablePos[i],
													this.ChecksumTableRecordCount()[i]
												);
				//records = records.Concat(r);
				records.AddRange(r);
			}
			return records;
		}

		/*public IList<RomChecksumRecord> ReadTableRecords (int count)
		{
			return ReadTableRecords (this.tablePos, count);
		}*/

		public IList<RomChecksumRecord> ReadTableRecords (int pos, int count)
		{
			List<RomChecksumRecord> records = new List<RomChecksumRecord> (count);

			int offset = Rom.RomLoadAddress(this.romType);
			//stream.Seek (tablePos, System.IO.SeekOrigin.Begin);
			stream.Seek (pos, System.IO.SeekOrigin.Begin);
			for (int i = 0; i < count; i++) {
				int _start 	  = stream.ReadInt32BigEndian (),
					_end	  = stream.ReadInt32BigEndian ();
				//0 means that checksum is disabled, keep it.
				if (_start == 0 && _end == 0){
					offset = 0;
				}
				int start 	 = _start - offset,
					end		 = _end - offset,
					checkSum = stream.ReadInt32BigEndian ();
				RomChecksumRecord record = new RomChecksumRecord (start, end, checkSum);
				records.Add (record);
			}
			this.checksumRecords = records;
			return records;
		}

		public int CalcSum (RomChecksumRecord record)
		{
			int sum = 0;
			if (!record.IsEmpty) {
				stream.Seek (record.StartAddress, SeekOrigin.Begin);
				for (int p = record.StartAddress; p < record.EndAddress; p += sizeof(Int32)) {
					sum += stream.ReadInt32BigEndian ();
				}
			}
			return sum;
		}

		public int CalcChecksumValue (RomChecksumRecord record)
		{
			return ChecksumConstant - CalcSum (record);
		}

		public bool IsValid (RomChecksumRecord record)
		{
			return CalcChecksumValue (record) == record.Checksum;
		}

		/// <summary>
		/// Calculates Calibration Verification Number of 8-byte-type..
		/// This type of CVN is being used on diesel models.
		/// </summary>
		/// <returns>
		/// A <see cref="System.Int64"/>.
		/// To get CVN standard string "XXXXXXXX XXXXXXXX" use .ToString ("X16").Insert (8, " ");
		/// </returns>
		public long CalcCVN8 ()
		{
			// Could derive #3 from record.Checksum: sum3 = 0x5AA5A55A - record.Checksum
			// Must not extend as signed for last line to work correctly.
			ulong sum3 = (ulong)(uint)CalcSum (checksumRecords [3]);

			// #4: Actual sum needed, record.Checksum is different!
			ulong sum4 = (ulong)(uint)CalcSum (checksumRecords [4]);
			//Console.WriteLine ("sum3: {0:X8} sum4: {1:X8} sum3<<32: {2:X8}", sum3, sum4, sum3 << 32);

			// Use return type long (CLS-compliant) instead of ulong as it doesn't matter.
			return (long)((sum3 << 32) | sum4);
		}

		/// <summary>
		/// Makes CVN standard string "XXXXXXXX XXXXXXXX" out of given value.
		/// </summary>
		/// <param name="cvn">
		/// A <see cref="System.Int64"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public static string CVN8Str (long cvn)
		{
			return cvn.ToString ("X16").Insert (8, " ");
		}
	}
}
