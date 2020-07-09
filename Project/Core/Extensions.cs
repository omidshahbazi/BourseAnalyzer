using OfficeOpenXml;

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
	}
}
