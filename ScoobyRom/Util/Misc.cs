using System;

namespace Util
{
	public static class Misc
	{
		// Do not use "Gnome.Vfs.Vfs.FormatFileSizeForDisplay (value)" from gnome-vfs-sharp.dll
		// works fine but this assembly is not included in Gtk# (Windows installer) !
		// Linux package: gnome-vfs-sharp "Mono bindings for GNOME-VFS"

		public static string SizeForDisplay (long byteCount)
		{
			// longs run out around EB
			string[] suffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
			if (byteCount == 0)
				return "0" + suffix [0];
			long bytes = Math.Abs (byteCount);
			//int place = Convert.ToInt32 (Math.Floor (Math.Log (bytes, 1024)));
			// (int)double does the needed Math.Floor
			int place = (int)Math.Log (bytes, 1024);
			double num = bytes / Math.Pow (1024, place);
			int decimals = num > 100 ? 1 : 2;
			num = Math.Round (Math.Sign (byteCount) * num, decimals);
			return num.ToString () + " " + suffix [place];
		}

		/* // alternative
		public static string SizeForDisplay (double len)
		{
			string[] sizes = { "B", "KB", "MB", "GB" };
			int order = 0;
			while (len >= 1024 && (order + 1 < sizes.Length)) {
				order++;
				len = len / 1024;
			}
			return string.Format ("{0:0.#} {1}", len, sizes [order]);
		}
		*/
	}
}
