// Rom.cs: ROM class - read/analyze ROM file content.

/* Copyright (C) 2011-2016 SubaruDieselCrew
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
using System.ComponentModel;
using System.IO;
using Tables.Denso;

namespace Subaru.File
{
	public sealed class Rom : IDisposable
	{
		public const int KiB = 1024;
		public const int MiB = KiB * KiB;

		/// <summary>
		/// Progress info while ROM is being analyzed.
		/// </summary>
		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

		string path;
		Stream stream;
		RomType romType;
		// ROM files are much smaller (couple MiB max) than 2 GiB so Int32 for pos, size etc. is sufficient
		int startPos, lastPos;
		int percentDoneLastReport;
		DateTime? romDate;
		int? reflashCount;
		RomRaiderEditStamp.RomRaiderEditStampData? romRaiderEditStampData;

		public string Path {
			get { return this.path; }
		}

		public Stream Stream {
			get { return this.stream; }
		}

		public int Size {
			get { return (int)this.stream.Length; }
		}

		public RomType RomType {
			get { return this.romType; }
		}

		public RomChecksumming RomChecksumming {
			get { return new RomChecksumming (this.romType, this.stream); }
		}

		public DateTime? RomDate {
			get { return this.romDate; }
		}

		public string RomDateStr {
			get { return this.romDate.HasValue ? this.romDate.Value.ToString ("yyyy-MM-dd") : "-"; }
		}

		public int? ReflashCount {
			get { return reflashCount; }
		}

		public string ReflashCountStr {
			get { return reflashCount.HasValue ? reflashCount.Value.ToString () : "-"; }
		}

		public string RomRaiderEditStampStr {
			get { return romRaiderEditStampData.HasValue ? romRaiderEditStampData.Value.ToString () : "-"; }
		}

		public Rom (string path)
		{
			this.path = path;
			Open ();
		}

		void Open ()
		{
			const int BufferSize = 16 * KiB;

			// Copy whole file into MemoryStream at once for best performance.
			// As Stream has .Position property, it makes sence to use this instead of byte[] + own pos variable.
			byte[] buffer;
			int length;
			using (FileStream fstream = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan)) {
				length = (int)fstream.Length;
				buffer = new byte[length];
				if (fstream.Read (buffer, 0, length) != length)
					throw new IOException ("stream.Read length mismatch");
			}
			this.stream = new MemoryStream (buffer, 0, length, false, true);

			this.romType = DetectRomType (stream);
		}

		/// <summary>
		/// Read raw bytes and convert to string.
		/// (Ex: Calibration ID)
		/// </summary>
		/// <param name="pos">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <param name="length">
		/// A <see cref="System.Int32"/>
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public string ReadASCII (long pos, int length)
		{
			stream.Position = pos;
			byte[] bytes = new byte[length];
			stream.Read (bytes, 0, length);
			return System.Text.Encoding.ASCII.GetString (bytes);
		}

		public void Close ()
		{
			if (stream != null) {
				stream.Dispose ();
				stream = null;
			}
		}

		void CheckProgress (long pos)
		{
			if (ProgressChanged == null)
				return;

			int percentDone = (int)(100f * (((float)pos - (float)startPos)) / ((float)lastPos - (float)startPos));
			if (percentDone >= percentDoneLastReport + 10) {
				percentDoneLastReport = percentDone;
				OnProgressChanged (percentDone);
			}
		}

		void OnProgressChanged (int percentDone)
		{
			if (ProgressChanged == null)
				return;
			else
				ProgressChanged (this, new ProgressChangedEventArgs (percentDone, null));
		}

		/* //not used so far

		public IList<Table3D> FindMaps3D ()
		{
			return FindMaps3D (0);
		}

		public IList<Table3D> FindMaps3D (int startPos)
		{
			// should stop at EOF
			return FindMaps3D (startPos, int.MaxValue);
		}

		public IList<Table3D> FindMaps3D (int startPos, int lastPos)
		{
			this.startPos = startPos;
			lastPos = Math.Min (lastPos, Size - 1);
			this.lastPos = lastPos;
			OnProgressChanged (0);
			this.percentDoneLastReport = 0;
			List<Table3D> list3D = new List<Table3D> (800);

			for (long pos = startPos; pos <= lastPos;) {
				// check for end of file
				if (pos >= stream.Length)
					break;
				stream.Position = pos;
				CheckProgress (pos);

				Table3D info3D = Table3D.TryParseValid (this.stream);
				if (info3D != null) {
					list3D.Add (info3D);
					pos = stream.Position;
				} else {
					// not valid, try at next possible location
					pos++;
				}
			}
			OnProgressChanged (100);
			return list3D;
		}
		*/

		public void FindMaps (Util.Range? tableSearchRange, out IList<Table2D> list2D, out IList<Table3D> list3D)
		{
			int startPos, lastPos;
			if (tableSearchRange.HasValue) {
				startPos = tableSearchRange.Value.Pos;
				lastPos = tableSearchRange.Value.Last;
			} else {
				// search through whole ROM file
				startPos = 0;
				lastPos = int.MaxValue;
			}
			FindMaps (startPos, lastPos, out list2D, out list3D);
		}

		public void FindMaps (int startPos, int lastPos, out IList<Table2D> list2D, out IList<Table3D> list3D)
		{
			this.startPos = startPos;
			// smallest 2D table record size is 12 bytes
			lastPos = Math.Min (lastPos, Size - 1 - 12);
			this.lastPos = lastPos;

			// Restricting pointers to a realistic range improves detection accuracy.
			// Using conservative settings here:
			// In Subaru ROMs at least first 8 KiB usually contain low level stuff, no maps.
			Table.PosMin = 8 * KiB;
			Table.PosMax = Size - 1;

			// default capacities suitable for current (MY2015 2MiB) diesel ROMs
			list2D = new List<Table2D> (1500);
			list3D = new List<Table3D> (1300);

			OnProgressChanged (0);
			this.percentDoneLastReport = 0;

			// records need to be 4-byte-aligned anyway
			// --> much faster than just ++pos, also skips a few false positives
			for (long pos = startPos; pos <= lastPos; pos = NextAlignedPos (pos, 4)) {
				stream.Position = pos;
				CheckProgress (pos);

				// try Table3D first as it contains more struct info; more info to validate = better detection
				Table3D info3D = Table3D.TryParseValid (this.stream);
				if (info3D != null) {
					list3D.Add (info3D);
					// this is often a valid next pos, already aligned
					pos = stream.Position;
				} else {
					// must back off
					stream.Position = pos;
					// not 3D, try 2D
					Table2D info2D = Table2D.TryParseValid (this.stream);
					if (info2D != null) {
						list2D.Add (info2D);
						// this is often a valid next pos, already aligned
						pos = stream.Position;
					} else {
						// nothing valid, try at next possible location
						pos++;
					}
				}
			}
			OnProgressChanged (100);
		}

		/// <summary>
		/// Calculates the next aligned position, which is greater than or equal given position.
		/// </summary>
		/// <returns>The aligned position.</returns>
		/// <param name="pos">Position or memory address.</param>
		/// <param name="align">Alignment in bytes, &gt; 0, i.e. 4 for many 32 bit architectures.</param>
		public static long NextAlignedPos (long pos, int align)
		{
			if (align < 0)
				throw new ArgumentOutOfRangeException ("align");
			long mod = pos % align;
			return mod == 0 ? pos : pos - mod + align;
		}

		static bool IsASCIIPrintable (char c)
		{
			return (c >= ' ' && c <= '~');
		}

		static bool Predicate (char c)
		{
			return (IsASCIIPrintable (c) && (char.IsLetterOrDigit (c) || char.IsWhiteSpace (c) || char.IsPunctuation (c)));
		}

		static bool CheckString (string s, Func<char, bool> predicate)
		{
			for (int i = 0; i < s.Length; i++) {
				if (!predicate (s [i]))
					return false;
			}
			return true;
		}

		static void PrintStringInfo (string s, long pos)
		{
			Console.WriteLine ("ASCII [{0}] \"{1}\" @ 0x{2:X}", s.Length.ToString (), s, pos);
		}

		bool FindExtendASCII (Stream stream, string find, out string found, out long position)
		{
			int? stringPos = Util.SearchBinary.FindASCII (stream, find);
			if (stringPos.HasValue) {
				stream.Position = stringPos.Value;
				found = Util.SearchBinary.ExtendFindASCII (stream, Predicate);
				position = stream.Position - found.Length;
				return true;
			} else {
				found = null;
				position = -1;
				return false;
			}
		}

		public void FindMetadata ()
		{
			const string DensoASCII = "DENSO";
			// Euro5 (not Euro4) diesel as well as newer petrol models have System String
			const string DieselASCII = "DIESEL";
			const string TurboASCII = "TURBO";

			long posDenso = 0;
			long pos;
			string strFound;

			stream.Position = 0;
			if (FindExtendASCII (stream, DensoASCII, out strFound, out pos)) {
				PrintStringInfo (strFound, pos);
				// sometimes i.e. first char is ASCII but wrong, should start with 'C', for "Copr." or similar
				posDenso = pos;
				int i = strFound.IndexOf ('C');
				if (i > 0 && i < 4) {
					posDenso += i;
				}
			}

			stream.Position = 0;
			if (FindExtendASCII (stream, DieselASCII, out strFound, out pos)) {
				PrintStringInfo (strFound, pos);
			}

			stream.Position = 0;
			if (FindExtendASCII (stream, TurboASCII, out strFound, out pos)) {
				PrintStringInfo (strFound, pos);
			}

			// 0x4000 = 16 KiB
			const int Pos_SH7058_Diesel = 0x4000;
			// 0x8000 = 32 KiB
			const int Pos_SH72543R_Diesel = 0x8000;

			const int RomIDlongLength = 32;
			const int CIDLength = 8;

			switch (RomType) {
			case RomType.SH7058:
			case RomType.SH7059:
				pos = Pos_SH7058_Diesel;
				break;
			case RomType.SH72543R:
				pos = Pos_SH72543R_Diesel;
				break;
			default:
				Console.WriteLine ("Unknown RomType");
				return;
			}


			string romIDlong = ReadASCII (pos, RomIDlongLength).TrimEnd ();
			if (CheckString (romIDlong, Predicate)) {
				Console.WriteLine ("RomIDlong[{0}]: {1}", romIDlong.Length.ToString (), romIDlong);
			}

			string CID = romIDlong.Substring (romIDlong.Length - CIDLength);
			if (CheckString (CID, Predicate)) {
				Console.WriteLine ("CID: {0}", CID);
			}

			// HACK
			bool isDiesel = posDenso > 0x4000;

			if (posDenso > 0) {
				try {
					stream.Position = posDenso - 3;
					DateTime? date;

					if (isDiesel) {
						date = Util.BinaryHelper.ParseDate (stream);
					} else {
						date = Util.BinaryHelper.ParseDatePackedNaturalBCD (stream);
					}

					this.romDate = date;
					Console.WriteLine ("RomDate: {0}", RomDateStr);
				} catch (Exception) {
					Console.Error.WriteLine ("Exception: RomDate failed");
				}
			}

			if (posDenso > 0) {
				try {
					var reflashCounterObj = new ReflashCounter (romType, stream);
					this.reflashCount = reflashCounterObj.Read ();
					Console.WriteLine ("ReflashCount @ 0x{0:X} = {1}", reflashCounterObj.Position, ReflashCountStr);
				} catch (Exception) {
					Console.Error.WriteLine ("Exception: ReflashCount failed");
				}
			}

			try {
				var obj = new RomRaiderEditStamp (romType, stream);
				this.romRaiderEditStampData = obj.Read ();
				Console.WriteLine ("RomRaiderEditStamp @ 0x{0:X} = {1}", obj.Position, romRaiderEditStampData);
			} catch (Exception) {
				Console.Error.WriteLine ("Exception: RomRaiderEditStamp failed");
			}

			byte[] searchBytes = new byte[] { 0xA2, 0x10, 0x14 };
			stream.Position = 0;
			var ssmIDpos = Util.SearchBinary.FindBytes (stream, searchBytes);
			Console.WriteLine ("Diesel SSMID pos: 0x{0:X}", ssmIDpos);

			const int RomIDLength = 5;
			byte[] romidBytes = new byte[RomIDLength];
			long romid;
			if (ssmIDpos.HasValue) {
				stream.Position = ssmIDpos.Value + 3;
				if (stream.Read (romidBytes, 0, RomIDLength) == RomIDLength) {
					// parse value from 5 bytes, big endian
					romid = ((long)(romidBytes [0]) << 32) + Util.BinaryHelper.Int32BigEndian (romidBytes, 1);
					Console.WriteLine ("ROMID: {0}", romid.ToString ("X10"));
				}
			}
		}


		#region IDisposable implementation

		void IDisposable.Dispose ()
		{
			Close ();
		}

		#endregion

		/// <summary>
		/// Currently only looks at size.
		/// </summary>
		/// <param name="stream">
		/// A <see cref="Stream"/>
		/// </param>
		/// <returns>
		/// A <see cref="RomType"/>
		/// </returns>
		public RomType DetectRomType (Stream stream)
		{
			try {
				int length = Size;
				switch (length) {
				case 512 * KiB:
					return RomType.SH7055;
				case 1 * MiB:
					return RomType.SH7058;
				case (1024 + 512) * KiB:
					return RomType.SH7059;
				case 1280 * KiB:
					return RomType.SH72531;
				case 2 * MiB:
					return RomType.SH72543R;
				default:
					return RomType.Unknown;
				}
			} catch (NotSupportedException) {
				return RomType.Unknown;
			}
		}
	}
}