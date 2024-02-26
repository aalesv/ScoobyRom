using System;
using Cairo;
using Gtk;
using System.Collections.Generic;

namespace GtkWidgets
{
	/// <summary>
	/// Nav bar widget.
	/// In GUI designer leave this empty: "Common Widget Properties" -> "Events", otherwise some events may not work
	/// </summary>
	[System.ComponentModel.ToolboxItem (true)]
	public sealed class NavBarWidget : Gtk.DrawingArea
	{
		//public event EventHandler<EventArgs> Changed;

		const double LineWidth = 1;
		// max performance, filled anyway
		const double LineWidthRegions = 0;
		int minWidth = 50;
		const int minRectHeight = 16;
		const int padLeft = 10;
		const int padRight = padLeft;
		const int padTop = 4;
		const int padBottom = padTop;
		const int minHeight = padTop + minRectHeight + padBottom;
		const double ZoomFactor = 1.5;

		int width, height, backWidth, backHeight;
		Cairo.Rectangle totalRect;
		Gdk.Rectangle clipping_area;
		Viewport viewport;
		int firstPos, lastPos;
		int currentPos;
		Util.Region[] regions, regionsTop;
		int[] markedPositions;
		double posFactor;
		/// <summary>
		/// Relative to full view where world fits exactly into physical display view width.
		/// for info only, not required, not perfectly implemented yet
		/// </summary>
		double zoom = 1.0;
		Cairo.Color ColorBack = new Cairo.Color (1, 1, 1);
		Cairo.Color ColorFrame = new Cairo.Color (0, 0, 0);
		// red
		Cairo.Color ColorCurrentPos = new Cairo.Color (1, 0, 0, 0.9);
		// red brown
		Cairo.Color ColorMarkedPos = new Cairo.Color (165 / 255.0, 42 / 255.0, 42 / 255.0, 0.9);

		#region boilerplate constructors

		public NavBarWidget () : base ()
		{
			Init ();
		}

		public NavBarWidget (IntPtr raw) : base (raw)
		{
			Init ();
		}

		#endregion

		void Init ()
		{
			this.CanFocus = true;

			//this.SizeAllocated += new SizeAllocatedHandler(SizeAllocated);
			//this.ExposeEvent += new ExposeEventHandler(ExposeEvent);
			this.EnterNotifyEvent += new EnterNotifyEventHandler (OnEnterNotifyEvent);
			this.LeaveNotifyEvent += new LeaveNotifyEventHandler (OnLeaveNotifyEvent);
//			this.ButtonPressEvent += new ButtonPressEventHandler(ButtonPressEvent);
			this.MotionNotifyEvent += new MotionNotifyEventHandler (OnMotionNotifyEvent);
//			this.ButtonReleaseEvent += new ButtonReleaseEventHandler(ButtonReleaseEvent);
//			this.ScrollEvent += new ScrollEventHandler(OnScrollEvent);
			this.KeyPressEvent += new KeyPressEventHandler (OnKeyPressEvent);
//			this.KeyReleaseEvent += new KeyReleaseEventHandler (OnKeyReleaseEvent);

			// Subscribe to DrawingArea mouse movement and button press events.
			// Enter and Leave notification is necessary to make ToolTips work.
			// Specify PointerMotionHint to prevent being deluged with motion events.
			this.AddEvents ((int)Gdk.EventMask.EnterNotifyMask);
			this.AddEvents ((int)Gdk.EventMask.LeaveNotifyMask);
			this.AddEvents ((int)Gdk.EventMask.ButtonPressMask);
//			this.AddEvents ((int)Gdk.EventMask.ButtonReleaseMask);
			this.AddEvents ((int)Gdk.EventMask.PointerMotionMask);
//			this.AddEvents ((int)Gdk.EventMask.PointerMotionHintMask);
//			this.AddEvents ((int)Gdk.EventMask.ScrollMask);

			Clear ();
		}

		#region public properties and methods

