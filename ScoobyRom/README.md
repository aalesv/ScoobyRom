﻿# ScoobyRom

![](Images/AppIcon.png)

*	Author: subdiesel, Subaru Diesel Crew <http://subdiesel.wordpress.com/>

*	ScoobyRom Homepage: <http://subdiesel.wordpress.com/ecu-analysis/scoobyrom-software/>

*	Source code repository and downloads: <http://github.com/SubaruDieselCrew/ScoobyRom/>

# Documentation

## CONTENTS

1.	[Purpose](#purpose)

2.	[License](#license)

3.	[Installation](#installation)

	3.1	[Binary archive](#binary)

	3.2	[Source Code](#source)

4.	[Dependencies](#dependencies)

	4.1 [.NET or compatible runtime](#dotnet)

	4.2 [Gtk#](#gtksharp)

	4.3 [Florence](#florence)

	4.4 [gnuplot](#gnuplot)

5.	[Files](#files)

	5.1 [ScoobyRom.exe.config](#config)

	5.2 [ScoobyRom XML](#scoobyrom_xml)

	5.3 [RomRaider ECU def export](#romraider_export)

	5.4 [TunerPro XDF export](#tunerpro_export)

	5.5 [SVG export](#svg_export)

	5.6 [Other export formats](#other_export)

	5.7 [Temporary files](#temp_files)

	5.8 [gnuplot template files](#gnuplot_template_files)

6.	[Launch and Command Line Parameters](#launch)
7.	[User Interface Hints](#ui)

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
		*	*SH72531* (1.25 MiB)

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

c) *CSV* plain-text format

Comma-separated values (CSV) is a text file format that uses commas to separate values, and newlines to separate records. Each row contains a record about one table.

<http://tunerpro.net/>

---

## <a name="license"></a> 2) License

GPLv3. See text file [COPYING.txt](COPYING.txt) for license text.

[http://fsf.org/](http://fsf.org/ "Free Software Foundation")

You can also get license text in different formats and further details there.

[http://www.gnu.org/licenses/gpl.html](http://www.gnu.org/licenses/gpl.html)

[http://www.gnu.org/licenses/gpl-faq.html](http://www.gnu.org/licenses/gpl-faq.html)

---

## <a name="installation"></a> 3) Installation

### <a name="binary"></a> 3.1) Binary archive

REQUIRED!

There is no installer but the process is easy:
Extract the archive (`.zip`) into a new folder, location does not matter.

* Windows 32 bit example: `C:\Program Files\ScoobyRom`
* Windows x64 example: `C:\Program Files (x86)\ScoobyRom`

No write access is needed there other than in case you need to edit configuration file `ScoobyRom.exe.config` (see chapter 5.1).

#### IMPORTANT
Make sure that you've got all required dependencies installed,
**.NET 4.0** and **Gtk#** at least, see chapter 4.

#### Architecture (32 / 64 bit)

ScoobyRom is compiled as 32 bit application due to dependency *Gtk#* being limited to 32 bit. (There is no official 64 bit version so far. Underlying native *Gtk+* toolkit is available as 64 bit nowadays, though.)
This applies only to the ScoobyRom (GUI app) itself. Extra plot windows are generated by
automatically launching independent external tool _gnuplot_, therefore *gnuplot* x64 is working fine and recommended on 64 bit operating systems.


### <a name="source"></a> 3.2) Source Code

OPTIONAL.

Project homepage on *GitHub*: [https://github.com/aalesv/ScoobyRom][project homepage]

Project homepage on *GitHub*, deprecated: [https://github.com/SubaruDieselCrew/ScoobyRom/][project homepage]

#### Download
* Advanced: Clone source repository, assuming [git](http://www.git-scm.com/) is installed:

	`git clone https://github.com/aalesv/ScoobyRom`

Or you can use a graphical git interface, for example [GitHub Desktop](https://desktop.github.com/)

* Easy: Download sources (snapshot, no history) as ZIP archives download:

	* Visit [project homepage].
	* Click "Releases"

		or
	* Select desired *branch* and/or *tag* (top left).
		* On the right pane click the "**Download ZIP**" button.

[project homepage]: https://github.com/aalesv/ScoobyRom

[project homepage, deprecated]: https://github.com/SubaruDieselCrew/ScoobyRom/

#### Compilation
For instructions and hints see file [DEVELOPMENT.md](DEVELOPMENT.md).

If the environment is configured correctly, run `build.bat`.

---

## <a name="dependencies"></a> 4) Dependencies

### <a name="dotnet"></a> 4.1) .NET or compatible runtime

REQUIRED!

Version: **.NET v4.0 or higher**

*	Windows 10

	* Tested, just works. Windows 10 comes with .NET Framework 4.6 pre-installed.

*	Windows 8.1

	* Tested, just works. Windows 8.1 ships with .NET v4.0 pre-installed. Upgrading to newer version does not hurt (v4.5 and v4.6 run 4.0 apps and might be slightly faster).

* Older Windows versions:

	* You might already got .NET 4 or later through *Windows Update*.
	* If you are not sure, try to run the app anyway.
	* .NET 4 *Client Profile* is sufficient, does not contain some server related stuff, is smaller and faster to install compared to the *Full Framework*. However, according to Microsoft: "Starting with the .NET Framework 4.5, the Client Profile has been discontinued and only the full redistributable package is available. When you install the .NET Framework 4.5, the .NET Framework 4 Client Profile is updated to the full version of the .NET Framework."
	Full framework is probably needed if you want to compile the source code by yourself.

	* Windows 7 and Vista shipped with .NET 3.5 pre-installed. .NET 4 is available through Windows Update, however, upgrading Windows with service packs might be a pre-requisite.

	* Links

		* [Microsoft .NET homepage](http://www.microsoft.com/net)
		lists most recent .NET Framework Downloads.

		* [Microsoft .NET Framework 4.6 (Web Installer) ](https://www.microsoft.com/en-us/download/details.aspx?id=48130)

		* [Microsoft .NET Framework 4.6 (Offline Installer) ](https://www.microsoft.com/en-sa/download/details.aspx?id=48137)

				File Name: NDP46-KB3045557-x86-x64-AllOS-ENU.exe
				Date Published: 2015-07-10
				File Size: 62.4 MB

		* [Microsoft .NET Framework 4 Client Profile (Web Installer)](http://www.microsoft.com/en-us/download/details.aspx?id=17113)

		* [Microsoft .NET Framework 4 Client Profile (Standalone Installer)](https://www.microsoft.com/en-us/download/details.aspx?id=24872)

				File Name: dotNetFx40_Client_x86_x64.exe
				Date Published: 2011-02-21
				File Size: 41.0 MB

*	**Linux/Mac/Windows**: Mono

	free open source, multi-platform

	<http://www.mono-project.com/>

	Windows alternative to Microsoft's Framework: Mono + GTK# install, see next chapter.

	Linux packages:

	*	Debian: [mono-complete](http://packages.debian.org/search?suite=default&section=all&arch=any&searchon=names&keywords=mono-complete)
	*	Ubuntu: [mono-complete](http://packages.ubuntu.com/search?suite=default&section=all&arch=any&keywords=mono-complete&searchon=names)
	*	Arch Linux: [mono](http://www.archlinux.org/packages/?q=mono)


### <a name="gtksharp"></a> 4.2) Gtk\#

REQUIRED!

The application will not run without this as it is used to show its (multi-platform) *graphical user interface* (GUI).

**What is Gtk#?**

Excerpt from <http://www.mono-project.com/docs/gui/gtksharp/> :

*Gtk# is a Graphical User Interface Toolkit for mono and .Net. The project binds the [Gtk+](http://www.gtk.org/) toolkit and assorted [GNOME](http://www.gnome.org/) libraries, enabling fully native graphical Gnome application development using the Mono and .Net development frameworks.*

Basically it's a .NET wrapper for native [Gtk+](http://www.gtk.org/) binaries.

**Note:**
Some software like *MonoDevelop / Xamarin Studio* depends on *Gtk#*, too.
No need to install this if it is already there of course.


<http://www.mono-project.com/download/>

Select your **operating system** and follow instructions...

#### Windows:
<http://www.mono-project.com/download/#download-win>


Most users will want the following item:

*	**GTK# for .NET**
Installer for running Gtk#-based applications on Microsoft .NET:

	1.	Click Button: **Download Gtk#** (currently links to <http://download.xamarin.com/GTKforWindows/Windows/gtk-sharp-2.12.26.msi>, 25 MiB)
	2.	Run installer (`gtk-sharp-2.12.26.msi`)
	3.	If launching ScoobyRom does nothing (no app window appears), do a Windows logout and login (tested working for Windows 10 at least). If this does not help, try a reboot.

*	Alternative: **Mono + GTK#**

	**Mono for Windows** is not needed if you have a sufficient .NET runtime on your system already! If you need or want this combination:

	1.	Click Button: **Download Mono** (currently links to <http://download.mono-project.com/archive/4.0.3/windows-installer/mono-4.0.3-gtksharp-2.12.26-win32-0.msi>, 114 MiB)
	2.	Run installer (`mono-4.0.3-gtksharp-2.12.26-win32-0.msi`)

#### Linux:

package names:

*	Debian: [gtk-sharp2](https://packages.debian.org/search?suite=default&section=all&arch=any&searchon=names&keywords=gtk-sharp2)
*	Ubuntu: [gtk-sharp2](http://packages.ubuntu.com/search?suite=default&section=all&arch=any&keywords=gtk-sharp2&searchon=names)
*	Arch Linux: [gtk-sharp-2](https://www.archlinux.org/packages/?q=gtk-sharp-2)


### <a name="florence"></a> 4.3) Florence (fork of *NPlot*, .NET plotting library)

Required. Nothing to do as this library is already included pre-compiled (`/vendor/Florence.dll`).

Did some bug fixes and improvements only for the Gtk# widget code, included in ScoobyRom source (`GtkWidgets/PlotWidget.cs`).

Florence source code is pure C#, multi-platform. Currently, WinForms, Gtk#, and BitMap backends are implemented.

<https://github.com/scottstephens/Florence>

Older versions of Scoobyrom (v0.6.x) used *NPlot.dll* which does not support interactive zoom, pan etc.
The plot graphics look exactly the same.

##### *NPlot*
Development is rather inactive for years but pretty good and compact. Cannot do 3D (surface) plots, therefore gnuplot is used!

<http://netcontrols.org/nplot/wiki/>


### <a name="gnuplot"></a> 4.4) gnuplot

OPTIONAL.

"Gnuplot is a portable command-line driven graphing utility for Linux, OS/2, MS Windows, OSX, VMS, and many other platforms."

Homepage: <http://www.gnuplot.info/>

ScoobyRom uses gnuplot for **external** plotting (opening extra windows).

*	It's the only method to get 3D surface plots.
*	Interactive (zoom, scale, stretch... try mouse/keys, depends on used *gnuplot terminal*, see gnuplot documentation)
*	ScoobyRom action "Plot -> Create SVG File.." is also done through gnuplot, basically it's like a re-plot into file.
	Nowadays however, some gnuplot terminals (i.e. "qt", "wxt" - check menus/toolbar) also provide their own export features (SVG, PDF, bitmap image).

#### Installation

##### Version

ScoobyRom since v0.7.x is designed to work with gnuplot v5+. There are new/modified/obsolete commands compared to gnuplot v4 used by older ScoobyRom v0.6.x.

Try latest version first. In case of troubles tested version gnuplot v5.0.1 is recommended.

##### Windows

Step 1: Windows binaries:
<http://www.gnuplot.info/download.html>

currently redirects to:
<http://sourceforge.net/projects/gnuplot/files/gnuplot/>

Select either a win32 or win64 file depending on your operating system architecture. Installer `.exe` recommended for most users.

Tested example:

*	Windows 8.1 x64

	`gp501-win64-mingw.exe` (v5.0.1, 64 bit, 2015-07-14, 18 MB)

	Accepted default install path: `"C:\Program Files\gnuplot\"`

	`ScoobyRom.exe.config` entry (see below) -> `"C:\Program Files\gnuplot\bin\gnuplot.exe"`


Step 2: Run installer EXE (easiest method) or extract ZIP, respectively.

Note: The Windows installer can also set gnuplot's default terminal (`GNUTERM` environment variable). I recommend to select "**qt**" as "wxt" in 5.0.1 x64 on Windows has a window-resize bug. "qt" also seems to render faster.

Step 3: Check and edit file `ScoobyRom.exe.config` with a text editor.
Enter the exact full path of `gnuplot.exe`, it will probably exist in a gnuplot subfolder like "bin".
See chapter 5.1 for more details.

Example:

`<add key="gnuplot_Win32NT" value="C:\Program Files\gnuplot\bin\gnuplot.exe"/>`


##### Linux/Unix Systems

Your distribution repositories might offer a package called "gnuplot" - use your package manager to install it.
(If you want to play with gnuplot commands yourself I also recommend installing documentation package
like "gnuplot-doc" or similar in case doc is not already included).

You should be able to run command `gnuplot` (from a terminal). It may live in `/usr/bin/gnuplot` for example.
Edit `ScoobyRom.exe.config` if necessary, see chapter 5.1 for more details.


##### Other Platforms

Mac OS X etc. - Not tested yet! Please provide feedback!


#### Testing gnuplot

Launch the binary (Windows: `gnuplot.exe`, Linux/Unix: `gnuplot`)

(gnuplot command prompt `gnuplot>` should come up.) Now type:

	plot sin(x), cos(x)

(graph window showing function plots should appear)

	quit

(closes gnuplot)


#### Advanced Tips

*	On Windows gnuplot comes with a graphical user interface app, a special editor (`wgnuplot.exe`). An icon created by the installer version also points to this one.

*	gnuplot Terminals:

	Depending on the platform, gnuplot ships with multiple interactive *terminals*.
	Tested on Linux and Windows:

	*	qt
	*	wxt

	Try available ones to see which one works best.
	If not specified (**`GNUTERM`** environment variable not set, see gnuplot documentation), gnuplot will use the default one. Windows: Advanced System Settings - Environment Variables.

	Manually, temporary:

	Windows example: choose terminal "qt" and launch ScoobyRom:

		SET GNUTERM=qt
		ScoobyRom.exe


	Linux example, choose terminal "wxt" and launch ScoobyRom:

	`GNUTERM=wxt ScoobyRom.exe`

	You can also set terminal by editing gnuplot *template* files (`.plt`), insert command "`set term qt`" near the beginning, see also chapter 5.7.

---

## <a name="files"></a> 5) Files

General note regarding (XML) file line endings:

Applies to both *ScoobyRom.exe.config*, *ScoobyRom XML* and also *gnuplot template* files (`.plt`).

Newline type (CR/LF Windows, LF Unix) does not matter, as *ScoobyRom* and *gnuplot* can read both. Therefore you can easily share such files across operating systems.
When new files are written, such as by action *Save* (`Ctrl+S`), these will be in current native format.

[Wikipedia: Newline](http://en.wikipedia.org/wiki/Newline)


### <a name="config"></a> 5.1) ScoobyRom.exe.config

*Extensible Markup Language* ([XML](http://en.wikipedia.org/wiki/XML)) file, contains application settings, comes with descriptive comments for convenience.

Since there is no preferences dialog as part of the user interface yet, you will need to edit this file - use a text editor.
To be recognized it must live in the EXE-folder and have the exact prescribed name.
The app itself should work without .config file but it might be required for gnuplot features to work.
ScoobyRom does not modify this file and it is only being read at startup.
For changes to become effective, you need to restart ScoobyRom unfortunately.

Use any text (or even XML) editor to view/edit the file.
Recommendation for Windows: **Notepad++** (free OSS, lots of features) 	<http://notepad-plus-plus.org/>

#### gnuplot

##### Linux/Unix
Will probably just work by using default command "`gnuplot`":

	<add key="gnuplot_Unix" value="gnuplot"/>

You can test this by launching gnuplot manually via terminal.

##### Windows
Especially on Windows the appropriate path for gnuplot is important!
ScoobyRom does not try to find required **`gnuplot.exe`** on its own - very time consuming to search entire disks.

In case the **gnuplot installer** added gnuplot binary dir into system PATH, or you did this manually by yourself, just "`gnuplot.exe`" will work. Specifying full path does not hurt, though.
You can test this by launching gnuplot via *Command Prompt Window*.

###### Tested examples:

a)	Windows 8.1 x64 + gnuplot 5.0.1 x64 (`gp501-win64-mingw.exe`) using default install path (`C:\Program Files\gnuplot`)

--> full path of gnuplot.exe is `"C:\Program Files\gnuplot\bin\gnuplot.exe"`

Edit .config line to:

`<add key="gnuplot_Win32NT" value="C:\Program Files\gnuplot\bin\gnuplot.exe"/>`

b)	Windows 10 x64 + gnuplot 5.0.1 x64: workes exactly like a)

#### Icons on by default
Applies to icon column in both 2D and 3D table lists.

	<add key="iconsOnByDefault" value="True"/>

*	`"True"`: As icons are generated using a background thread this does not really matter anymore on fast computers.

	or

*	`"False"`: Icons are created on first demand. UI row heights are smaller without icons. Perhaps use this in case you rarely want icons visible.

#### Icon size
Errors or missing entries will result in default size (48 x 32) pixels. Both, width and height values are clamped, max 255. Minimum in UI for zooming out will be 24 x 16, larger icons are allowed via zoom in.

	<add key="iconWidth" value="48"/>
	<add key="iconHeight" value="32"/>


### <a name="scoobyrom_xml"></a> 5.2) ScoobyRom XML

#### 5.2.1) Description

Simple format to support app specific features, **not compatible with any other software** (*RomRaider, EcuFlash*)!

Basically it's meant for saving entered metadata (title, category, axis names, units, data type, ...).

Currently the application only writes items into file that have got some entered text.
For entirely new ROMs, where there is no accompanied XML found yet, it does create a bare minimum but valid file on action *"File -> Save"* (`Ctrl+S`).

Use any text (or even XML) editor to view/edit the file.
Recommendation for Windows: **Notepad++** (free OSS, lots of features) 	<http://notepad-plus-plus.org/>

The file is not required (like opening a new ROM with no ScoobyRom XML saved yet).
If it does exist, the app will try to read saved metadata and
combine (merge) it with data (values) found from searching through the ROM.

Notes:

*	Included XML comments are automatically generated by *ScoobyRom* as these can be useful when viewing the file. Also applies to file format created by feature "*Export As -> RomRaider def XML*".
*	Any comments will be ignored and lost whenever XML is being saved, just the newly written auto-generated ones will be there!
*	In XML payload text special characters such as "`<`" or "`&`" must be escaped. Either avoid these or use "`&lt;`" or "`&amp;`" respectively.

	Details: [Wikipedia: XML - Escaping](http://en.wikipedia.org/wiki/XML#Escaping)

#### 5.2.2) File Path
ScoobyRom assumes the metadata XML file to be found inside current ROM file directory!!!
Filename will/must be

	<ROM-filename-without-extension>.xml

ROM binary file extension (`.bin, .hex, .rom` etc.) does not matter!

Example:

`"my ROM file.bin"`

`"my ROM file.xml"` <-- loaded if it exists when opening binary, or will be created/overwritten using save command (`Ctrl+S`)


* Pros:
  No additional file dialog for XML to click through when opening a ROM file.
  No ambiguity, there's only a single valid XML path for a specific ROM file.
  Easy to find and backup this XML regularly (simple file copy, source control, ...).

* Cons:
  ROM folder must be writable.

WARNING: In case of existing file, ScoobyRom does not ask for permission (too annoying for every save), it immediately overwrites existing file! Be careful not to overwrite XML files meant for other software!!!

#### 5.2.3) Table Record Search Range
In short, this is to improve load performance and avoid false search results.

ROM search range is not required but improves search/load speed tremendously!
Unfortunately, the exact table position range is very **ROM specific**.

Without XML file or missing *tableSearch* XML element (see below), the app will search through the whole ROM file.
On a slow computer this can take several seconds.
By using a good search range, load time is usually just a fraction of a second - highly recommended if you use this app frequently in terms of editing metadata!

See record position column as well as statistics window to get first/last record positions.
Currently you've got to add and adjust this manually in XML:


It's not necessary to copy & paste exact numbers like

	<tableSearch start="0x8BE98" end="0x936D8" />

it will provide maximum load speed, though.

Somewhat larger range will be almost as fast, in addition it might work for many similar ROMs:

	<tableSearch start="0x80000" end="0x95000" />


##### Notes:

*	Searching through unsuitable range (too large or whole file) might introduce false results (random data that looks like valid table data from detection algorithm's perspective)!

*	If you want to annotate a new ROM, I recommend specifying metadata and a suitable search range sooner than later, you'll get proper ROM ID text (*internalidstring*, see next chapter) displayed in main window, better load speed and avoid looking at false maps.

*	Records for 2D tables as well as 3D tables are often grouped together in the ROM. Looking at column *Record* (click column header to sort), you should see those table record addresses being close together. If record addresses differ a lot from most others, these are probably false results, easy to be excluded using *tableSearch*.

*	In many Denso ROMs, all 2D table records are stored together, then followed by all 3D ones. Therefore creating search range by taking addresses of first (valid) 2D table and last (valid) 3D table works very well.

*	Another indication for bad tables is that their data, therefore also min, max, average are very high/low numbers (e.g. -1.567E+11). Sometimes even *NaN* (Not a Number).

#### 5.2.4) ROM ID Metadata
Borrowed from *RomRaider* ECU definition format, needed for *"Export as -> RomRaider def XML"* anyway.

There is no UI dialog yet, you have to edit XML manually!

In case you've got similar ROMs I suggest copy & paste the whole romid region (`<romid>...</romid>`), then edit required changes.

ScoobyRom itself currently does only use these sub-elements

*	internalidaddress
*	internalidstring

to verify the string from ROM and display it in main window title.
All others are just being read and re-written.

#### 5.2.5) Full example content
The following ScoobyRom XML example file contains just two annotated tables for brevity:

	<?xml version="1.0" encoding="utf-8"?>
	<rom>
	  <romid>
		<xmlid>JZ2F401A</xmlid>
		<internalidaddress>400C</internalidaddress>
		<internalidstring>JZ2F401A</internalidstring>
		<ecuid>6644D87207</ecuid>
		<year>2009_2010</year>
		<market>EDM</market>
		<make>Subaru</make>
		<model>Impreza</model>
		<submodel>2.0 Turbo Diesel 150 HP</submodel>
		<transmission>6MT</transmission>
		<memmodel>SH7058</memmodel>
		<flashmethod>subarucan</flashmethod>
		<filesize>1MB</filesize>
	  </romid>
	  <tableSearch start="0x89500" end="0x93700" />
	  <table2D category="Sensors" name="Coolant Temperature" storageaddress="0x8BE98">
		<!-- 0.248 to 4.836 -->
		<axisX storageaddress="0xB36D8" name="Sensor Voltage" unit="V" />
		<!-- -40 to 135 -->
		<values storageaddress="0xB374C" unit="°C" storagetype="UInt8" />
		<description>NTC thermistor</description>
	  </table2D>
	  <table3D category="Fueling" name="Injection Quantity Limit (Gear)" storageaddress="0x90518">
		<!-- 0 to 5400 -->
		<axisX storageaddress="0xCD144" name="Engine Speed" unit="1/min" />
		<!-- 0 to 6 -->
		<axisY storageaddress="0xCD1A0" name="Gear" unit="1" />
		<!-- -30 to 100 -->
		<values storageaddress="0xCD1BC" unit="mm³/st" storagetype="UInt8" />
		<description />
	  </table3D>
	</rom>

### <a name="romraider_export"></a> 5.3) RomRaider ECU def export

You can use *RomRaider* to edit map values and modify the ROM.

Homepage: <http://www.romraider.com/>

ScoobyRom will display statistics and ask whether you want to export all, selected only or annotated only tables.

Empty categories will be named "Unknown 2D" and "Unknown 3D".

Tables having empty names will appear named "Record 0x..." in RomRaider, indicating table (record) location which can be entered/searched/sorted in ScoobyRom's "Record" column.

Note: For loading many hundreds of tables into RomRaider, you might need to increase allowed memory usage for RomRaider. See RomRaider documentation/forums on how to do this.

Do not use very old versions of RomRaider: For signed data types (int16, int8) support,
RomRaider v0.5.3b RC7 or newer is needed!
<http://www.romraider.com/forum/viewtopic.php?f=14&t=6801>


### <a name="tunerpro_export"></a> 5.4) TunerPro XDF export

*TunerPro* is a fully featured generic ROM editor. Loading big definition files is very fast compared to *RomRaider*. Unlike RomRaider, TunerPro supports creating definitions from scratch via user interface, no text/XML editor needed. This also applies to editing imported data of course.

Some disadvantages compared to RR: Windows only, closed-source

Homepage: <http://tunerpro.net/>

ScoobyRom will display statistics and ask whether you want to export all, selected only or annotated only tables.

Empty categories will be named "Unknown 2D" and "Unknown 3D".

Tables having empty names will appear named "Record 0x..." in TunerPro, indicating table (record) location which can be entered/searched/sorted in ScoobyRom's "Record" column.

### <a name="svg_export"></a> 5.5) SVG export

<http://en.wikipedia.org/wiki/Scalable_Vector_Graphics>

Requires working gnuplot setup!
Basically the app instructs gnuplot to refresh the current plot (window) into a SVG file.

Regarding 3D: SVG is a pure 2D format, you cannot adjust the viewpoint anymore once created!
Rotate/adjust the plot view until satisfied, then do the SVG export.
Of course you can create as many exports as you like.

An SVG showing a 2D-plot takes around 20 KiB file size, rather fast to view.
A 3D plot SVG containing hundreds of polygons can take 150 KiB for example.
Displaying a complicated file can take seconds depending on viewer program and computer speed.

SVGs should be viewable in all modern browsers like

*	Mozilla Firefox <http://www.mozilla.org/products/firefox/>
*	Chrome <http://www.google.com/chrome/>
*	Opera <http://www.opera.com/>
*	Internet Explorer <http://ie.microsoft.com/>
*	... various graphic viewers and vector graphics editors.

Recommended full featured editor, uses SVG format natively:
*Inkscape* (free OSS, multi-platform): <http://inkscape.org/>


### <a name="other_export"></a> 5.6) Other export formats

Export to comma-separated values table is supported.

gnuplot also supports many other export formats like PDF, EPS, PNG and so on.
Would be easy to add support in the same way as SVG.
Nowadays some gnuplot terminals (i.e. "qt", "wxt" - check menus/toolbar) also provide their own export features (SVG, PDF, bitmap image).

### <a name="temp_files"></a> 5.7) Temporary files

Only a single one: `gnuplot_data.tmp`

created in operating system specific temporary folder.

Examples:

*	Windows 8.1: `C:\Users\%USER%\AppData\Local\Temp\`
*	Linux: `/tmp/`

This binary data file is being used for all gnuplot plots - overwritten on each new plot action. Therefore no need for cleanup as its size is only a few KB anyway.

Although gnuplot supports parsing text data via standard input - no need for temp file, I prefer binary transfer where full accuracy is guaranteed (32 bit floating point), similar like a 32 bit car control unit microcontroller would see the maps. No need to generate text and parse in gnuplot, no worries about decimal places etc.

### <a name="gnuplot_template_files"></a> 5.8) gnuplot template files

These two text files

*	`gnuplot_Table2D.plt`

*	`gnuplot_Table3D.plt`

must be found in either

1.	current (where app has been launched from) directory

	or

2.	app install (EXE) directory.

Otherwise it will report "File not found" error and plot window will not be created.
Current folder has precedence, therefore you might use this to play with a local version of these template files for testing. Rename or delete local corresponding `.plt` file and it will try to use `.plt` from app dir.

Templates contain most *gnuplot* commands being executed on every plot action.
The latter file (3D) has lots of comments in it - you might be able to learn some gnuplot commands from it.
Feel free to modify those - change colours, layout, default view etc. Simply re-plot to review changes.

A few (text labeling) commands are generated within ScoobyRom on the fly, you need to change source code for this.

---

## <a name="launch"></a> 6) Launch and Command Line Parameters

ScoobyRom is designed to only load a **single ROM**!
However you can start the app multiple times, load any ROM in each instance,
put app windows on different monitors or switch windows and so on.

One optional argument is supported:

`ScoobyRom.exe <ROM_file>`

Recommended: Put file/path in quotes when there are spaces or other characters in it.

On *Windows* for example, type within a command prompt window:
`ScoobyRom.exe "C:\Data\ROM\My ROM.bin"`

Using *Mono* runtime on *Unix/Linux* systems for example:
`mono ScoobyRom.exe "~/ROMs/My ROM.rom"`

If `<ROM_file>` cannot be found, ScoobyRom ignores this error, starts in normal (empty) mode, ready to open a ROM via UI command (`Ctrl+O`).

---

## <a name="ui"></a> 7) User Interface Hints

Couple of things that may not be immediately obvious:

*	Both tab pages have a horizontal splitter between list view (top) and visualisation area (bottom).
Click & drag splitter according to your needs. However, the app does not remember such UI settings.

*	Using check mark columns has no effect at all, selection is not being saved/restored.
You can use it to temporarily select and sort items if you want to. Might have more effect in future versions.

*	You can sort by any column (except icons), click on column header to toggle between ascending/descending sort.

*	Quickly jump to an annotated table by entering text beginning. This is case-insensitive. You might need to click into list view column content to focus the desired column first as it only searches a single column. Double-click instead is for entering/modifying text.

	For example, if *Title* "Engine Speed" exists on a table, focus title column if not done already, type "`eng`" and it will probably jump to the first matching row.

	Hex columns *Record, X/Y/ZPos*: you need to type in complete address in hex, i.e. "`8ddc4`" or "`0x8ddc4`" or "`$8ddc4`". Supported hexadecimal prefixes are "`0x`" and "`$`".

*	Visualisation (`Ctrl+Space`): Either displays coloured table values (3D) or 2D values + line graph in bottom tab pane.
Any visualisation (except icons) does not update on changed metadata (text, data type),
you'll have to trigger updates manually (`Ctrl+Space`) and/or (`Ctrl+P`) for gnuplot window.
Double-clicking or (`Enter`) key on a focused read-only row column (icon, numbers)
also triggers internal visualisation.

*	Icons update immediately on table type change as this is a fast operation.
By looking a the icon you can often tell already whether current data type is correct or not ("stripe patterns", bad values).

*	Gray icons ("const") are displayed for tables containing only same values (min = max).
Often these are valid tables and are actually used in ROM firmware logic.

*	gnuplot windows:
	*	(`Ctrl+P`) creates a gnuplot window or destroys it if it already existed.

		There's currently a bug when closing gnuplot windows directly (i.e. `Alt+F4`) - without ScoobyRom. Then ScoobyRom does not get noticed and in order to re-plot the map you need to do (`Ctrl+P`) twice.

	*	Quitting ScoobyRom also closes all gnuplot windows.

	*	gnuplot 3D view: (`Home` / `Pos1`) key to reset viewing angle (implemented in template)

*	Integrated 2D line plot has interactive features:
	*	zoom in/out via mouse wheel or keys (`+`) / (`-`)
	*	pan (click and drag in plot area)
	*	stretch axis (click and drag in axis area)
	*	(`Home` / `Pos1`) key to reset plot (auto-scale both axes)
	*	(`Alt`) in addition to other keys for finer operation

*	*"Edit -> Copy Table"* (`Alt+C`)

	*	Copies table values as text into clipboard.
	*	The format is *RomRaider*-compatible but also works well for pasting into spreadsheet applications (*LibreOffice Calc, Microsoft Excel*). You can easily verify the output by pasting into a text editor.
	*	Values only, no annotations (text)
	*	The number format (decimal separator) depends on current operating system region settings. (Not sure yet if *RomRaider* also uses current locale or always needs English formatting for copy/paste.)

*	Navigation Bar

	*	Visualises ROM content using different colours: table records, axes and values.
	*	Slim horizontal bars (dark yellow) on top of the content-rectangle indicate **checksummed regions**.
	*	Currently **zoom** in/out/reset works by placing mouse pointer somewhere over NavBar area, holding (left) mouse button #1 depressed and pressing key (`+`), (`-`) or (`0`), respectively.
	*	Tooltip shows ROM position (hex, byte size, decimal) as well as content type at mouse pointer. You should use this to find out the mapping between colour and content.
	*	Since ScoobyRom deals with tables mostly, unlike *[IDA](http://www.hex-rays.com/products/ida/)* for example, it does not scan for other content types like code, empty space, misc data etc.
	*	Vertical markers (red) show currently viewed table record and corresponding locations of axes and values. These markers only update using visualise-action.
	*	Special markers (left + right) and coloured region (grey) appear when *tableSearch* is specified. Makes it easy to verify or improve table search range.
