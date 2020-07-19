using OfficeOpenXml;
using System;

namespace Core
{
	static class Extensions
	{
		public static T GetValue<T>(this ExcelWorksheet Self, int Row, int Column, T DefaultValue = default(T))
		{
			try
			{
				return Self.GetValue<T>(Row, Column);
			}
			catch
			{ }

			return DefaultValue;
		}

		public static string ToDatabaseDateTime(this DateTime Self)
		{
			return Self.ToString("yyyy/MM/dd hh:mm:ss");
		}
	}
}
