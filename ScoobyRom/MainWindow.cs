// MainWindow.cs: Main application window user interface.

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


// can be useful for testing
//#define LOAD_SYNC

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gtk;
using ScoobyRom;

public partial class MainWindow : Gtk.Window
{
	enum ActiveUI
	{
		Undefined,
		View2D,
		View3D
	}

	// 0 = first page = View2D; 1 = second = View3D
	const int DefaultNotebookPageShown = 1;

	readonly Data data = new Data ();

	readonly DataView3DModelGtk dataView3DModelGtk;
	readonly DataView3DGtk dataView3DGtk;

	readonly DataView2DModelGtk dataView2DModelGtk;
	readonly DataView2DGtk dataView2DGtk;

	// Gtk# integration: NPlot.Gtk.PlotSurface2D instead of generic NPlot.PlotSurface2D
	//readonly NPlot.Gtk.NPlotSurface2D plotSurface = new NPlot.Gtk.NPlotSurface2D ();
	readonly Florence.InteractivePlotSurface2D plotSurface = new Florence.InteractivePlotSurface2D ();


	readonly Plot2D plot2D;

	// const so far, so share it
	static readonly Util.Coloring coloring = new Util.Coloring ();

	// measuring load/search performance
	readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch ();

	// Remember output folder for convenience. No need to save it in app.config?
	string svgDirectory = null;

	public MainWindow (string[] args) : base (Gtk.WindowType.Toplevel)
	{
		// Execute Gtk# visual designer generated code (MonoDevelop http://monodevelop.com/ )
		// Obviously, Visual Studio doesn't have a Gtk# designer, you'll have to code all UI stuff by yourself.
		// Compiling existing generated UI code within Visual Studio does work however.
		Build ();

		this.Icon = MainClass.AppIcon;

		int iconWidth = Config.IconWidth;
		int iconHeight = Config.IconHeight;

		dataView3DModelGtk = new DataView3DModelGtk (this.data, iconWidth, iconHeight);
		dataView3DGtk = new DataView3DGtk (dataView3DModelGtk, treeview3D);
		dataView3DGtk.Activated += delegate(object sender, ActionEventArgs e) {
			var table3D = (Tables.Denso.Table3D)e.Tag;
			if (table3D != null) {
				this.Show3D (table3D);
			}
		};

		dataView2DModelGtk = new DataView2DModelGtk (this.data, iconWidth, iconHeight);
		dataView2DGtk = new DataView2DGtk (dataView2DModelGtk, treeview2D);
		dataView2DGtk.Activated += delegate(object sender, ActionEventArgs e) {
			var table2D = (Tables.Denso.Table2D)e.Tag;
			if (table2D != null) {
				this.Show2D (table2D);
			}
		};

		plot2D = new Plot2D (plotSurface);

		var plotWidget = new Florence.GtkSharp.PlotWidget ();
		plotWidget.InteractivePlotSurface2D = plotSurface;
		// default: true, otherwise causes flickering
		//plotWidget.DoubleBuffered = false;

		this.hpaned2D.Add2 (plotWidget);
		//global::Gtk.Paned.PanedChild pc = ((global::Gtk.Paned.PanedChild)(this.hpaned2D [plotWidget]));
		// to resize both panes proportionally when parent (main window) resizes
		//pc.Resize = false;
		//pc.Shrink = false;
		this.hpaned2D.ShowAll ();

		this.notebook1.Page = DefaultNotebookPageShown;
		OnNotebook1SwitchPage (null, null);

		// program arguments: first argument is ROM path to auto-load
		if (args != null && args.Length > 0 && !string.IsNullOrEmpty (args [0])) {
			OpenRom (args [0]);
		}
	}

	ActiveUI CurrentUI {
		get {
			if (notebook1.CurrentPageWidget == vpaned2D)
				return ActiveUI.View2D;
			else if (notebook1.CurrentPageWidget == vpaned3D)
				return ActiveUI.View3D;
			else
				return ActiveUI.Undefined;
		}
	}

