// ActionEventArgs.cs: EventArgs class

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

namespace ScoobyRom
{
	// TODO flesh out or find pre-defined one
	public sealed class ActionEventArgs : EventArgs
	{
		object tag;

		public object Tag {
			get { return this.tag; }
		}

		public ActionEventArgs (object tag)
		{
			this.tag = tag;
		}
	}
}