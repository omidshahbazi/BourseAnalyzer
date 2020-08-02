using GameFramework.Common.Utilities;
using System;
using System.Data;
using System.Diagnostics;

namespace Core
{
	public static partial class Analyzer
	{
		public static class RSI
		{
			//https://blog.quantinsti.com/rsi-indicator/

			private static float LowRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.LowRSI; }
			}

			private static float MidRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MidRSI; }
			}

			private static float HighRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.HighRSI; }
			}

			private static float MaxRSI
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.MaxRSI; }
			}

			private static float IgnoreThreshold
			{
				get { return ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.IgnoreThreshold; }
			}

			public static Result Analyze(Indicator.Info Info)
			{
				if (!ConfigManager.Config.DataAnalyzer.RelativeStrengthIndex.Enabled)
					return null;

				if (LowRSI <= 0 || MidRSI <= LowRSI)
				{
					ConsoleHelper.WriteError("LowRSI must be grater than 0 and smaller than MidRSI, current value is {0}", LowRSI);
					return null;
				}

				if (MidRSI <= LowRSI || HighRSI <= MidRSI)
				{
					ConsoleHelper.WriteError("MidRSI must be grater than LowRSI and smaller than HighRSI, current value is {0}", MidRSI);
					return null;
				}

				if (HighRSI <= MidRSI || MaxRSI <= HighRSI)
				{
					ConsoleHelper.WriteError("HighRSI must be grater than MidRSI and smaller than MaxRSI, current value is {0}", HighRSI);
					return null;
				}
				DataTable data = Info.HistoryData;

				if (!data.Columns.Contains("rsi"))
					return null;

				Result result = new Result() { Signals = new Signal[ConfigManager.Config.DataAnalyzer.BacklogCount] };

				for (int i = 0; i < result.Signals.Length; ++i)
				{
					int index = data.Rows.Count - 1 - i;
					double prevRSI = Convert.ToDouble(data.Rows[index - 1]["rsi"]);
					double currRSI = Convert.ToDouble(data.Rows[index]["rsi"]);

					int action = 0;
					double worthiness = 0;

					if (Math.Abs(currRSI - prevRSI) >= IgnoreThreshold)
					{
						if (prevRSI <= LowRSI && LowRSI < currRSI)
						{
							action = 1;
							worthiness = (LowRSI - prevRSI) / LowRSI;
						}
						if (HighRSI <= prevRSI && currRSI < HighRSI)
						{
							action = -1;
							worthiness = (prevRSI - HighRSI) / (MaxRSI - HighRSI);
						}
						else
						{
							double prevClose = Convert.ToDouble(data.Rows[index - 1]["close"]);
							double currClose = Convert.ToDouble(data.Rows[index]["close"]);

							if (Analyzer.CheckPointCrossover(prevRSI, currRSI, MidRSI, out action))
							{
								if (prevClose >= currClose)
									action = 0;
								else if (prevClose <= currClose)
									action = 0;
							}

							Debug.Assert(false, "Calculate worthiness like MACD");
						}
					}

					result.Signals[result.Signals.Length - 1 - i] = new Signal() { Worthiness = action * worthiness };
				}

				return result;
			}
		}
	}
}