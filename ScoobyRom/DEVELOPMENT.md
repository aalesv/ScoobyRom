# ScoobyRom

![](Images/AppIcon.png)

Author: <http://subdiesel.wordpress.com/>

Project homepage on *GitHub*: <http://github.com/SubaruDieselCrew/ScoobyRom/>

# Development

## CONTENTS

1. Integrated Development Environment (IDE)
2. Compiling from source without IDE
3. Miscellaneous

---

## 1) Integrated Development Environment (IDE)

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

## 2) Compiling from source without IDE

### 2.1) Mono (Linux)

Mono provides `xbuild` command line tool.
Besides main runtime package (often named `mono`, `mono-runtime` or `mono-core`), on some Linux distributions required additional packages might be called *mono-devel* ([Ubuntu](http://packages.ubuntu.com/search?keywords=mono-devel), [Debian](https://packages.debian.org/search?keywords=mono-devel&searchon=names)) or *mono-dev*.

`xbuild /property:Configuration=Release ScoobyRom.sln`

or

`xbuild /property:Configuration=Debug ScoobyRom.sln`

or just `xbuild` (defaults to debug build, picks solution/project file in current directory)


### 2.2) Windows

Like on Linux, just replace `xbuild` with `msbuild`.
You might need to find and specify exact path of `msbuild.exe`.

If not in path, consider adding tools directory to environment variable `%PATH%`, either temporarily (see below) or in Windows system settings.

Windows 8.1 x64 tested example:

	SET PATH=%PATH%;C:\Windows\Microsoft.NET\Framework\v4.0.30319\
	
	msbuild /property:Configuration=Release ScoobyRom.sln

---

## 3) Miscellaneous

Created on Linux using free open source software!

#### .NET version

Currently the code needs a .NET 4.0 compatible runtime (Windows .NET, Mono etc.).
Back-porting code and solution/project files to VS 2008 and .NET 3.5 for example is certainly possible.
Currently there's little code making use of .NET 4.0 features (`Task` is one).
Minimum .NET version might step up in the future as using newer C# language features makes sense and most users upgrade .NET anyway.

#### Code Formatting
MonoDevelop feature *Format Document* has been used, not consistently, as it ain't perfect.

#### Code Design
The main author still considers this software experimental!
It wasn't clear what features would work upfront, with as little effort as possible.
Some classes have been optimized and refactored over time, others are not so well designed.