	DataViewModelBaseGtk CurrentViewModel {
		get {
			switch (CurrentUI) {
			case ActiveUI.View2D:
				return dataView2DModelGtk;
			case ActiveUI.View3D:
				return dataView3DModelGtk;
			}
			return null;
		}
	}

	DataViewBaseGtk CurrentView {
		get {
			switch (CurrentUI) {
			case ActiveUI.View2D:
				return dataView2DGtk;
			case ActiveUI.View3D:
				return dataView3DGtk;
			}
			return null;
		}
	}

	Tables.Denso.Table CurrentTable {
		get {
			var view = CurrentView;
			if (view == null)
				return null;
			var table = view.Selected;
			if (table == null) {
				ErrorMsg ("Error", "No table selected.");
			}
			return table;
		}
	}

	void OpenRom (string path)
	{
		this.progressbar1.Adjustment.Lower = 0;
		this.progressbar1.Adjustment.Upper = 100;
		//this.progressbar1.Adjustment.StepIncrement = 5;
		this.progressbar1.Adjustment.Value = 0;

		data.ProgressChanged += delegate(object s, System.ComponentModel.ProgressChangedEventArgs pArgs) {
			Application.Invoke (delegate {
				this.progressbar1.Adjustment.Value = pArgs.ProgressPercentage;
			});
		};

		this.statusbar1.Push (0, "Analyzing file " + System.IO.Path.GetFileName (path));


		#if LOAD_SYNC

		LoadRomTask (path);
		LoadRomDone (null);

		#else

		Task task = new Task (() => LoadRomTask (path));
		// Exceptions must be handled in Task
		task.ContinueWith (t => LoadRomDone (t));

		task.Start ();

		#endif
	}

	void LoadRomTask (string path)
	{
		Application.Invoke (delegate {
			this.openAction.Sensitive = false;
			this.statusbar1.Show ();
		});

		stopwatch.Reset ();
		stopwatch.Start ();
		data.LoadRom (path);
	}

	void LoadRomDone (Task t)
	{
		stopwatch.Stop ();

		Application.Invoke (delegate {
			if (t != null && t.Status == TaskStatus.Faulted) {
				Console.Error.WriteLine ("Exception processing ROM:");
				Console.Error.WriteLine (t.Exception.ToString ());
				openAction.Sensitive = true;
			} else {
				SetWindowTitle ();
				SetActionsSensitiveForRomLoaded (true);

				string txt = string.Format ("Processing ROM finished in {0} ms.", stopwatch.ElapsedMilliseconds.ToString ());
				this.progressbar1.Text = txt;
				this.statusbar1.Push (0, "Updating UI ...");
				DoPendingEvents ();
				Console.WriteLine (txt);

				PopulateNavBar ();

				dataView2DGtk.ShowIcons = false;
				dataView3DGtk.ShowIcons = false;
				ClearVisualizations ();

				data.UpdateUI ();

				if (Config.IconsOnByDefault) {
					dataView2DGtk.ShowIcons = true;
					dataView3DGtk.ShowIcons = true;
				}
			}

			this.statusbar1.Hide ();
			this.statusbar1.Pop (0);
			this.progressbar1.Text = string.Empty;

			OnNotebook1SwitchPage (null, null);
		});
	}

