# ScoobyRom

![](Images/AppIcon.png)

Author: <http://subdiesel.wordpress.com/>

Project homepage on *GitHub*: <https://github.com/aalesv/ScoobyRom/>

Project homepage on *GitHub*, deprecated: <http://github.com/SubaruDieselCrew/ScoobyRom/>

# Development

## CONTENTS

1. Dependencies
2. Integrated Development Environment (IDE)
3. Compiling from source without IDE

---

## 1) Dependencies

GTK ver. 3 x64 runtime must be installed. Gtk# could be installed from NuGet repository, package name `GtkSharp`. Please note that currently **all** Gtk# versions 3.x have a bug that crashes application. More detailed information could be found [here](https://github.com/GtkSharp/GtkSharp/issues/248). Patch is available [here](https://github.com/zii-dmg/GtkSharp/commit/8b7240b4d80e94e2a2d312ca39915f45bad55eac) or [here](https://github.com/aalesv/GtkSharp/tree/develop-toggleref). You should build Gtk# packages yourself or get prebuilt packages [here](https://github.com/aalesv/GtkSharp/releases) and put them into local Nuget repository (out of scope of this document).

## 2) Integrated Development Environment (IDE)

*	*MonoDevelop* (Linux) / *Xamarin Studio* (Windows)
<http://monodevelop.com/>

	This one is ideal because it has a good *Gtk#* graphical user interface designer - saves time.
	Multi-platform, tested on Linux and Windows.
	It is **open source**, written in C#, uses *Mono*'s *xbuild* for compilation.
	So far, *ScoobyRom* has been written using *MonoDevelop* on Linux almost exclusively.

*	*Visual Studio*
	<http://www.visualstudio.com/>

	Free *Community* edition is more than capable, old *Express* versions also used to work.

	Obviously Windows-only, there is no *Gtk#* designer but it can compile own and already designer-generated *Gtk#* code.
	VS uses `msbuild` under the hood.

	Tested working: Visual Studio 2015 Community (Windows 8.1 x64)

*	Others: not tested yet

---

## 3) Compiling from source without IDE
To build single executable run:

	dotnet publish -p:PublishSingleFile=true --no-self-contained

#### .NET version

Currently the code needs a .NET 8.0 runtime.

#### Code Formatting
MonoDevelop feature *Format Document* has been used, not consistently, as it ain't perfect.

#### Code Design
The main author still considers this software experimental!
It wasn't clear what features would work upfront, with as little effort as possible.
Some classes have been optimized and refactored over time, others are not so well designed.
