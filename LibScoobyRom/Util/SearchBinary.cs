// SearchBinary.cs: Search for bytes, strings etc.

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
using System.IO;

namespace Util
{
	public static class SearchBinary
	{
		/// <summary>
		/// Returns the stream position of the first occurence of specified bytes.
		/// </summary>
		/// <returns>The position or null if not found.</returns>
		/// <param name="stream">Data.</param>
		/// <param name="target">Values to search for.</param>
		public static int? FindBytes (Stream stream, byte[] target)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			if (target == null)
				throw new ArgumentNullException ("target");
			if (target.Length == 0)
				throw new ArgumentOutOfRangeException ("target", "target.Length == 0");

			int firstByteTarget = target [0];
			int currentByte;
			bool match;
			while ((currentByte = stream.ReadByte ()) >= 0) {
				if (currentByte != firstByteTarget)
					continue;
				match = true;
				for (int i = 1; i < target.Length; i++) {
					currentByte = stream.ReadByte ();
					if (currentByte < 0)
						return null;
					if (currentByte != target [i]) {
						match = false;
						stream.Position -= i;
						break;
					}
				}
				if (match)
					return (int)(stream.Position) - target.Length;

			}
			return null;
		}

		/// <summary>
		/// Returns the stream position of the first occurence of specified string.
		/// </summary>
		/// <returns>The position or null if not found.</returns>
		/// <param name="stream">Data.</param>
		/// <param name="target">String to search for.</param>
		public static int? FindASCII (Stream stream, string target)
		{
			return FindBytes (stream, System.Text.Encoding.ASCII.GetBytes (target));
		}

		public static byte[] ExtendFind (Stream stream, Func<byte, bool> check)
		{
			int currentByte;

			long leftPos = stream.Position;

			// left
			while ((currentByte = stream.ReadByte ()) >= 0 && check ((byte)currentByte)) {
				leftPos = stream.Position - 1;
				stream.Position = leftPos - 1;
			}

			// right
			while ((currentByte = stream.ReadByte ()) >= 0 && check ((byte)currentByte)) {
			}

			int count = (int)(stream.Position - leftPos) - 1;
			if (count < 1)
				return null;

			byte[] bytes = new byte[count];
			stream.Position = leftPos;
			if (stream.Read (bytes, 0, count) != count)
				throw new IOException ("byte count mismatch");

			return bytes;
		}

		public static string ExtendFindASCII (Stream stream, Func<char, bool> check)
		{
			byte[] result = ExtendFind (stream, b => check ((char)b));
			return System.Text.Encoding.ASCII.GetString (result);
		}
	}
}