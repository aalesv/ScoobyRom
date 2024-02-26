// GnuPlot.cs: Launch and control gnuplot processes (external plot windows).

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


using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tables.Denso;
using System;

namespace ScoobyRom
{
	public sealed class GnuPlot
	{
		// Temporary file to transfer binary plot data
		// Sharing a single temporary file is sufficient, written on each new plot.
		const string BinaryFilename = "gnuplot_data.tmp";

		// TODO could put template file names into app.config
		const string TemplateFile2D = "gnuplot_Table2D.plt";
		const string TemplateFile3D = "gnuplot_Table3D.plt";


		#region static

		static string BinaryFile;
		static readonly string exePath = Config.GnuplotPath;
		static readonly Dictionary<Table, GnuPlot> gnupDict = new Dictionary<Table, GnuPlot> ();

		static GnuPlot ()
		{
			BinaryFile = System.IO.Path.Combine (System.IO.Path.GetTempPath (), BinaryFilename);
			// Windows 8.1 x64 -> "C:\Users\<UserName>\AppData\Local\Temp\gnuplot_data.tmp"
		}

		/// <summary>
		/// Create or close gnuplot for given table.
		/// </summary>
		/// <param name="table">
		/// A <see cref="Table"/>
		/// </param>
		public static void ToggleGnuPlot (Table table)
		{
			GnuPlot gnuPlot = GetExistingGnuPlot (table);
			if (gnuPlot == null) {
				gnuPlot = new GnuPlot (table);
				gnupDict.Add (table, gnuPlot);
			} else {
				gnuPlot.Quit ();
			}
		}

		/// <summary>
		/// Gets existing GnuPlot object if there is one.
		/// </summary>
		/// <param name="table">
		/// The <see cref="Table"/> object.
		/// </param>
		/// <returns>
		/// The <see cref="GnuPlot"/> reference or null.
		/// </returns>
		public static GnuPlot GetExistingGnuPlot (Table table)
		{
			GnuPlot gnuPlot;
			gnupDict.TryGetValue (table, out gnuPlot);
			return gnuPlot;
		}

		/// <summary>
		/// Plots into SVG file.
		/// As this merely redraws into export file, the current view from the plot window will be used.
		/// </summary>
		/// <param name="table">
		/// A <see cref="Table"/>
		/// </param>
		public static void CreateSVG (Table table, string svgPath)
		{
			GnuPlot gnuPlot = GetExistingGnuPlot (table);
			if (gnuPlot == null)
				throw new GnuPlotException ("Need existing gnuplot window.");

			// Test creating empty file in order to get possible exception.
			// Otherwise no easy way to get feedback from gnuplot error.
			// Sort of Unix command 'touch'.
			File.WriteAllText (svgPath, string.Empty);

			CreateSVG (gnuPlot.stdInputStream, svgPath);
		}

		static void Remove (GnuPlot gnuplot)
		{
			Table table = null;
			foreach (var kvp in gnupDict) {
				if (kvp.Value == gnuplot) {
					table = kvp.Key;
					break;
				}
			}
			if (table != null)
				gnupDict.Remove (table);
		}

		#endregion static

		#region fields

		// keep Process reference for keeping it and input stream alive
		Process process;
		// sending gnuplot commands via standard input stream
		StreamWriter stdInputStream;

		#endregion fields

		// use static factory methods!
		private GnuPlot (Table table)
		{
			try {
				using (FileStream fs = new FileStream (BinaryFile, FileMode.Create, FileAccess.Write))
				using (BinaryWriter bw = new BinaryWriter (fs)) {
					Table3D t3D = table as Table3D;
					if (t3D != null)
						WriteGnuPlotBinary (bw, t3D);
					else
						WriteGnuPlotBinary (bw, (Table2D)table);
				}
			} catch (Exception ex) {
				throw new GnuPlotException ("Could not create temporary binary data file for GnuPlot:\n" + BinaryFile + "\n\n" + ex.Message);
			}

			try {
				StartProcess (table);
			} catch (System.ComponentModel.Win32Exception ex) {
				// from MSDN
				// These are the Win32 error code for file not found or access denied.
				const int ERROR_FILE_NOT_FOUND = 2;
				const int ERROR_ACCESS_DENIED = 5;

				switch (ex.NativeErrorCode) {
				case ERROR_FILE_NOT_FOUND:
					throw new GnuPlotProcessException ("Could not find gnuplot executable path:\n" + exePath + "\n\n" + ex.Message);
				case ERROR_ACCESS_DENIED:
					throw new GnuPlotProcessException ("Access denied, no permission to start gnuplot process!\n" + ex.Message);
				default:
					throw new GnuPlotProcessException ("Unknown error. Could not start gnuplot process.\n" + ex.Message);
				}
			}
		}

