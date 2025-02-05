// This is an open source non-commercial project. Dear PVS-Studio, please check it.

// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;

namespace Util
{
	public enum RegionType
	{
		Undefined,
		AxisX,
		AxisY,
		ValuesY,
		ValuesZ,
		TableRecord2D,
		TableRecord3D,
		TableSearch,
		Checksummed
	}

	public static class RegionHelper
	{
		public static string ToStr (this RegionType rt)
		{
			string s = null;
			switch (rt) {
			case RegionType.AxisX:
				s = "AxisX";
				break;
			case RegionType.AxisY:
				s = "AxisY";
				break;
			case RegionType.ValuesY:
				s = "ValuesY";
				break;
			case RegionType.ValuesZ:
				s = "ValuesZ";
				break;
			case RegionType.TableRecord2D:
				s = "TableRecord2D";
				break;
			case RegionType.TableRecord3D:
				s = "TableRecord3D";
				break;
			case RegionType.TableSearch:
				s = "(TableSearch)";
				break;
			case RegionType.Checksummed:
				s = "(Checksummed)";
				break;
			default:
				break;
			}
			return s;
		}
	}
}
