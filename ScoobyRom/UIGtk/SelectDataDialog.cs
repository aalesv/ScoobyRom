// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;

namespace ScoobyRom
{
	public partial class SelectDataDialog : Gtk.Dialog
	{
		Data data;

		public SelectDataDialog (Data data)
		{
			this.data = data;
			this.Build (); //-V3068
			Update ();
		}

		public SelectedChoice Choice {
			get {
				if (this.radiobuttonAll.Active)
					return SelectedChoice.All;
				else if (this.radiobuttonSelected.Active)
					return SelectedChoice.Selected;
				else if (this.radiobuttonAnnotated.Active)
					return SelectedChoice.Annotated;
				else
					return SelectedChoice.Undefined;
			}
		}

		public void Update ()
		{
			if (data == null)
				return;

			label2DCountTotal.Text = data.List2D.Count.ToString ();
			label3DCountTotal.Text = data.List3D.Count.ToString ();

			label2DSelected.Text = data.List2DSelected ().Count.ToString ();
			label3DSelected.Text = data.List3DSelected ().Count.ToString ();

			label2DAnnotated.Text = data.List2DAnnotated ().Count.ToString ();
			label3DAnnotated.Text = data.List3DAnnotated ().Count.ToString ();
		}
	}
}
