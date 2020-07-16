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

			public Info(int ID, string Symbol, DataTable HistoryData, DataTable LiveData)
			{
				this.ID = ID;
				this.Symbol = Symbol;
				this.HistoryData = HistoryData;
				this.LiveData = LiveData;
			}
		}

		public class TendLine
		{
			private class Spot
			{
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
				double rate = MathHelper.Clamp(SECONDS_PER_DAY / (double)LONG_TERM_SECONDS * LONG_TERM_INFILITRATE_RATE, SHORT_TERM_INFILITRATE_RATE, LONG_TERM_INFILITRATE_RATE);

				// Ascending
				SpotList pitSpots = new SpotList();
				FindExtremums(data, pitSpots, (PrevClose, CurrClose, NextClose) => { return (PrevClose >= CurrClose && CurrClose <= NextClose); });

				ValidateResult growthResult = ValidateSpots(data, pitSpots, 0,
					(float Slope) => { return (Slope > float.Epsilon); },
					(Spot Spot, float EstimatedClose) =>
					{
						return !(Spot.Close < EstimatedClose &&
								Spot.High < EstimatedClose &&
								Spot.Low < EstimatedClose &&
								(EstimatedClose - Spot.Close) / Spot.Close > rate);
					});

				// Descending
				SpotList peakSpots = new SpotList();
				FindExtremums(data, peakSpots, (PrevClose, CurrClose, NextClose) => { return (CurrClose >= PrevClose && NextClose <= CurrClose); });

				ValidateResult shrinkResult = ValidateSpots(data, peakSpots, 0,
					(float Slope) => { return (Slope < -float.Epsilon); },
					(Spot Spot, float EstimatedClose) =>
					{
						return !(Spot.Close > EstimatedClose &&
								Spot.High > EstimatedClose &&
								Spot.Low > EstimatedClose &&
								(Spot.Close - EstimatedClose) / Spot.Close > rate);
					});

				if (growthResult.IsValid || shrinkResult.IsValid)
				{
					Console.Write("Stock: {0} ", Info.ID);

					Console.Write("Status: ");

					if (growthResult.IsValid && shrinkResult.IsValid)
						Console.Write("Suspicious ");

					if (growthResult.IsValid)
						Console.Write("Growing Speed: ~{0}IRR/day ", (int)(growthResult.Slope * SECONDS_PER_DAY));

					if (shrinkResult.IsValid)
						Console.Write("Shrinking Speed: ~{0}IRR/day ", -(int)(shrinkResult.Slope * SECONDS_PER_DAY));

					Console.Write("Worthiness: {0}%", (int)((growthResult.IsValid ? growthResult.HitCount : 1) / (float)(shrinkResult.IsValid ? shrinkResult.HitCount : 1) * 100));

					Console.WriteLine();
				}
			}

			private static void FindExtremums(DataTable Data, SpotList List, Func<int, int, int, bool> Comparison)
			{
				Spot prevSpot = null;
				for (int i = 1; i < Data.Rows.Count - 1; ++i)
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

			private static ValidateResult ValidateSpots(DataTable Data, SpotList List, int StartIndex, Func<float, bool> CheckSlope, Func<Spot, float, bool> CheckInfiltrate)
			{
				int startIndex = StartIndex;

				int hitCount = 0;
				float slope = 0;

				while (startIndex < List.Count - 2)
				{
					Spot firstSpot = List[startIndex++];
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

						if (!CheckInfiltrate(spot, estimatedClose))
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

				return new ValidateResult() { IsValid = (hitCount != 0), Slope = slope, HitCount = 2 + hitCount };
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