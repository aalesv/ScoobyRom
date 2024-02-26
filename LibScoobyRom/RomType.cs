// RomType.cs: enum

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

namespace Subaru
{
	public enum RomType
	{
		Unknown,
		/// <summary>
		/// 512 KiB = 524288 bytes
		/// </summary>
		SH7055,
		/// <summary>
		/// 1 MiB = 1024 KiB = 1048576 bytes
		/// </summary>
		SH7058,
		/// <summary>
		/// 1.5 MiB = 1536 KiB = 1572864 bytes
		/// </summary>
		SH7059,
		/// <summary>
		/// 1.25 MiB = 1280 KiB = 1310720 bytes
		/// </summary>
		SH72531,
		/// <summary>
		/// 2 MiB = 2048 KiB = 2097152 bytes
		/// </summary>
		SH72543R
	}
}
