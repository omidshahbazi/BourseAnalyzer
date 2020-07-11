using System;
using System.Globalization;
using System.IO;
using Core;
using GameFramework.Common.Utilities;

namespace Importer
{
	class Program
	{
		private static void Main(string[] args)
		{
			ArgumentParser arguments = ArgumentParser.Parse(args);

			if (arguments.Contains("file"))
			{
				string filePath = arguments.Get<string>("file");
				if (!File.Exists(filePath))
				{
					ConsoleHelper.WriteError("File [{0}] doesn't exists", filePath);
					return;
				}

				Import(filePath);

				return;
			}

			if (arguments.Contains("directory"))
			{
				string directory = arguments.Get<string>("directory");
				if (!Directory.Exists(directory))
				{
					ConsoleHelper.WriteError("Directory [{0}] doesn't exists", directory);
					return;
				}

				string[] filesPath = Directory.GetFiles(directory, "*.xlsx", SearchOption.AllDirectories);
				for (int i = 0; i < filesPath.Length; ++i)
					Import(filesPath[i]);

				return;
			}

			ConsoleHelper.WriteInfo("use -file for single file or -directory for multiple files");
		}

		private static void Import(string FilePath)
		{
			DateTime dateTime;
			if (!GetDateFromFilePath(FilePath, out dateTime))
			{
				ConsoleHelper.WriteError("Something went wrong in finding date of file");
				return;
			}

			byte[] stocksData = File.ReadAllBytes(FilePath);

			ConsoleHelper.WriteInfo("Importing [{0}] for {1}...", FilePath, dateTime);

			XLSXImporter.Import(Data.Database, new XLSXImporter.Info { Time = dateTime, Data = stocksData });

			ConsoleHelper.WriteInfo("Importing done");
		}

		private static bool GetDateFromFilePath(string FilePath, out DateTime Date)
		{
			Date = DateTime.Now;

			FilePath = Path.GetFileNameWithoutExtension(FilePath);

			string[] parts = FilePath.Split('-');
			if (parts.Length != 2)
				return false;

			parts = parts[1].Split('_');
			if (parts.Length != 3)
				return false;

			Date = new DateTime(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]), 12, 0, 0, 0, new PersianCalendar());

			return true;
		}
	}
}
