using OfficeOpenXml;
using System;
using System.Globalization;

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
			return Self.ToString("yyyy/MM/dd HH:mm:ss");
		}

		public static string ToPersianDate(this DateTime Self)
		{
			PersianCalendar persianCalendar = new PersianCalendar();

			return string.Format("{0}/{1}/{2}", persianCalendar.GetYear(Self), persianCalendar.GetMonth(Self), persianCalendar.GetDayOfMonth(Self));
		}
	}
}
