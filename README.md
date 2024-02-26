# ScoobyRom

![](ScoobyRom/Images/AppIcon.png)

*	Author: subdiesel, Subaru Diesel Crew <http://subdiesel.wordpress.com/>

*	ScoobyRom Homepage: <http://subdiesel.wordpress.com/ecu-analysis/scoobyrom-software/>

*	Source code repository and downloads: <http://github.com/SubaruDieselCrew/ScoobyRom/>

# Quick Facts

## CONTENTS

1.	[Purpose](#purpose)

2.	[License](#license)

3.	[Further Details](#details)

---

## <a name="purpose"></a> 1) Purpose

*ScoobyRom* is a car control unit ([ECU](http://en.wikipedia.org/wiki/Engine_Control_Unit), [TCU](http://en.wikipedia.org/wiki/Transmission_Control_Unit)) firmware (ROM) **data visualisation viewer and metadata editor**.
Currently it is very [*Denso*](http://en.wikipedia.org/wiki/Denso) specific.

Originally designed for *Subaru* Diesel (Euro 4 & 5) ROMs, some *Subaru* petrol models, as well as ROMs from other manufacturers like *Mazda*, *Mitsubishi*, *Nissan* etc. have been tested working (where *Denso* supplied control units).
In general, different car models are equipped with different ECUs (hardware and/or firmware), therefore success varies.

Main features

*	Find and visualise 2D (x-y) and 3D (x-y-z) tables ("maps").
*	Allows adding metadata (category, title, axes names, units, description text) as ROMs (= compiled software + calibration data such as tables) do not contain such annotations.
*	Display and verify checksums.

ROM memory model is supposed to be **32 bit, big endian**, others are unlikely to work.

ROM types confirmed working:

* *Engine Control Unit* (ECU):
	*	petrol and diesel models
	*	*Renesas* microcontrollers
		*	*SH7055* (512 KiB)
		*	*SH7058, SH7058S* (1.0 MiB)
		*	*SH7059* (1.5 MiB)

* *Transmission Control Unit* (TCU):
	*	Automatic Transmission (*Subaru 5AT*) (*SH7058*, 1.0 MiB)

However you can try any file safely as it is being opened in **read-only** mode.
Worst thing that can happen is the app finds nothing at all or false items only.

This application is **not a ROM editor** (yet), you cannot change table values or modify a ROM in any way!
Remember, in this version the ROM file is only being read.
All additional data is saved into an extra XML file.
However, ScoobyRom **supports multiple ROM editors via "export as"** features:

a)	*RomRaider ECU definition* format

"RomRaider is a free, open source tuning suite created for viewing, logging and tuning of modern Subaru Engine Control Units and some older BMW M3 (MS41/42/43) DME."

<http://www.romraider.com>

b)	*TunerPro XDF* format

"TunerPro is a free, donate-if-you-like-it tuning platform aimed at making tuning easier and cheaper for the hobbyist and professional alike. It uses a versatile and extensible calibration definition format (XDF) that is user-defineable and is quite powerful."

<http://tunerpro.net/>

---

## <a name="license"></a> 2) License

GPLv3. See text file [COPYING.txt](COPYING.txt) for license text.

[http://fsf.org/](http://fsf.org/ "Free Software Foundation")

You can also get license text in different formats and further details there.

[http://www.gnu.org/licenses/gpl.html](http://www.gnu.org/licenses/gpl.html)

[http://www.gnu.org/licenses/gpl-faq.html](http://www.gnu.org/licenses/gpl-faq.html)

---

## <a name="details"></a> 3) Further Details

See `README.md` and other documentation files (`*.md`) in project subfolders.

Main documentation:

*	[ScoobyRom/README.md](ScoobyRom/README.md) (click this on *GitHub* - it will render as HTML in web browser)

Following formats are also included in binary download package (`.ZIP`) for convenience, automatically generated from above Markdown source:

*	ScoobyRom/README.html