		public void Clear ()
		{
			firstPos = 0;
			lastPos = -1;
			regions = null;
			regionsTop = null;
			markedPositions = null;
			QueueDraw ();
		}

		public void ZoomIn ()
		{
			UpdateViaZoom (ZoomFactor);
		}

		public void ZoomOut ()
		{
			UpdateViaZoom (1.0 / ZoomFactor);
		}

		public void ZoomReset ()
		{
			this.minWidth = 50;
			QueueResize ();
		}

		public int FirstPos {
			get { return firstPos; }
			set {
				if (firstPos == value)
					return;
				firstPos = value;
				UpdatePosRelated ();
			}
		}

		/// <summary>
		/// Gets or sets the last position.
		/// </summary>
		/// <value>
		/// The last pos < 0 means no data.
		/// </value>
		public int LastPos {
			get { return lastPos; }
			set {
				if (lastPos == value)
					return;
				lastPos = value;
				UpdatePosRelated ();
			}
		}

		public bool NoData {
			get { return lastPos < 0; }
		}

		int PosSize {
			get { return lastPos - firstPos + 1; }
		}

		public int CurrentPos {
			get { return currentPos; }
			set {
				if (currentPos == value)
					return;
				currentPos = value;
				QueueDraw ();
			}
		}

		public void SetRegions (Util.Region[] regions)
		{
			this.regions = regions;
			QueueDraw ();
		}

		public void SetRegionsTop (Util.Region[] regions)
		{
			this.regionsTop = regions;
			QueueDraw ();
		}

		public void ClearMarkedPositions ()
		{
			markedPositions = null;
			QueueDraw ();
		}

		public void SetMarkedPositions (int[] positions)
		{
			markedPositions = positions != null && positions.Length == 0 ? null : positions;
			QueueDraw ();
		}

		public int[] GetMarkedPositions ()
		{
			return markedPositions;
		}

		bool RegionsToDisplay {
			get { return regions != null && regions.Length > 0; }
		}

		bool RegionsTopToDisplay {
			get { return regionsTop != null && regionsTop.Length > 0; }
		}

		bool MarkedPositionsToDisplay {
			get { return markedPositions != null && markedPositions.Length > 0; }
		}

		// !!! Stetic designer seems to take every public setter,
		// generating code to set default value (0) in Build ().


		#endregion public properties and methods

		void PrintAdjustment (Adjustment ad)
		{
			Console.WriteLine ("Value={0} | Lower={1} | Upper={2} | StepIncrement={3} | PageIncrement={4} | PageSize={5}",
				ad.Value, ad.Lower, ad.Upper, ad.StepIncrement, ad.PageIncrement, ad.PageSize);
		}

		#region mono x64 optimizations

		// mono x64 asm tested: (ref Struct) to avoid stack activity at callsite

		static void SetColor (Cairo.Context cr, ref Cairo.Color color)
		{
			// Cairo.Context.Color = value calls SetSourceRGBA (,,,) internally
			cr.SetSourceRGBA (color.R, color.G, color.B, color.A);
		}

		static void Rectangle (Cairo.Context cr, ref Rectangle rect)
		{
			cr.Rectangle (rect.X, rect.Y, rect.Width, rect.Height);
		}

		#endregion

		double WorldToPhysicalX (double posWorld)
		{
			return padLeft + posFactor * posWorld;
		}

		double PhysicalToWorldX (double posPhysical)
		{
			return (posPhysical - padLeft) / posFactor;
		}

		#region events

		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			base.OnExposeEvent (ev);

			// Insert drawing code here.
			using (Cairo.Context cr = Gdk.CairoHelper.Create (ev.Window)) {
				DrawEverything (cr);
			}
			return true;
		}

		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			width = allocation.Width;
			height = allocation.Height;
			//Console.WriteLine ("OnSizeAllocated: {0}x{1}", width, height);

			backWidth = width - padLeft - padRight;
			backHeight = height - padTop - padBottom;

			UpdatePosRelated ();