		void Quit ()
		{
			stdInputStream.WriteLine ("quit");
			stdInputStream.Dispose ();
			stdInputStream = null;
			process.WaitForExit (1000);
			process.Dispose ();
			process = null;
			Remove (this);
		}

		void StartProcess (Table table)
		{
			if (string.IsNullOrEmpty (exePath))
				throw new GnuPlotProcessException ("gnuplot executable path is empty!");

			process = new Process ();
			process.StartInfo.FileName = exePath;
			process.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;

			// StartInfo.UseShellExecute must be false in order to use stream redirection
			process.StartInfo.UseShellExecute = false;
			// Could write temp script file but using standard input is more elegant and allows more functionality..
			process.StartInfo.RedirectStandardInput = true;

			// --persist without stdinput method didn't work on Windows - gnuplot window closes immediatly!?
			// --persist + using stdinput prints gnuplot commands into terminal text
			// process.StartInfo.Arguments = "--persist";


			//process.StartInfo.StandardInputEncoding = System.Text.Encoding.UTF8;

			// hide additional cmd window on Windows, useful for debugging (printing gnuplot vars etc.)
			process.StartInfo.CreateNoWindow = true;

			process.EnableRaisingEvents = true;
			process.Exited += HandleProcessExited;

			process.Start ();

			stdInputStream = process.StandardInput;

			ScriptGnuplotCommon (stdInputStream, table);

			Table3D t = table as Table3D;
			if (t != null)
				ScriptGnuplot3D (stdInputStream, t);
			else
				ScriptGnuplot2D (stdInputStream, (Table2D)table);

			stdInputStream.Flush ();
		}

		void HandleProcessExited (object sender, System.EventArgs e)
		{
			// e.g. gnuplot process got killed
			Remove (this);
		}


		#region private static helper methods

		static void ScriptGnuplotCommon (StreamWriter sw, Table table)
		{
			// easy test:
			// sw.WriteLine ("plot sin(x), cos(x)"); return;

			sw.WriteLine ("set encoding utf8");
			WriteLine (sw, string.Format ("windowtitle=\"{0}\"", table.Title));
		}

		/// <summary>
		/// Try to find filename in current dir or application dir.
		/// </summary>
		/// <returns>The file path.</returns>
		/// <param name="filename">Filename.</param>
		static string FindFileInCurrentOrAppFolder (string filename)
		{
			string p = filename;
			if (File.Exists (p)) {
				return p;
			} else {
				p = Path.Combine (MainClass.AssemblyFolder, filename);
				if (File.Exists (p)) {
					return p;
				} else {
					throw new FileNotFoundException ("Cannot find required file in current dir or application dir!", filename);
				}
			}
		}

		static void ScriptGnuplot3D (StreamWriter sw, Table3D table3D)
		{
			WriteLine (sw, SetLabel ("xlabel", table3D.NameX, true, table3D.UnitX));
			WriteLine (sw, SetLabel ("ylabel", table3D.NameY, true, table3D.UnitY));
			// use title instead of zlabel as it would need extra space
			WriteLine (sw, SetLabel ("title", table3D.Title, false, table3D.UnitZ));

			WriteLine (sw, "set label 1 \"" + AnnotationStr (table3D) + "\" at screen 0.01,0.95 front left textcolor rgb \"blue\"");
			//set label 1 "Annotation Label" at screen 0.01,0.95 front left textcolor rgb "blue"

			// call gnuplot script, also pass argument to it (path to temporary binary data file)
			// Windows: do not use double quotes - does not recognize paths containing "\" then

			sw.WriteLine ("call '{0}' '{1}'", FindFileInCurrentOrAppFolder (TemplateFile3D), BinaryFile);
		}

		static void ScriptGnuplot2D (StreamWriter sw, Table2D table2D)
		{
			WriteLine (sw, SetLabel ("xlabel", table2D.NameX, false, table2D.UnitX));
			WriteLine (sw, SetLabel ("ylabel", table2D.Title, false, table2D.UnitY));
			WriteLine (sw, SetLabel ("title", table2D.Title, false, table2D.UnitY));

			// Min/Max/Avg label might obscure title etc.
			//sw.WriteLine ("set label 1 \"" + AnnotationStr (table2D) + "\" at screen 0.01,0.96 front left textcolor rgb \"blue\"");

			// Windows: use single instead of double quotes because would interpret backslashes in path
			sw.WriteLine ("call '{0}' '{1}'", FindFileInCurrentOrAppFolder (TemplateFile2D), BinaryFile);
		}