	void PopulateNavBar ()
	{
		if (!data.RomLoaded) {
			navbarwidget.Clear ();
			return;
		}

		var regions = new List<Util.Region> (data.List2D.Count * 3 + data.List3D.Count * 4 + 1);

		navbarwidget.FirstPos = 0;
		navbarwidget.LastPos = data.Rom.Size - 1;

		try {
			var rcs = data.Rom.RomChecksumming;
			var checksums = rcs.ReadTableRecords ();
			var regionsTop = new List<Util.Region> (checksums.Count);
			foreach (var cs in checksums) {
				regionsTop.Add (new Util.Region (cs.StartAddress, cs.EndAddress, Util.RegionType.Checksummed));
			}
			navbarwidget.SetRegionsTop (regionsTop.ToArray ());
		} catch (Exception ex) {
			Console.Error.WriteLine (ex.ToString ());
		}

		var searchRange = data.TableSearchRange;
		if (searchRange.HasValue) {
			regions.Add (new Util.Region (searchRange.Value.Pos, searchRange.Value.Last, Util.RegionType.TableSearch));
		}

		var tables2D = data.List2D;
		foreach (var t in tables2D) {
			regions.Add (new Util.Region (t.Location, t.Location + t.RecordSize - 1, Util.RegionType.TableRecord2D));
			regions.Add (new Util.Region (t.RangeX.Pos, t.RangeX.Last, Util.RegionType.AxisX));
			regions.Add (new Util.Region (t.RangeY.Pos, t.RangeY.Last, Util.RegionType.ValuesY));
		}

		var tables3D = data.List3D;
		foreach (var t in tables3D) {
			regions.Add (new Util.Region (t.Location, t.Location + t.RecordSize - 1, Util.RegionType.TableRecord3D));
			regions.Add (new Util.Region (t.RangeX.Pos, t.RangeX.Last, Util.RegionType.AxisX));
			regions.Add (new Util.Region (t.RangeY.Pos, t.RangeY.Last, Util.RegionType.AxisY));
			regions.Add (new Util.Region (t.RangeZ.Pos, t.RangeZ.Last, Util.RegionType.ValuesZ));
		}

		navbarwidget.SetRegions (regions.ToArray ());
	}

	static void DoPendingEvents ()
	{
		while (Application.EventsPending ()) {
			Application.RunIteration ();
		}
	}

	void SetWindowTitle ()
	{
		this.Title = data.RomLoaded ? string.Format ("{0} - {1}", MainClass.AppName, data.CalID) : MainClass.AppName;
	}

	void ClearVisualizations ()
	{
		navbarwidget.ClearMarkedPositions ();

		plotSurface.Clear ();
		plotSurface.Refresh ();

		Gtk.Widget widget;
		widget = this.scrolledwindowTable2D.Child;
		if (widget != null)
			this.scrolledwindowTable2D.Remove (widget);

		widget = this.scrolledwindowTable3D.Child;
		if (widget != null)
			this.scrolledwindowTable3D.Remove (widget);
	}

	void Show3D (Tables.Denso.Table3D table)
	{
		if (table == null)
			return;

		navbarwidget.CurrentPos = table.Location;

		navbarwidget.SetMarkedPositions (new int[] { table.RangeX.Pos, table.RangeY.Pos, table.RangeZ.Pos });

		var valuesZ = table.GetValuesZasFloats ();
		var tableUI = new GtkWidgets.TableWidget3D (coloring, table.ValuesX, table.ValuesY, valuesZ,
			              table.Xmin, table.Xmax, table.Ymin, table.Ymax, table.Zmin, table.Zmax);
		tableUI.TitleMarkup = Util.Markup.NameUnit_Large (table.Title, table.UnitZ);
		tableUI.AxisXMarkup = Util.Markup.NameUnit (table.NameX, table.UnitX);
		tableUI.AxisYMarkup = Util.Markup.NameUnit (table.NameY, table.UnitY);
		tableUI.FormatValues = ScoobyRom.Data.AutomaticValueFormat (valuesZ, table.Zmin, table.Zmax);

		// Viewport needed for ScrolledWindow to work as generated table widget has no scroll support
		var viewPort = new Gtk.Viewport ();
		viewPort.Add (tableUI.Create ());

		Gtk.Widget previous = this.scrolledwindowTable3D.Child;
		if (previous != null)
			this.scrolledwindowTable3D.Remove (previous);
		// previous.Dispose () or previous.Destroy () cause NullReferenceException!

		this.scrolledwindowTable3D.Add (viewPort);
		this.scrolledwindowTable3D.ShowAll ();
	}

	void SetClipboardText (string s)
	{
		var cb = this.GetClipboard (Gdk.Selection.Clipboard);
		cb.Text = s;
	}

