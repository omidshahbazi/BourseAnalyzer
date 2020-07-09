using GameFramework.DatabaseManaged;
using System.IO;
using Core;

namespace Importer
{
	class Program
	{
		static void Main(string[] args)
		{
			Config conf = ConfigManager.Config;

			Database db = new MySQLDatabase(conf.DatabaseConnection);

			byte[] stocksData = File.ReadAllBytes("C:/Users/Omid/Downloads/MarketWatchPlus-1399_4_18.xlsx");

			//XLSXImporter.Import(db, stocksData);
		}
	}
}
