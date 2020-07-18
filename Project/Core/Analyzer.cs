using GameFramework.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Data;

namespace Core
{
	public static class Analyzer
	{
		public class Info
		{
			public int ID
			{
				get;
				private set;
			}

			public string Symbol
			{
				get;
				private set;
			}

			public DataTable HistoryData
			{
				get;
				private set;
			}

			public DataTable LiveData
			{
				get;
				private set;
			}

			public DataTable AnalyzedData
			{
				get;
				private set;
			}

			public Info(int ID, string Symbol, DataTable HistoryData, DataTable LiveData, DataTable AnalyzedData)
			{
				this.ID = ID;
				this.Symbol = Symbol;
				this.HistoryData = HistoryData;
				this.LiveData = LiveData;
				this.AnalyzedData = AnalyzedData;
			}
		}

		public class TendLine
		{
			private class Spot
			{
				public int ID;

				public int Index;

				public int Time;

				public int High;
				public int Low;
				public int Close;

				public bool IsExtremum;
			}

			private class SpotList : List<Spot>
			{ }

			private class ValidateResult
			{
				public Spot FirstSpot;
				public bool IsValid;
				public float Slope;
				public int HitCount;
			}

			private const int SECONDS_PER_DAY = 86400;
			private const int DAYS_PER_MONTH = 22;

			private const int LONG_TERM_SECONDS = 6 * DAYS_PER_MONTH * SECONDS_PER_DAY;

			private const float SHORT_TERM_INFILITRATE_RATE = 0.01F;
			private const float LONG_TERM_INFILITRATE_RATE = 0.03F;

			public static void Analyze(Info Info)
			{
				DataTable data = Info.HistoryData;

				int dayCount = data.Rows.Count;
				double allowedRate = MathHelper.Clamp(SECONDS_PER_DAY * dayCount / (double)LONG_TERM_SECONDS * LONG_TERM_INFILITRATE_RATE, SHORT_TERM_INFILITRATE_RATE, LONG_TERM_INFILITRATE_RATE);

				int startFromIndex = 0;//???? fetch

				// Ascending
				SpotList pitSpots = new SpotList();
				FindExtremums(data, startFromIndex, pitSpots, (PrevClose, CurrClose, NextClose) => { return (PrevClose >= CurrClose && CurrClose <= NextClose); });

				ValidateResult growthResult = ValidateSpots(pitSpots,
					(float Slope) => { return (Slope > float.Epsilon); },
					(Spot Spot, float EstimatedClose) =>
					{
						return (Spot.Close < EstimatedClose &&
								((Spot.High < EstimatedClose && Spot.Low < EstimatedClose) ||
								((EstimatedClose - Spot.Close) / Spot.Close > allowedRate)));
					});

				// Descending
				SpotList peakSpots = new SpotList();
				FindExtremums(data, startFromIndex, peakSpots, (PrevClose, CurrClose, NextClose) => { return (CurrClose >= PrevClose && NextClose <= CurrClose); });

				ValidateResult shrinkResult = ValidateSpots(peakSpots,
					(float Slope) => { return (Slope < -float.Epsilon); },
					(Spot Spot, float EstimatedClose) =>
					{
						return (Spot.Close > EstimatedClose &&
								((Spot.High > EstimatedClose && Spot.Low > EstimatedClose) ||
								((Spot.Close - EstimatedClose) / Spot.Close > allowedRate)));
					});

				//if (growthResult.IsValid || shrinkResult.IsValid)
				//{
				//	Console.Write("Stock: {0} ", Info.ID);

				//	Console.Write("Status: ");

				//	if (growthResult.IsValid && shrinkResult.IsValid)
				//		Console.Write("Suspicious ");

				//	if (growthResult.IsValid)
				//		Console.Write("Growing Worthiness: {0}% Speed: ~{1}IRR/day ", (int)(growthResult.HitCount / (float)pitSpots.Count * 100), (int)(growthResult.Slope * SECONDS_PER_DAY));

				//	if (shrinkResult.IsValid)
				//		Console.Write("Shrinking Worthiness: {0}% Speed: ~{1}IRR/day ", (int)(shrinkResult.HitCount / (float)peakSpots.Count * 100), -(int)(shrinkResult.Slope * SECONDS_PER_DAY));

				//	Console.WriteLine();
				//}

				if (growthResult.IsValid != shrinkResult.IsValid)
				{
					int action = growthResult.IsValid ? 1 : -1;
					float worthiness = growthResult.IsValid ? (growthResult.HitCount / (float)pitSpots.Count) : (shrinkResult.HitCount / (float)peakSpots.Count);

					Data.Database.Execute("INSERT INTO analyzes(stock_id, analyze_time, action, worthiness, first_snapshot_id) VALUES(@stock_id, NOW(), @action, @worthiness, @first_snapshot_id)",
					"stock_id", Info.ID,
					"action", action,
					"worthiness", worthiness,
					"first_snapshot_id", growthResult.FirstSpot.ID);
				}
			}

