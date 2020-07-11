using GameFramework.DatabaseManaged;
using GameFramework.DatabaseManaged.Migration;
using System;

namespace Core
{
	public static class Data
	{
		private static readonly string[] MIGRATIONS = new string[] { MIGRATION_BOURSE_2020071101, MIGRATION_BOURSE_2020071102 };
		private static readonly string[] MIGRATIONS_NAME = new string[] { "Migration_Bourse_2020071101", "Migration_Bourse_2020071102" };

		private const string MIGRATION_BOURSE_2020071101 = @"
			CREATE TABLE `stocks` (
				`id` int NOT NULL AUTO_INCREMENT,
				`symbol` text NOT NULL,
				`name` text NOT NULL,
				PRIMARY KEY(`id`)
			);

			CREATE TABLE `snapshot_data` (
				`id` int NOT NULL AUTO_INCREMENT,
				`stock_id` int NOT NULL,
				`take_time` datetime NOT NULL,
				`count` int NOT NULL,
				`size` int NOT NULL,
				`value` int NOT NULL,
				`yesterday` int NOT NULL,
				`first` int NOT NULL,
				`last_transaction_amount` int NOT NULL,
				`last_transaction_change` int NOT NULL,
				`last_transaction_percent` float NOT NULL,
				`last_price_amount` int NOT NULL,
				`last_price_change` int NOT NULL,
				`last_price_percent` float NOT NULL,
				`minimum` int NOT NULL,
				`maximum` int NOT NULL,
				`eps` int NOT NULL,
				`pe` float NOT NULL,
				`buy_count` int NOT NULL,
				`buy_amount` int NOT NULL,
				`buy_price` int NOT NULL,
				`sell_price` int NOT NULL,
				`sell_amount` int NOT NULL,
				`sell_count` int NOT NULL,
				PRIMARY KEY(`id`)
			);";

		private const string MIGRATION_BOURSE_2020071102 = @"
			ALTER TABLE `bourse_analyzer`.`snapshot_data` 
			DROP COLUMN `sell_count`,
			DROP COLUMN `sell_amount`,
			DROP COLUMN `sell_price`,
			DROP COLUMN `buy_price`,
			DROP COLUMN `buy_amount`,
			DROP COLUMN `buy_count`,
			DROP COLUMN `pe`,
			DROP COLUMN `eps`;";

		public static Database Database
		{
			get;
			private set;
		}

		static Data()
		{
			Config conf = ConfigManager.Config;

			Database = new MySQLDatabase(conf.DatabaseConnection);

			Func<string, string> migrationLoader = (string Name) =>
			{
				return MIGRATIONS[Array.IndexOf(MIGRATIONS_NAME, Name)];
			};

			MigrationManager.Migrate(Database, MIGRATIONS_NAME, migrationLoader);
		}
	}
}