		// needed to get special characters displayed correctly on Windows, not needed on Linux
		// workaround as there is no process.StartInfo.StandardOutputEncoding, only process.StartInfo.StandardInputEncoding
		static void WriteLine (StreamWriter sw, string s)
		{
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes (s);
			sw.BaseStream.Write (bytes, 0, bytes.Length);
			sw.WriteLine ();
		}

		static string SetLabel (string item, string name, bool newLineBeforeUnit, string unit)
		{
			// requires "termoption enhanced" = enhanced text capabilities
			// example:
			// set xlabel "Engine Speed\n{/mono [1/min]}"
			StringBuilder sb = new StringBuilder (128);
			sb.Append ("set ");
			sb.Append (item);
			sb.Append (" \"");
			sb.Append (name);

			if (!string.IsNullOrEmpty (unit)) {
				sb.Append (newLineBeforeUnit ? @"\n" : "  ");
				sb.Append ("{/mono [");
				sb.Append (unit);
				sb.Append ("]}\"");
			}
			return sb.ToString ();
		}

		static string AnnotationStr (Table3D table3D)
		{
			return string.Format ("Min: {0}\\nMax: {1}\\nAvg: {2}", table3D.Zmin.ToString (), table3D.Zmax.ToString (), table3D.Zavg.ToString ());
		}

		//		static string AnnotationStr (Table2D table2D)
		//		{
		//			return string.Format ("Min: {0} Max: {1} Avg: {2}", table2D.Ymin.ToString (), table2D.Ymax.ToString (), table2D.Yavg.ToString ());
		//		}

		// TODO investigate piping binary data via standard input, avoiding temp file
		// However temp file might be useful for manual gnuplot experiments.
		static void WriteGnuPlotBinary (BinaryWriter bw, Table3D table3D)
		{
			/* from gnuplot help PDF page 157, "matrix binary":

					Single precision ﬂoats are stored in a binary ﬁle as follows:
					<N+1> <y0>   <y1>   <y2>  ... <yN>
					<x0> <z0,0> <z0,1> <z0,2> ... <z0,N>
					<x1> <z1,0> <z1,1> <z1,2> ... <z1,N>
					*/
			// x <-> y designation from above (manual) seems wrong as xlabel etc. matches code below!

			float[] valuesX = table3D.ValuesX;
			// write first float: <N+1>
			bw.Write ((float)(valuesX.Length));

			// x axis
			foreach (var x in valuesX) {
				bw.Write (x);
			}

			float[] valuesY = table3D.ValuesY;
			float[] valuesZ = table3D.GetValuesZasFloats ();
			for (int iy = 0; iy < valuesY.Length; iy++) {
				bw.Write (valuesY [iy]);
				for (int ix = 0; ix < valuesX.Length; ix++) {
					bw.Write (valuesZ [ix + iy * valuesX.Length]);
				}
			}
		}

		static void WriteGnuPlotBinary (BinaryWriter bw, Table2D table2D)
		{
			float[] valuesX = table2D.ValuesX;
			bw.Write ((float)(valuesX.Length));
			foreach (var x in valuesX) {
				bw.Write (x);
			}
			// same format as 3D but only write a single row
			float[] valuesY = table2D.GetValuesYasFloats ();
			bw.Write (0f);
			for (int ix = 0; ix < valuesX.Length; ix++) {
				bw.Write (valuesY [ix]);
			}
		}

		static void CreateSVG (StreamWriter gnuplotInput, string svgPath)
		{
			gnuplotInput.WriteLine ("set terminal push");
			// need apropriate size for font, point size etc.
			// TODO UI dialog to enter/adjust such output properties
			gnuplotInput.WriteLine ("set terminal svg size 1200,800 dynamic enhanced font \"sans,16\"");
			// gnuplot does not like double quotes and Windows backslashes -> wrong paths!
			gnuplotInput.WriteLine ("set output '{0}'", svgPath);
			// "refresh" instead of "replot"
			// 	don't want to reset viewing angle (3D), use current view from window
			gnuplotInput.WriteLine ("refresh");
			gnuplotInput.WriteLine ("set terminal pop");

			// TODO is this necessary to flush & close written file ???
			gnuplotInput.WriteLine ("refresh");

			// Note: could use "Rsvg" - a C# wrapper for displaying SVG (MonoDoc: Gnome libraries)
			// or launch an app (web browser) to check written file...
		}

		#endregion private static helper methods
	}
}