	void Show2D (Tables.Denso.Table2D table)
	{
		if (table == null)
			return;

		navbarwidget.CurrentPos = table.Location;
		navbarwidget.SetMarkedPositions (new int[] { table.RangeX.Pos, table.RangeY.Pos });

		// plot
		plot2D.Draw (table);

		// table data as text
		var values = table.GetValuesYasFloats ();
		var tableUI = new GtkWidgets.TableWidget2D (coloring, table.ValuesX, values, table.Xmin, table.Xmax, table.Ymin, table.Ymax);
		tableUI.HeaderAxisMarkup = Util.Markup.Unit (table.UnitX);
		tableUI.HeaderValuesMarkup = Util.Markup.Unit (table.UnitY);
		tableUI.AxisXMarkup = Util.Markup.NameUnit (table.NameX, table.UnitX);
		tableUI.ValuesMarkup = Util.Markup.NameUnit (table.Title, table.UnitY);
		tableUI.FormatValues = ScoobyRom.Data.AutomaticValueFormat (values, table.Ymin, table.Ymax);

		// Viewport needed for ScrolledWindow to work as generated table widget has no scroll support
		var viewPort = new Gtk.Viewport ();
		viewPort.Add (tableUI.Create ());

		Gtk.Widget previous = this.scrolledwindowTable2D.Child;
		if (previous != null)
			this.scrolledwindowTable2D.Remove (previous);
		// previous.Dispose () or previous.Destroy () cause NullReferenceException!

		this.scrolledwindowTable2D.Add (viewPort);
		this.scrolledwindowTable2D.ShowAll ();
	}


	#region UI Events

	void OnNotebook1SwitchPage (object o, Gtk.SwitchPageArgs args)
	{
		iconsAction.Active = CurrentView.ShowIcons;
		exportTableAsCSVAction.Sensitive = CurrentUI == ActiveUI.View2D;
	}

	void OnVisualizationAction (object sender, System.EventArgs e)
	{
		if (!data.RomLoaded)
			return;

		var table = CurrentTable;
		if (table is Tables.Denso.Table2D) {
			Show2D ((Tables.Denso.Table2D)table);
		} else {
			Show3D ((Tables.Denso.Table3D)table);
		}
	}

	void OnAbout (object sender, System.EventArgs e)
	{
		AboutDialog dialog = new AboutDialog {
			ProgramName = MainClass.AppName,
			Version = MainClass.AppVersion,
			Copyright = MainClass.AppCopyright,
			Comments = MainClass.AppDescription,
			Authors = new string[] { "subdiesel\thttp://subdiesel.wordpress.com/",
				"\nThanks for any feedback!",
				"\nEXTERNAL BINARY DEPENDENCIES:",
				"Gtk#\thttp://mono-project.com/GtkSharp",
				"Florence\thttp://github.com/scottstephens/Florence",
				"gnuplot\thttp://www.gnuplot.info/",
			},
			WrapLicense = true,
		};

		dialog.Icon = dialog.Logo = MainClass.AppIcon;

		string licensePath = MainClass.LicensePath;
		try {
			dialog.License = System.IO.File.ReadAllText (licensePath);
		} catch (System.IO.FileNotFoundException) {
			dialog.License = "Could not load license file '" + licensePath + "'.\nGo to http://www.fsf.org";
		}

		// default works fine on Linux, need extra work on Windows it seems...
		//AboutDialog.SetUrlHook (HandleAboutDialogActivateLinkFunc);

		dialog.Run ();
		dialog.Destroy ();
	}

	/*
	void HandleAboutDialogActivateLinkFunc (AboutDialog about, string uri)
	{
	}
	*/

