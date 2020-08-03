using GameFramework.Common.Utilities;
using GameFramework.DatabaseManaged;
using GameFramework.DatabaseManaged.Migration;
using System;
using System.Data;

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
			MIGRATION_BOURSE_2020071802,
			MIGRATION_BOURSE_2020071901,
			MIGRATION_BOURSE_2020071902,
			MIGRATION_BOURSE_2020072001,
			MIGRATION_BOURSE_2020072002,
			MIGRATION_BOURSE_2020072003,
			MIGRATION_BOURSE_2020072004,
			MIGRATION_BOURSE_2020072005,
			MIGRATION_BOURSE_2020072101,
			MIGRATION_BOURSE_2020072301,
			MIGRATION_BOURSE_2020072601,
			MIGRATION_BOURSE_2020072701,
			MIGRATION_BOURSE_2020080101,
			MIGRATION_BOURSE_2020080201,
			MIGRATION_BOURSE_2020080301 };

		private static readonly string[] MIGRATIONS_NAME = new string[] {
			"Migration_Bourse_2020071101",
			"Migration_Bourse_2020071102",
			"Migration_Bourse_2020071201",
			"Migration_Bourse_2020071301",
			"Migration_Bourse_2020071401",
			"Migration_Bourse_2020071801",
			"Migration_Bourse_2020071802",
			"Migration_Bourse_2020071901",
			"Migration_Bourse_2020071902",
			"Migration_Bourse_2020072001",
			"Migration_Bourse_2020072002",
			"Migration_Bourse_2020072003",
			"Migration_Bourse_2020072004",
			"Migration_Bourse_2020072005",
			"Migration_Bourse_2020072101",
			"Migration_Bourse_2020072301",
			"Migration_Bourse_2020072601",
			"Migration_Bourse_2020072701",
			"Migration_Bourse_2020080101",
			"Migration_Bourse_2020080201",
			"Migration_Bourse_2020080301" };

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

		private const string MIGRATION_BOURSE_2020071901 = @"
			CREATE VIEW `analyzes_view` AS
			SELECT s.id, s.symbol, s.name, a.analyze_time, a.action, a.worthiness*100 worthiness FROM analyzes a INNER JOIN stocks s ON a.stock_id=s.id;";

		private const string MIGRATION_BOURSE_2020071902 = @"
			CREATE TABLE `analyzes_validation` (
				`id` INT NOT NULL AUTO_INCREMENT,
				`analyze_id` INT NOT NULL,
				`was_valid` INT NOT NULL,
				PRIMARY KEY (`id`)
			);";

		private const string MIGRATION_BOURSE_2020072001 = @"
			CREATE TABLE `trades` (
				`id` INT NOT NULL AUTO_INCREMENT,
				`trader_id` INT NOT NULL,
				`stock_id` INT NOT NULL,
				`price` INT NOT NULL,
				`count` INT NOT NULL,
				`action` INT NOT NULL,
				`action_time` DATETIME NOT NULL,
				PRIMARY KEY (`id`)
			);";

		private const string MIGRATION_BOURSE_2020072002 = @"
			CREATE TABLE `traders` (
				`id` int NOT NULL AUTO_INCREMENT,
				`name` text NOT NULL,
				`email` text NOT NULL,
				PRIMARY KEY (`id`)
			);";

		private const string MIGRATION_BOURSE_2020072003 = @"
			CREATE TABLE `worker_schedules` (
				`id` INT NOT NULL AUTO_INCREMENT,
				`name` TEXT NOT NULL,
				`schedule_time` DATETIME NOT NULL,
				`done` INT NOT NULL,
				PRIMARY KEY (`id`)
			);";

		private const string MIGRATION_BOURSE_2020072004 = @"
			CREATE 
				OR REPLACE ALGORITHM = UNDEFINED 
				DEFINER = `root`@`localhost` 
				SQL SECURITY DEFINER
			VIEW `analyzes_view` AS
				SELECT 
				`s`.`id` AS `id`,
				`s`.`symbol` AS `symbol`,
				`s`.`name` AS `name`,
				`a`.`analyze_time` AS `analyze_time`,
				`a`.`action` AS `action`,
				(`a`.`worthiness` * 100) AS `worthiness`,
				`v`.`was_valid` AS `was_valid`
			FROM
				(`analyzes` `a`
				JOIN `stocks` `s` ON ((`a`.`stock_id` = `s`.`id`))
				JOIN `analyzes_validation` `v` ON ((`v`.`analyze_id` = `a`.`id`)));";

		private const string MIGRATION_BOURSE_2020072005 = @"
			CREATE 
				OR REPLACE ALGORITHM = UNDEFINED 
				DEFINER = `root`@`localhost` 
				SQL SECURITY DEFINER
			VIEW `analyzes_view` AS
				SELECT 
				`s`.`id` AS `id`,
				`s`.`symbol` AS `symbol`,
				`s`.`name` AS `name`,
				`a`.`analyze_time` AS `analyze_time`,
				`a`.`action` AS `action`,
				(`a`.`worthiness` * 100) AS `worthiness`,
				`v`.`was_valid` AS `was_valid`
			FROM
				(`analyzes` `a`
				JOIN `stocks` `s` ON ((`a`.`stock_id` = `s`.`id`))
				LEFT OUTER JOIN `analyzes_validation` `v` ON ((`v`.`analyze_id` = `a`.`id`)));";

		private const string MIGRATION_BOURSE_2020072101 = @"
			ALTER TABLE `analyzes` 
			DROP COLUMN `first_snapshot_id`;";

		private const string MIGRATION_BOURSE_2020072301 = @"
			ALTER TABLE `traders` 
			ADD COLUMN `send_full_sell_report` INT NOT NULL AFTER `email`;";

		private const string MIGRATION_BOURSE_2020072601 = @"
			ALTER TABLE `traders` 
			CHANGE COLUMN `email` `emails` TEXT NOT NULL ;";

		private const string MIGRATION_BOURSE_2020072701 = @"
			ALTER TABLE `traders` 
			ADD COLUMN `is_admin` INT NOT NULL AFTER `send_full_sell_report`;";

		private const string MIGRATION_BOURSE_2020080101 = @"
			ALTER TABLE `analyzes_validation` 
			ADD COLUMN `validate_time` DATETIME NOT NULL AFTER `analyze_id`;";

		private const string MIGRATION_BOURSE_2020080201 = @"
			CREATE VIEW `trades_view` AS
			SELECT s.id, s.symbol, s.name stock_name, ts.name trader_name, t.price, t.count, (t.price*t.count) total_price, t.action_time FROM trades t INNER JOIN traders ts ON t.trader_id=ts.id INNER JOIN stocks s on t.stock_id=s.id;";

		private const string MIGRATION_BOURSE_2020080301 = @"
			CREATE OR REPLACE ALGORITHM = UNDEFINED 
				DEFINER = `root`@`localhost` 
				SQL SECURITY DEFINER
			VIEW `trades_view` AS
				SELECT s.id, s.symbol, s.name stock_name, ts.name trader_name, AVG(t.price) average_price, SUM(t.count*t.action) count, SUM(t.price*t.count*t.action) total_price, t.action_time FROM trades t INNER JOIN traders ts ON t.trader_id=ts.id INNER JOIN stocks s on t.stock_id=s.id GROUP BY t.trader_id, t.stock_id;";

		public static Database Database
		{
			get;
			private set;
		}

		static Data()
		{
			CreateConnection();

			Func<string, string> migrationLoader = (string Name) =>
			{
				return MIGRATIONS[Array.IndexOf(MIGRATIONS_NAME, Name)];
			};

			MigrationManager.Migrate(Database, MIGRATIONS_NAME, migrationLoader);
		}

		public static void Execute(string Query, params object[] Parameters)
		{
			Database.Execute(Query, Parameters);
		}

		public static DataTable QueryDataTable(string Query, params object[] Parameters)
		{
			try
			{
				return Database.QueryDataTable(Query, Parameters);
			}
			catch (Exception e)
			{
				ConsoleHelper.WriteException(e, "QueryDataTable has failed");
			}

			CreateConnection();

			return QueryDataTable(Query, Parameters);
		}

		private static void CreateConnection()
		{
			Config conf = ConfigManager.Config;

			Database = new MySQLDatabase(conf.DatabaseConnection);
		}
	}
}