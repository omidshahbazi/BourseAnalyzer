using GameFramework.Common.Utilities;
using System;
using System.Data;

namespace Core
{
	public static partial class Analyzer
	{
		public static class MovingAverageConvergenceDivergence
		{
			private static float IgnoreThreshold
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.IgnoreThreshold; }
			}

			private static int PostPeriodCount
			{
				get { return ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.PostPeriodCount; }
			}

			public static Result Analyze(Indicator.Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.Enabled)
					return null;

				if (PostPeriodCount < 0)
				{
					ConsoleHelper.WriteError("PostPeriodCount cannot be negative, current value is {1}", PostPeriodCount);
					return null;
				}

				if (PostPeriodCount != 0 && IgnoreThreshold < 0)
				{
					ConsoleHelper.WriteError("IgnoreThreshold must be grater than 0, current value is {1}", IgnoreThreshold);
					return null;
				}

				Result result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount] };

				DataTable data = Info.HistoryData;

				for (int i = 0; i < result.Signals.Length; ++i)
				{
					int action = 0;
					double worthiness = 0;

					int index = data.Rows.Count - 1 - i - PostPeriodCount;

					if (index > 0)
					{
						double prevMACD = Convert.ToDouble(data.Rows[index - 1]["macd"]);
						double currMACD = Convert.ToDouble(data.Rows[index]["macd"]);

						double prevSignal = Convert.ToDouble(data.Rows[index - 1]["signal"]);
						double currSignal = Convert.ToDouble(data.Rows[index]["signal"]);

						int close = Convert.ToInt32(data.Rows[data.Rows.Count - 1 - i]["close"]);
						double threshold = (close == 0 ? 0 : Math.Abs(currMACD - currSignal) / close);
						if (PostPeriodCount == 0 || threshold >= IgnoreThreshold)
						{
							Analyzer.CheckCrossover(prevMACD, currMACD, prevSignal, currSignal, out action);

							if (action != 0)
							{
								DataTable smaData = Indicator.GenerateSimpleMovingAverageData(data, "close", 9, 2);

								worthiness = action * Math.Abs((Convert.ToDouble(smaData.Rows[1]["sma"]) / Convert.ToDouble(smaData.Rows[0]["sma"])) - 1);
							}
						}
					}

					result.Signals[result.Signals.Length - 1 - i] = new Signal() { Worthiness = worthiness };
				}

				return result;
			}
		}
	}
}