	void OnOpenActionActivated (object sender, System.EventArgs e)
	{
		Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog ("Choose ROM file to open", this, FileChooserAction.Open, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Open, ResponseType.Accept);

		Gtk.FileFilter ff;
		ff = new FileFilter { Name = "ROM, BIN, HEX files" };
		ff.AddPattern ("*.rom");
		ff.AddPattern ("*.bin");
		ff.AddPattern ("*.hex");
		fc.AddFilter (ff);

		ff = new FileFilter { Name = "All files" };
		ff.AddPattern ("*");
		fc.AddFilter (ff);

		ResponseType response = (ResponseType)fc.Run ();
		string path = fc.Filename;
		fc.Destroy ();

		if (response == ResponseType.Accept) {
			try {
				OpenRom (path);
			} catch (System.Exception ex) {
				ErrorMsg ("Error opening file", ex.Message);
			}
		}
	}

	void OnSaveActionActivated (object sender, System.EventArgs e)
	{
		// TODO consider over-write warning for first time save
		try {
			data.SaveXml ();
		} catch (System.Exception ex) {
			ErrorMsg ("Error saving file", ex.Message);
		}
	}

	Gtk.ResponseType DisplaySelectDataDialog (out SelectedChoice choice)
	{
		var dialog = new SelectDataDialog (data);
		var response = (Gtk.ResponseType)dialog.Run ();
		choice = dialog.Choice;
		dialog.Destroy ();
		return response;
	}

	void OnExportAsRRActionActivated (object sender, System.EventArgs e)
	{
		SelectedChoice choice;
		var responseType = DisplaySelectDataDialog (out choice);
		if (responseType == ResponseType.Cancel)
			return;

		string pathSuggested = ScoobyRom.Data.PathWithNewExtension (data.Rom.Path, ".RR.xml");
		var fc = new Gtk.FileChooserDialog ("Export as RomRaider definition file", this,
			         FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "XML files";
			filter.AddPattern ("*.xml");
			// would show other XML files like .svg (on Linux at least): filter.AddMimeType ("text/xml");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.SetFilename (pathSuggested);
			// in addition this is necessary to populate filename when target file does not exist yet:
			fc.CurrentName = System.IO.Path.GetFileName (pathSuggested);

			if (fc.Run () == (int)ResponseType.Accept) {
				data.SaveAsRomRaiderXml (fc.Filename, choice);
			}
		} catch (Exception ex) {
			ErrorMsg ("Error writing file", ex.Message);
		} finally {
			// Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
			if (fc != null)
				fc.Destroy ();
		}
	}

	void OnExportAsXDFActionActivated (object sender, EventArgs e)
	{
		SelectedChoice choice;
		var responseType = DisplaySelectDataDialog (out choice);
		if (responseType == ResponseType.Cancel)
			return;

		string pathSuggested = ScoobyRom.Data.PathWithNewExtension (data.Rom.Path, ".xdf");
		var fc = new Gtk.FileChooserDialog ("Export as TunerPro XDF file", this,
			         FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "XDF files";
			filter.AddPattern ("*.xdf");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.SetFilename (pathSuggested);
			fc.CurrentName = System.IO.Path.GetFileName (pathSuggested);

			if (fc.Run () == (int)ResponseType.Accept) {
				data.SaveAsTunerProXdf (fc.Filename, choice);
			}
		} catch (Exception ex) {
			ErrorMsg ("Error writing file", ex.Message);
		} finally {
			if (fc != null)
				fc.Destroy ();
		}
	}

	// closing main app window
	void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	void OnQuitActionActivated (object sender, System.EventArgs e)
	{
		OnDeleteEvent (this, new DeleteEventArgs ());
	}

	// icons ON/OFF
	void OnIconsActionActivated (object sender, System.EventArgs e)
	{
		var view = CurrentView;
		if (view == null)
			return;
		view.ShowIcons = iconsAction.Active;
	}

	void OnIncreaseIconSizeActionActivated (object sender, System.EventArgs e)
	{
		var view = CurrentView;
		if (view == null)
			return;
		view.IncreaseIconSize ();
	}

	void OnDecreaseIconSizeActionActivated (object sender, EventArgs e)
	{
		var view = CurrentView;
		if (view == null)
			return;
		view.DecreaseIconSize ();
	}