			totalRect = new Cairo.Rectangle (WorldToPhysicalX (0), padTop, backWidth, backHeight);
			clipping_area = new Gdk.Rectangle (0, 0, width, height);

			base.OnSizeAllocated (allocation);
			// Insert layout code here.

			//PrintAdjustment (viewport.Hadjustment);
		}

		protected override void OnSizeRequested (ref Gtk.Requisition requisition)
		{
			// GUI designer automatically puts this widget into Viewport when property "Show Scrollbars" is set
			// necessary for automatic scroll support.
			// Need additional access in here to improve positioning when zooming.
			// cannot be initialized in constructor
			if (viewport == null) {
				viewport = (Gtk.Viewport)this.Parent;


				viewport.AddEvents ((int)Gdk.EventMask.ScrollMask);
				// not called when scrollbar is being moved
				//viewport.ScrollAdjustmentsSet += Viewport_ScrollAdjustmentsSet;
				viewport.ScrollEvent += Viewport_ScrollEvent;
				;

			}
			//var vr = viewport.Allocation;
			//Console.WriteLine ("Viewport size request Width={0} Height={1}", vr.Width, vr.Height);

			// Calculate desired size here.
			requisition.Width = minWidth;
			requisition.Height = minHeight;
			//Console.WriteLine ("OnSizeRequested -> Requisition: {0}x{1}", requisition.Width, requisition.Height);
		}

		void Viewport_ScrollEvent (object o, ScrollEventArgs args)
		{
			// need to update when scrolled and pointer still at same position, otherwise would display outdated info
			// HACK
			ClearTooltip ();
			// TODO update tooltip after scroll event
			// ok but not accurate, probably called before scroll is done
			//UpdateToolTip ();
		}

		void OnKeyPressEvent (object o, KeyPressEventArgs args)
		{
			//Console.WriteLine ("OnKeyPressEvent");
			const Gdk.ModifierType modifier = Gdk.ModifierType.Button1Mask;

			Gdk.Key key = args.Event.Key;
			if ((args.Event.State & modifier) != 0) {
				if (key == Gdk.Key.Key_0 || key == Gdk.Key.KP_0) {
					ZoomReset ();
					args.RetVal = true;     // Prevents further key processing
					return;
				} else if (key == Gdk.Key.plus || key == Gdk.Key.KP_Add) {
					ZoomIn ();
					args.RetVal = true;
					return;
				} else if (key == Gdk.Key.minus || key == Gdk.Key.KP_Subtract) {
					ZoomOut ();
					args.RetVal = true;
					return;
				}
			}
		}

		//		void OnKeyReleaseEvent (object o, KeyReleaseEventArgs args)
		//		{
		//			args.RetVal = true;
		//		}

		void OnLeaveNotifyEvent (object o, LeaveNotifyEventArgs args)
		{
			//Console.WriteLine ("OnLeaveNotifyEvent");
			if (this.HasFocus) {
				HasFocus = false;
			}
			args.RetVal = true;
		}

		void OnEnterNotifyEvent (object o, EnterNotifyEventArgs args)
		{
			//Console.WriteLine ("OnEnterNotifyEvent");
			// necessary for keys to work:
			if (!this.HasFocus)
				this.GrabFocus ();
			args.RetVal = true;
		}

		/* could also override Widget methods i.e.:
		protected override bool OnMotionNotifyEvent(Gdk.EventMotion evnt)
		{
			UpdateToolTip ();
			return base.OnMotionNotifyEvent (evnt);
		}
		*/

		void OnMotionNotifyEvent (object o, MotionNotifyEventArgs args)
		{
			UpdateToolTip ();
		}

		#endregion events

		void UpdatePosRelated ()
		{
			if (lastPos == 0)
				return;
			posFactor = (double)backWidth / (double)lastPos;
		}

		void UpdateViaZoom (double zoomRelative)
		{
			double physicalDelta = WorldToPhysicalX (1) - WorldToPhysicalX (0);
			// limit excessive zoom-in
			if (zoomRelative > 1 && physicalDelta > 1) {
				return;
			}

			// remember current left world pos for scrollbar pos
			double worldXleft = PhysicalToWorldX (viewport.Hadjustment.Value);

			double savePosFactor = posFactor;
			posFactor *= zoomRelative;
			zoom *= zoomRelative;

			minWidth = (int)(WorldToPhysicalX (lastPos) - WorldToPhysicalX (0)) + padLeft + padRight;

			if (minWidth < viewport.Allocation.Width) {
				// all visible
				posFactor = savePosFactor;
				zoom = 1.0;
			}

			//PrintAdjustment (viewport.Hadjustment);


			if (viewport != null) {
				//zoomRelative = zoomRelative > 1 ? 1.1 * zoomRelative : zoomRelative * 1.1;

				// otherwise Adjustment.Value would remain constant
				//viewport.Hadjustment.Value *= zoomRelative;
				// .Upper changes automatically

				viewport.Hadjustment.Value = WorldToPhysicalX (worldXleft);
			}
			QueueResize ();
			//Console.WriteLine ("zoom={0}", zoom.ToString ());
		}

		void DrawEverything (Cairo.Context cr)
		{
			// using Cairo and possibly Gtk.Style.Paint... commands

			cr.LineCap = LineCap.Butt;
			cr.LineJoin = LineJoin.Miter;
			cr.LineWidth = LineWidth;

			DrawBack (cr);

			if (NoData)
				return;

			DrawRegions (cr);
			DrawRegionsTop (cr);

			cr.LineWidth = LineWidth;

			DrawMarkedPositions (cr);

			DrawMarker (cr, ref ColorCurrentPos, currentPos);
		}

		void DrawRegions (Cairo.Context cr)
		{
			if (!RegionsToDisplay)
				return;
			cr.LineWidth = LineWidthRegions;
			foreach (var r in this.regions) {
				Cairo.Color color = Util.Coloring.RegionColor (r.RegionType);
				DrawRegion (cr, ref color, r.Pos1, r.Pos2);

				if (r.RegionType == Util.RegionType.TableSearch) {
					cr.LineWidth = LineWidth;
					DrawRangeMarker (cr, r.Pos2, ArrowType.Left);
					DrawGtkStyleRangeMarker (r.Pos2, ArrowType.Left);

					DrawRangeMarker (cr, r.Pos1, ArrowType.Right);
					DrawGtkStyleRangeMarker (r.Pos1, ArrowType.Right);
					cr.LineWidth = LineWidthRegions;
				}
			}
		}

		void DrawRegionsTop (Cairo.Context cr)
		{
			if (!RegionsTopToDisplay)
				return;
			cr.LineWidth = LineWidthRegions;
			foreach (var r in this.regionsTop) {
				Cairo.Color color = Util.Coloring.RegionColor (r.RegionType);
				DrawRegionTop (cr, ref color, r.Pos1, r.Pos2);
			}
		}

		void DrawMarkedPositions (Cairo.Context cr)
		{
			if (MarkedPositionsToDisplay) {
				SetColor (cr, ref ColorMarkedPos);
				if (markedPositions.Length == PosSize) {
					// all positions are marked
					Rectangle (cr, ref totalRect);
					cr.Fill ();
				} else {
					foreach (int i in markedPositions) {
						cr.MoveTo (WorldToPhysicalX (i), 0);
						cr.RelLineTo (0, height);
					}
					// single call is much faster
					cr.Stroke ();
				}
			}
		}

		/*
		void DrawBack ()
		{
			// PaintFlatBox: null → nothing; "tooltip" → frame, "button" → solid
			//Gtk.Style.PaintFlatBox (this.Style, this.GdkWindow, StateType.Normal, ShadowType.None, clipping_area, this, "tooltip",
			//                    padLeft, padTop, backWidth, backHeight);

			// PaintBox: null → frame
			Gtk.Style.PaintBox (this.Style, this.GdkWindow, StateType.Normal, ShadowType.None, clipping_area, this, null,
			                    padLeft, padTop, backWidth, backHeight);
		}
		*/

		void DrawBack (Cairo.Context cr)
		{
			Rectangle (cr, ref totalRect);
			if (!NoData && Sensitive) {
				SetColor (cr, ref ColorBack);
				cr.FillPreserve ();
			}
			SetColor (cr, ref ColorFrame);
			cr.Stroke ();
		}

		void DrawMarker (Cairo.Context cr, ref Cairo.Color color, int sampleIndex)
		{
			SetColor (cr, ref color);
			cr.MoveTo (WorldToPhysicalX (sampleIndex), 0);
			cr.RelLineTo (0, height);
			cr.Stroke ();
		}

		void DrawGtkStyleRangeMarker (int pos, ArrowType arrowType)
		{
			int x = Convert.ToInt32 (WorldToPhysicalX (pos));
			int height = Convert.ToInt32 (totalRect.Height);
			int y = Convert.ToInt32 (totalRect.Y);
			int dx = height / 2 + 2;

			if (arrowType == ArrowType.Right)
				x -= dx;

			Gtk.Style.PaintArrow (this.Style, this.GdkWindow, StateType.Normal, ShadowType.Out, clipping_area, this, "",
				arrowType, false, x, y, dx, height);
		}

		void DrawRangeMarker (Cairo.Context cr, int pos, ArrowType arrowType)
		{
			double x = WorldToPhysicalX (pos);
			double height = totalRect.Height;
			double y = totalRect.Y;
			double dx = 0.33 * height;

			if (arrowType == ArrowType.Right)
				dx = -dx;
			cr.MoveTo (x + dx, y);
			cr.LineTo (x, y + 0.5 * height);
			cr.LineTo (x + dx, y + height);
			cr.Stroke ();
		}

		void DrawRegion (Cairo.Context cr, ref Cairo.Color color, int pos1, int pos2)
		{
			double left = WorldToPhysicalX (pos1);
			double right = WorldToPhysicalX (pos2);

			SetColor (cr, ref color);
			cr.Rectangle (left, totalRect.Y + LineWidth, right - left, totalRect.Height - (2 * LineWidth));
			cr.Fill ();
		}

		void DrawRegionTop (Cairo.Context cr, ref Cairo.Color color, int pos1, int pos2)
		{
			double left = WorldToPhysicalX (pos1);
			double right = WorldToPhysicalX (pos2);

			SetColor (cr, ref color);
			cr.Rectangle (left, 0, right - left, totalRect.Y - 1);
			cr.Fill ();
		}

		#region tooltip

		void UpdateToolTip ()
		{
			if (NoData) {
				this.TooltipMarkup = "<i>No data</i>";
				return;
			}

			int X, Y;
			GetPointer (out X, out Y);
			int worldX = Convert.ToInt32 (PhysicalToWorldX (X));
			if (firstPos <= worldX && worldX <= lastPos) {
				var sb = new System.Text.StringBuilder ("<tt>0x", 50);
				sb.Append (worldX.ToString ("X"));
				sb.Append ("</tt>\n");
				sb.Append (Util.Misc.SizeForDisplay (worldX));
				sb.AppendLine ();
				sb.Append (worldX.ToString ());

				string s = GetContentInfo (worldX);
				if (s != null) {
					sb.AppendLine ();
					sb.Append (s);
				}

				TooltipMarkup = sb.ToString ();
			} else {
				// remove tooltip immediatly to avoid mismatch (mouse outside content rectangle)
				ClearTooltip ();
			}
		}

		void ClearTooltip ()
		{
			TooltipText = null;
		}

		string GetContentInfo (int worldX)
		{
			string s = null;
			if (RegionsToDisplay) {
				foreach (var r in this.regions) {
					if (r.Contains (worldX)) {
						s = Util.RegionHelper.ToStr (r.RegionType);
						if (r.RegionType == Util.RegionType.TableSearch) {
							continue;
						} else {
							break;
						}
					}
				}
			}
			return s;
		}

		#endregion tooltip
	}
}
