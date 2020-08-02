using System;
using System.Data;
using System.Diagnostics;

namespace Core
{
	public static partial class Analyzer
	{
		public static class MACD_RSI_EMA
		{
			//https://finmaxbo.com/en/strategy/1160-macd-rsi-alligator.html
			//https://asanbourse.ir/Blog/?p=390

			private static float BuyMaxRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MaxRSI * 0.6F; }
			}

			private static float SellMinRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MaxRSI * 0.3F; }
			}

			public static Result Analyze(Indicator.Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.MovingAverageConvergenceDivergence.Enabled)
					return null;

				DataTable data = Info.HistoryData;

				if (!data.Columns.Contains("macd") || !data.Columns.Contains("rsi"))
					return null;

				Result result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount] };

				for (int i = 0; i < result.Signals.Length; ++i)
				{
					double worthiness = 0;

					int index = data.Rows.Count - 1 - i;

					if (index > 0)
					{
						int action = 0;
						int crossCheckResult = 0;

						double currRSI = Convert.ToDouble(data.Rows[index]["rsi"]);
						double currMACD = Convert.ToDouble(data.Rows[index]["macd"]);

						if (currMACD < 0)
						{
							if (Analyzer.CheckCrossover(Convert.ToDouble(data.Rows[index - 1]["macd"]), currMACD, Convert.ToDouble(data.Rows[index - 1]["signal"]), Convert.ToDouble(data.Rows[index]["signal"]), out crossCheckResult) && crossCheckResult > 0)
							{
								if (currRSI < BuyMaxRSI)
									action = 1;
							}
						}

						if (action < 1)
						{
							DataTable slowData = Indicator.GenerateExponentialMovingAverage(data, "close", 21, result.Signals.Length + 1);
							DataTable fastData = Indicator.GenerateExponentialMovingAverage(data, "close", 13, result.Signals.Length + 1);

							int smaIndex = slowData.Rows.Count - 1 - i;

							if (Analyzer.CheckCrossover(Convert.ToDouble(fastData.Rows[smaIndex - 1]["ema"]), Convert.ToDouble(fastData.Rows[smaIndex]["ema"]), Convert.ToDouble(slowData.Rows[smaIndex - 1]["ema"]), Convert.ToDouble(slowData.Rows[smaIndex]["ema"]), out crossCheckResult) && crossCheckResult < 0)
							{
								Debug.Assert(action <= 1);

								if (currRSI > SellMinRSI)
									action = -1;
							}
						}

						if (action != 0)
						{
							DataTable smaData = Indicator.GenerateSimpleMovingAverageData(data, "close", 9, 2);

							worthiness = action * Math.Abs((Convert.ToDouble(smaData.Rows[1]["sma"]) / Convert.ToDouble(smaData.Rows[0]["sma"])) - 1);
						}
					}

					result.Signals[result.Signals.Length - 1 - i] = new Signal() { Worthiness = worthiness };
				}

				return result;
			}
		}
	}
}