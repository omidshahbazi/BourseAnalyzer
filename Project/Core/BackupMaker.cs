using GameFramework.Common.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Core
{
	public class BackupMaker : Worker
	{
		public override bool Enabled
		{
			get { return ConfigManager.Config.BackupMaker.Enabled; }
		}

		public override float WorkHour
		{
			get { return ConfigManager.Config.BackupMaker.WorkHour; }
		}

		public override bool Do(DateTime CurrentDateTime)
		{
			string mySQLDumpPath = Path.Combine(ConfigManager.Config.BackupMaker.MySQLDumpPath, "mysqldump.exe");
			if (!File.Exists(mySQLDumpPath))
			{
				ConsoleHelper.WriteError("Couldn't find mysqldump in {0}", mySQLDumpPath);
				return false;
			}

			string outputPath = Path.GetFullPath(ConfigManager.Config.BackupMaker.OutputPath);
			if (!Directory.Exists(outputPath))
				Directory.CreateDirectory(outputPath);
			outputPath = Path.Combine(outputPath, string.Format("DatabaseBackup_{0}.sql", CurrentDateTime.ToString("yyyy-MM-dd")));

			StringBuilder arguments = new StringBuilder(); ;
			arguments.Append("--user=\"");
			arguments.Append(ConfigManager.Config.DatabaseConnection.Username);
			arguments.Append("\" --password=\"");
			arguments.Append(ConfigManager.Config.DatabaseConnection.Password);
			arguments.Append("\" --databases ");
			arguments.Append(ConfigManager.Config.DatabaseConnection.Name);
			arguments.Append(" > -r \"");
			arguments.Append(outputPath);
			arguments.Append("\"");

			Process.Start("\"" + mySQLDumpPath + "\"", arguments.ToString()).WaitForExit();

			return true;
		}
	}
}