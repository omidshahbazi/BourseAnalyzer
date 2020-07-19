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
			public int ID;

			public string Symbol;

			public DataTable HistoryData;
			public DataTable LiveData;
			public DataTable AnalyzesData;
		}

		public class Result
		{
			public int Action;
			public double Worthiness;
			public int FirstSnapshotID;
		}

		public static class TendLine
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

			private static int LongTermSeconds
			{
				get { return ConfigManager.Config.Analyzer.TrendLine.LongTermSeconds; }
			}

			private static float ShortTermInfiltrate
			{
				get { return ConfigManager.Config.Analyzer.TrendLine.ShortTermInfiltrate; }
			}

			private static float LongTermInfiltrate
			{
				get { return ConfigManager.Config.Analyzer.TrendLine.LongTermInfiltrate; }
			}

			public static Result Analyze(Info Info)
			{
				DataTable data = Info.HistoryData;

				int dayCount = data.Rows.Count;
				double allowedRate = MathHelper.Clamp(SECONDS_PER_DAY * dayCount / (double)LongTermSeconds * LongTermInfiltrate, ShortTermInfiltrate, LongTermInfiltrate);

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
					int action = (growthResult.IsValid ? 1 : -1);
					float worthiness = growthResult.IsValid ? (growthResult.HitCount / (float)pitSpots.Count) : (shrinkResult.HitCount / (float)peakSpots.Count);

					return new Result() { Action = action, Worthiness = worthiness, FirstSnapshotID = growthResult.FirstSpot.ID };
				}

				return null;
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

		public static class RelativeStrengthIndex
		{
			//https://blog.quantinsti.com/rsi-indicator/

			private const float MAX_RSI = 1;

			private static int MaxHistoryCount
			{
				get { return ConfigManager.Config.Analyzer.RelativeStrengthIndex.MaxHistoryCount; }
			}

			private static int CalclationCount
			{
				get { return ConfigManager.Config.Analyzer.RelativeStrengthIndex.CalclationCount; }
			}

			private static float LowRSI
			{
				get { return ConfigManager.Config.Analyzer.RelativeStrengthIndex.LowRSI; }
			}

			private static float MidRSI
			{
				get { return ConfigManager.Config.Analyzer.RelativeStrengthIndex.MidRSI; }
			}

			private static float HighRSI
			{
				get { return ConfigManager.Config.Analyzer.RelativeStrengthIndex.HighRSI; }
			}

			public static Result Analyze(Info Info)
			{
				DataTable data = Info.HistoryData;

				DataTable rsiTable = GenerateRSIData(data);
				if (rsiTable == null)
					return null;

				int lastIndex = rsiTable.Rows.Count - 1;
				double prevRSI = Convert.ToDouble(rsiTable.Rows[lastIndex - 1]["rsi"]);
				double currRSI = Convert.ToDouble(rsiTable.Rows[lastIndex]["rsi"]);

				int action = 0;
				double worthiness = 0;

				if (prevRSI <= LowRSI && LowRSI < currRSI)
				{
					action = 1;
					worthiness = (LowRSI - prevRSI) / LowRSI;
				}
				else if (prevRSI <= MidRSI && MidRSI < currRSI)
				{
					action = 1;
				}
				else if (HighRSI <= prevRSI && currRSI < HighRSI)
				{
					action = -1;
					worthiness = (prevRSI - HighRSI) / (MAX_RSI - HighRSI);
				}
				else if (MidRSI <= prevRSI && currRSI < MidRSI)
				{
					action = -1;
				}

				//if (action == 1)
				//	Console.WriteLine("Buy: {0} RSI: {1}% Worthiness: {2}%", Info.ID, (int)(currRSI * 100), (int)(worthiness * 100));
				//else if (action == -1)
				//	Console.WriteLine("Sell: {0} RSI: {1}% Worthiness: {2}%", Info.ID, (int)(currRSI * 100), (int)(worthiness * 100));

				return new Result() { Action = action, Worthiness = worthiness, FirstSnapshotID = Convert.ToInt32(data.Rows[data.Rows.Count - MaxHistoryCount]["id"]) };
			}

			private static DataTable GenerateRSIData(DataTable Data)
			{
				int requiredCount = MaxHistoryCount + CalclationCount - 1;

				if (Data.Rows.Count < requiredCount)
					return null;

				int startFromIndex = Data.Rows.Count - requiredCount;

				double gainAvg = 0;
				double lossAvg = 0;

				DataTable rsiData = new DataTable();
				rsiData.Columns.Add("gain", typeof(int));
				rsiData.Columns.Add("loss", typeof(int));
				rsiData.Columns.Add("rsi", typeof(double));

				for (int i = 0; i < requiredCount; ++i)
				{
					DataRow row = Data.Rows[startFromIndex + i];

					int open = Convert.ToInt32(row["open"]);
					int close = Convert.ToInt32(row["close"]);

					int gain = (open < close ? close - open : 0);
					int loss = (close < open ? open - close : 0);

					rsiData.Rows.Add(gain, loss, 0);

					if (i < MaxHistoryCount)
					{
						gainAvg += gain;
						lossAvg += loss;
					}
				}

				gainAvg /= MaxHistoryCount;
				lossAvg /= MaxHistoryCount;

				startFromIndex = rsiData.Rows.Count - CalclationCount;

				rsiData.Rows[startFromIndex++]["rsi"] = CalculateRSI(gainAvg, lossAvg);

				for (int i = 0; i < CalclationCount - 1; ++i)
				{
					DataRow row = rsiData.Rows[startFromIndex + i];

					gainAvg = (gainAvg * (MaxHistoryCount - 1) + Convert.ToInt32(row["gain"])) / MaxHistoryCount;
					lossAvg = (lossAvg * (MaxHistoryCount - 1) + Convert.ToInt32(row["loss"])) / MaxHistoryCount;

					row["rsi"] = CalculateRSI(gainAvg, lossAvg);
				}

				rsiData.Columns.RemoveAt(0);
				rsiData.Columns.RemoveAt(0);

				for (int i = 1; i < startFromIndex; ++i)
					rsiData.Rows.RemoveAt(0);

				return rsiData;
			}

			private static double CalculateRSI(double GainAverage, double LossAverage)
			{
				if (LossAverage == 0)
					return 1;

				return MAX_RSI - (MAX_RSI / (1 + (GainAverage / LossAverage)));
			}
		}
	}
}