	void OnZoomNormalActionActivated (object sender, EventArgs e)
	{
		var view = CurrentView;
		if (view == null)
			return;
		view.ResetIconSize ();
	}

	void OnNavigationBarActionActivated (object sender, EventArgs e)
	{
		if (navigationBarAction.Active) {
			navScrolledWindow.ShowAll ();
		} else {
			navScrolledWindow.HideAll ();
		}
	}

	// create or close gnuplot window
	void OnPlotActionActivated (object sender, System.EventArgs e)
	{
		var table = CurrentTable;
		if (table == null)
			return;

		try {
			// gnuplot process itself can be slow to startup
			// so this does not prevent closing it immediatly when pressed twice
			//plotExternalAction.Sensitive = false;
			GnuPlot.ToggleGnuPlot (table);
		} catch (GnuPlotProcessException ex) {
			Console.Error.WriteLine (ex);
			ErrorMsg ("Error launching gnuplot!", ex.Message
			+ "\n\nHave you installed gnuplot?"
			+ "\nYou also may need to edit file '" + MainClass.AssemblyPath + ".exe.config'."
			+ "\nCurrent platform-ID is '" + System.Environment.OSVersion.Platform.ToString () + "'."
			+ "\nSee 'README.txt' for details.");
		} catch (GnuPlotException ex) {
			Console.Error.WriteLine (ex);
			ErrorMsg ("Error launching gnuplot!", ex.Message);
		} catch (System.IO.FileNotFoundException ex) {
			Console.Error.WriteLine (ex);
			ErrorMsg ("Error using gnuplot!", ex.Message + "\nFile: " + ex.FileName);
		}
	}

	// depends on gnuplot
	void OnCreateSVGFileActionActivated (object sender, System.EventArgs e)
	{
		if (data.RomLoaded == false)
			return;

		var table = CurrentTable;
		if (table == null)
			return;

		GnuPlot gnuPlot = GnuPlot.GetExistingGnuPlot (table);
		if (gnuPlot == null) {
			ErrorMsg ("Error creating SVG export", "Need existing gnuplot window. Do a normal plot first.");
			return;
		}

		string filenameSuggested = string.IsNullOrEmpty (table.Title) ? "plot" : table.Title;
		filenameSuggested += ".svg";
		if (svgDirectory == null && data.Rom.Path != null)
			svgDirectory = System.IO.Path.GetDirectoryName (data.Rom.Path);

		var fc = new Gtk.FileChooserDialog ("Export plot as SVG file", this, FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "SVG files";
			filter.AddPattern ("*.svg");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.SetCurrentFolder (svgDirectory);
			fc.CurrentName = filenameSuggested;
			if (fc.Run () == (int)ResponseType.Accept) {
				GnuPlot.CreateSVG (table, fc.Filename);
			}
			// remember used dir
			svgDirectory = System.IO.Path.GetDirectoryName (fc.Filename);
		} catch (GnuPlotException ex) {
			ErrorMsg ("Error creating SVG file", ex.Message);
		} catch (System.IO.IOException ex) {
			ErrorMsg ("IO Exception", ex.Message);
		} catch (Exception ex) {
			// Access to path denied...
			ErrorMsg ("Error", ex.Message);
		} finally {
			// Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
			if (fc != null)
				fc.Destroy ();
		}
	}

	void OnROMChecksumsActionActivated (object sender, System.EventArgs e)
	{
		// sharing internal FileStream won't work, could use another one though
		if (!data.RomLoaded)
			return;

		// Use a Window (not Dialog) to allow working in main window with new window still open.
		// Also supports multiple monitors.
		var d = new ChecksumWindow ();
		d.SetData (data);
		d.Show ();
	}

	void OnPropertiesWindowActionActivated (object sender, System.EventArgs e)
	{
		if (!data.RomLoaded)
			return;

		var d = new PropertiesWindow (data);
		d.Show ();
	}

