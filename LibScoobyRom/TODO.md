# LibScoobyRom

Author: <http://subdiesel.wordpress.com/>

Project homepage on *GitHub*: <http://github.com/SubaruDieselCrew/ScoobyRom/>

# To Do / Investigate

*	provide detected (diesel) ROM metadata for UI and new XML

*	Add CVN calculation methods for gasoline ECU ROMs (ranges might be hardcoded though, therefore not feasible)

*	Make some properties adjustable.
   E.g. validation min/max numbers, take file size into account etc.

*	Improve success probability.
   Might use more range overlap checking to increase table type detection accuracy.

*	Remove metadata fields from ROM specific table record structs.
   Use wrapper class/struct in UI app instead.
   That way library and UI app don't need so much type features synchronization.

*	Add more IDA (IDC script) features.