			private static void FindExtremums(DataTable Data, int StartIndex, SpotList List, Func<int, int, int, bool> Comparison)
			{
				Spot prevSpot = null;
				for (int i = StartIndex + 1; i < Data.Rows.Count - 1; ++i)
				{
					DataRow prevRow = Data.Rows[i - 1];
					DataRow currRow = Data.Rows[i];
					DataRow nextRow = Data.Rows[i + 1];

					int prevClose = Convert.ToInt32(prevRow["close"]);
					int currClose = Convert.ToInt32(currRow["close"]);
					int nextClose = Convert.ToInt32(nextRow["close"]);

					if (!Comparison(prevClose, currClose, nextClose))
						continue;

					if (prevSpot != null && prevSpot.Close == currClose)
						continue;

					prevSpot = new Spot() { Index = i, Time = Convert.ToInt32(currRow["relative_time"]), High = Convert.ToInt32(currRow["high"]), Low = Convert.ToInt32(currRow["low"]), Close = currClose, IsExtremum = true };

					List.Add(prevSpot);
				}

				if (prevSpot == null)
					return;

				for (int i = prevSpot.Index + 1; i < Data.Rows.Count; ++i)
				{
					DataRow currRow = Data.Rows[i];

					int currClose = Convert.ToInt32(currRow["close"]);

					if (prevSpot != null && prevSpot.Close == currClose)
						continue;

					prevSpot = new Spot() { Index = i, Time = Convert.ToInt32(currRow["relative_time"]), High = Convert.ToInt32(currRow["high"]), Low = Convert.ToInt32(currRow["low"]), Close = currClose, IsExtremum = false };

					List.Add(prevSpot);
				}
			}

			private static ValidateResult ValidateSpots(SpotList List, Func<float, bool> CheckSlope, Func<Spot, float, bool> CheckInfiltration)
			{
				Spot firstSpot = null;

				int startIndex = 0;

				int hitCount = 0;
				float slope = 0;

				while (startIndex < List.Count - 2)
				{
					firstSpot = List[startIndex++];
					Spot secondSpot = List[startIndex];

					slope = CalculateSlope(firstSpot, secondSpot);
					if (!CheckSlope(slope))
					{
						hitCount = 0;
						slope = 0;

						continue;
					}

					bool lineBroke = false;
					for (int i = startIndex + 1; i < List.Count; ++i)
					{
						Spot spot = List[i];

						float estimatedClose = CalculateClose(firstSpot, slope, spot.Time);

						if (CheckInfiltration(spot, estimatedClose))
						{
							startIndex = i;

							hitCount = 0;
							slope = 0;

							lineBroke = true;

							break;
						}

						if (spot.IsExtremum)
							++hitCount;
					}

					if (lineBroke)
						continue;

					break;
				}

				return new ValidateResult() { FirstSpot = firstSpot, IsValid = (hitCount != 0), Slope = slope, HitCount = 2 + hitCount };
			}

			private static float CalculateSlope(Spot A, Spot B)
			{
				return (B.Close - A.Close) / (float)(B.Time - A.Time);
			}