	void OnExportTableAsCSVActionActivated (object sender, System.EventArgs e)
	{
		if (data.RomLoaded == false)
			return;

		Tables.Denso.Table table = CurrentTable;
		if (table == null)
			return;

		if (table is Tables.Denso.Table3D) {
			ErrorMsg ("Error", "Creating CSV for 3D table not implemented yet.");
			return;
		}

		string filenameSuggested = string.IsNullOrEmpty (table.Title) ? "table" : table.Title;
		filenameSuggested += ".csv";
		// TODO another var to remember export dir
		if (svgDirectory == null && data.Rom.Path != null)
			svgDirectory = System.IO.Path.GetDirectoryName (data.Rom.Path);

		var fc = new Gtk.FileChooserDialog ("Export data as CSV file", this, FileChooserAction.Save, Gtk.Stock.Cancel, ResponseType.Cancel, Gtk.Stock.Save, ResponseType.Accept);
		try {
			FileFilter filter = new FileFilter ();
			filter.Name = "CSV files";
			filter.AddPattern ("*.csv");
			fc.AddFilter (filter);

			filter = new FileFilter ();
			filter.Name = "All files";
			filter.AddPattern ("*");
			fc.AddFilter (filter);

			fc.DoOverwriteConfirmation = true;
			fc.SetCurrentFolder (svgDirectory);
			fc.CurrentName = filenameSuggested;
			if (fc.Run () == (int)ResponseType.Accept) {
				using (System.IO.StreamWriter sw = new System.IO.StreamWriter (fc.Filename, false, System.Text.Encoding.UTF8)) {
					((Tables.Denso.Table2D)table).WriteCSV (sw);
				}
			}
			// remember used dir
			svgDirectory = System.IO.Path.GetDirectoryName (fc.Filename);
		} catch (GnuPlotException ex) {
			ErrorMsg ("Error creating CSV file", ex.Message);
		} catch (System.IO.IOException ex) {
			ErrorMsg ("IO Exception", ex.Message);
		} catch (Exception ex) {
			// Access to path denied...
			ErrorMsg ("Error", ex.Message);
		} finally {
			// Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
			if (fc != null)
				fc.Destroy ();
		}
	}

	void OnCopyTableAction (object sender, EventArgs e)
	{
		var table = CurrentTable;
		if (table == null)
			return;
		SetClipboardText (table.CopyTableRomRaider ());
	}

	#endregion UI Events

	void SetActionsSensitiveForRomLoaded (bool sensitive)
	{
		openAction.Sensitive = sensitive;
		saveAction.Sensitive = sensitive;
		exportAsAction.Sensitive = sensitive;
		exportAsRRAction.Sensitive = sensitive;
		exportAsXDFAction.Sensitive = sensitive;

		visualisationAction.Sensitive = sensitive;

		iconsAction.Sensitive = sensitive;
		zoomInAction.Sensitive = sensitive;
		zoomOutAction.Sensitive = sensitive;
		zoomNormalAction.Sensitive = sensitive;

		selectAllAction.Sensitive = sensitive;
		selectNoneAction.Sensitive = sensitive;

		checksumWindowAction.Sensitive = sensitive;
		propertiesWindowAction.Sensitive = sensitive;

		plotExternalAction.Sensitive = sensitive;
		createSVGFileAction.Sensitive = sensitive;
		copyAction.Sensitive = sensitive;
	}

	/// <summary>
	/// Displays a simple MessageDialog with error-icon and close-button.
	/// </summary>
	/// <param name="text">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="title">
	/// A <see cref="System.String"/>
	/// </param>
	void ErrorMsg (string title, string text)
	{
		MessageDialog md = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, text);
		md.UseMarkup = false;
		md.SecondaryUseMarkup = false;
		md.Title = title;
		md.Run ();
		md.Destroy ();
	}

	protected void OnSelectAllActionActivated (object sender, EventArgs e)
	{
		CurrentViewModel.ToggleAll (true);
	}

	protected void OnSelectNoneActionActivated (object sender, EventArgs e)
	{
		CurrentViewModel.ToggleAll (false);
	}
}
