using GameFramework.DatabaseManaged;
using GameFramework.DatabaseManaged.Migration;
using System;

namespace Core
{
	public static class Data
	{
		private static readonly string[] MIGRATIONS = new string[] {
			MIGRATION_BOURSE_2020071101,
			MIGRATION_BOURSE_2020071102,
			MIGRATION_BOURSE_2020071201,
			MIGRATION_BOURSE_2020071301,
			MIGRATION_BOURSE_2020071401,
			MIGRATION_BOURSE_2020071801,
			MIGRATION_BOURSE_2020071802 };

		private static readonly string[] MIGRATIONS_NAME = new string[] {
			"Migration_Bourse_2020071101",
			"Migration_Bourse_2020071102",
			"Migration_Bourse_2020071201",
			"Migration_Bourse_2020071301",
			"Migration_Bourse_2020071401",
			"Migration_Bourse_2020071801",
			"Migration_Bourse_2020071802" };

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
			ALTER TABLE `snapshot_data` 
			DROP COLUMN `sell_count`,
			DROP COLUMN `sell_amount`,
			DROP COLUMN `sell_price`,
			DROP COLUMN `buy_price`,
			DROP COLUMN `buy_amount`,
			DROP COLUMN `buy_count`,
			DROP COLUMN `pe`,
			DROP COLUMN `eps`;";

		private const string MIGRATION_BOURSE_2020071201 = @"
			ALTER TABLE `snapshot_data` 
			DROP COLUMN `last_price_percent`,
			DROP COLUMN `last_price_change`,
			DROP COLUMN `last_transaction_percent`,
			DROP COLUMN `last_transaction_change`,
			CHANGE COLUMN `maximum` `high` INT NOT NULL AFTER `first`,
			CHANGE COLUMN `minimum` `low` INT NOT NULL AFTER `high`,
			CHANGE COLUMN `last_transaction_amount` `last` INT NOT NULL,
			CHANGE COLUMN `last_price_amount` `close` INT NOT NULL AFTER `last`,
			CHANGE COLUMN `size` `volume` BIGINT(64) NOT NULL,
			CHANGE COLUMN `value` `value` BIGINT(64) NOT NULL,
			CHANGE COLUMN `yesterday` `open` INT NOT NULL;";

		private const string MIGRATION_BOURSE_2020071301 = @"
			ALTER TABLE `snapshot_data` RENAME TO  `snapshots`;";

		private const string MIGRATION_BOURSE_2020071401 = @"
			CREATE TABLE `analyze_results` (
				`id` INT NOT NULL AUTO_INCREMENT,
				`stock_id` INT NOT NULL,
				`analyze_time` DATETIME NOT NULL,
				`action` INT NOT NULL,
				`action_time` DATETIME NOT NULL,
				PRIMARY KEY (`id`)
			);";

		private const string MIGRATION_BOURSE_2020071801 = @"
			ALTER TABLE `analyze_results` 
			DROP COLUMN `action_time`,
			ADD COLUMN `worthiness` FLOAT NOT NULL AFTER `action`,
			ADD COLUMN `first_snapshot_id` INT NOT NULL AFTER `worthiness`;";

		private const string MIGRATION_BOURSE_2020071802 = @"
			ALTER TABLE `analyze_results` 
			RENAME TO `analyzes` ;";

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