			private static float CalculateClose(Spot First, float Slope, int Time)
			{
				return (Slope * (Time - First.Time)) + First.Close;
			}
		}

		public class RelativeStrengthIndex
		{
			private const int MINIMUM_HISTORY_COUNT = 14;

			public static void Analyze(Info Info)
			{
				DataTable data = Info.HistoryData;

				DataTable rsiTable = GenerateRSIData(data);

				double lastRSI = 0;
				for (int i = 0; i < rsiTable.Rows.Count; ++i)
				{
					DataRow row = rsiTable.Rows[i];

					double rsi = Convert.ToDouble(row["rsi"]);

					
				}
			}

			private static DataTable GenerateRSIData(DataTable Data)
			{
				if (Data.Rows.Count < MINIMUM_HISTORY_COUNT)
					return null;

				int fromIndex = Data.Rows.Count - MINIMUM_HISTORY_COUNT;

				DataTable rsiData = new DataTable();
				rsiData.Columns.Add("gain", typeof(int));
				rsiData.Columns.Add("loss", typeof(int));
				rsiData.Columns.Add("rsi", typeof(double));

				double gainAvg = 0;
				double lossAvg = 0;

				for (int i = fromIndex; i < Data.Rows.Count; ++i)
				{
					DataRow row = Data.Rows[i];

					int open = Convert.ToInt32(row["open"]);
					int close = Convert.ToInt32(row["close"]);

					int gain = (close > open ? close - open : 0);
					int loss = (open > close ? open - close : 0);

					rsiData.Rows.Add(gain, loss, 0);

					gainAvg += gain;
					lossAvg += loss;
				}

				gainAvg /= MINIMUM_HISTORY_COUNT;
				lossAvg /= MINIMUM_HISTORY_COUNT;

				rsiData.Rows[0]["rsi"] = CalculateRSI(gainAvg, lossAvg);

				for (int i = 1; i < rsiData.Rows.Count; ++i)
				{
					DataRow row = rsiData.Rows[i];

					gainAvg = (gainAvg * (MINIMUM_HISTORY_COUNT - 1) + Convert.ToInt32(row["gain"])) / MINIMUM_HISTORY_COUNT;
					lossAvg = (lossAvg * (MINIMUM_HISTORY_COUNT - 1) + Convert.ToInt32(row["loss"])) / MINIMUM_HISTORY_COUNT;

					row["rsi"] = CalculateRSI(gainAvg, lossAvg);
				}

				rsiData.Columns.RemoveAt(0);
				rsiData.Columns.RemoveAt(0);

				return rsiData;
			}

			private static double CalculateRSI(double GainAverage, double LossAverage)
			{
				if (LossAverage == 0)
					return 100;

				return 100 - (100 / (1 + GainAverage / LossAverage));
			}
		}

		public static class DirectionChange
		{
			public static void Analyze(Info Info)
			{
				List<int> closes = new List<int>();

				int prevSign = 0;
				for (int i = 0; i < Info.HistoryData.Rows.Count; ++i)
				{
					DataRow row = Info.HistoryData.Rows[i];

					int close = Convert.ToInt32(row["close"]);

					int prevClose = 0;
					Info.LiveData.DefaultView.RowFilter = "symbol='" + Info.Symbol + "'";
					if (Info.LiveData.DefaultView.Count != 0)
						prevClose = Convert.ToInt32(Info.LiveData.DefaultView[0]["close"]);

					int sign = Math.Sign(close - prevClose);

					if (sign == 0)
						continue;

					float rate = close / (float)prevClose;

					rate *= 100;

					if (prevSign != sign)
					{
						//Data.Database.Execute("INSERT INTO analyze_results(stock_id, analyze_time, action, action_time) VALUES(@stock_id, NOW(), @action, NOW())", //TIMESTAMPADD(second, 6 * 3600, TIMESTAMPADD(DAY, 1, DATE(NOW())))
						//	"stock_id", Info.ID,
						//	"action", sign);

						closes.Clear();
						closes.Add(prevClose);
					}

					closes.Add(close);

					prevSign = sign;
				}
			}
		}
	}
}