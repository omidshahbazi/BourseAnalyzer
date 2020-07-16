using System;
using System.Collections.Generic;
using System.Data;

namespace Core
{
	public static class Analyzer
	{
		private class Spot
		{
			public int Index;
			public int Time;
			public int High;
			public int Low;
			public int Close;
		}

		private class SpotList : List<Spot>
		{ }

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
			public static void Analyze(Info Info)
			{
				//if (Info.HistoryData.Rows.Count < 0)
				//	return;

				//int beginTime = Convert.ToInt32(Info.HistoryData.Rows[0]["take_time"]);

				DataTable table = Info.HistoryData;

				SpotList pitSpots = new SpotList();
				FindExtremums(table, pitSpots, (PrevClose, CurrClose, NextClose) => { return !(CurrClose > PrevClose || NextClose < CurrClose); });

				SpotList peakSpots = new SpotList();
				FindExtremums(table, peakSpots, (PrevClose, CurrClose, NextClose) => { return (PrevClose > CurrClose || CurrClose < PrevClose); });

				ValidateSpots(pitSpots,
					(float Slope) => { return !(Slope < 0); },
					(Spot Spot, float EstimatedClose) =>
					{
						return !(Spot.Close < EstimatedClose &&
								Spot.High < EstimatedClose &&
								Spot.Low < EstimatedClose &&
								(EstimatedClose - Spot.Close) / Spot.Close > 0.01F);
					});

				//int startIndex = 0;
				//int hitCount = 0;
				//while (startIndex < pitSpots.Count - 2)
				//{
				//	Spot firstSpot = pitSpots[startIndex++];
				//	Spot secondSpot = pitSpots[startIndex];

				//	float slope = CalculateSlope(firstSpot, secondSpot);
				//	if (slope < 0)
				//	{
				//		hitCount = 0;

				//		continue;
				//	}

				//	bool lineBroke = false;
				//	for (int i = startIndex + 1; i < pitSpots.Count; ++i)
				//	{
				//		Spot spot = pitSpots[i];

				//		float y = CalculateClose(firstSpot, slope, spot.Time);

				//		if (spot.Close < y &&
				//			spot.High < y &&
				//			spot.Low < y &&
				//			(y - spot.Close) / spot.Close > 0.01F)
				//		{
				//			startIndex = i;
				//			hitCount = 0;
				//			lineBroke = true;

				//			break;
				//		}

				//		++hitCount;
				//	}

				//	if (lineBroke)
				//		continue;

				//	break;
				//}

				//if (hitCount != 0)
				//{
				//	Console.WriteLine("{0} {1} {2}%", Info.ID, hitCount, ((hitCount + 2) / (float)pitSpots.Count) * 100);
				//}

				//if (lowestLows.Count == 3)
				//{
				//Spot firstSpot = lowestLows[2];
				//Spot middleSpot = lowestLows[1];
				//Spot lastSpot = lowestLows[0];

				//float y = FindClose(firstSpot, middleSpot, lastSpot.Time);

				//if (y > lastSpot.Close || lastSpot.Close - y < changeAvg)
				//{
				//	// should buy
				//	//Data.Database.Execute("INSERT INTO analyze_results(stock_id, analyze_time, action, action_time) VALUES(@stock_id, NOW(), @action, NOW())", //TIMESTAMPADD(second, 6 * 3600, TIMESTAMPADD(DAY, 1, DATE(NOW())))
				//	//	"stock_id", Info.ID,
				//	//	"action", sign);
				//}
				//}

				//if (highestHighs.Count == 3)
				//{
				//	Spot firstSpot = highestHighs[2];
				//	Spot middleSpot = highestHighs[1];
				//	Spot lastSpot = highestHighs[0];

				//	float y = FindClose(firstSpot, middleSpot, lastSpot.Time);

				//	if (y > lastSpot.Close || lastSpot.Close - y < changeAvg)
				//	{
				//		// should buy
				//		//Data.Database.Execute("INSERT INTO analyze_results(stock_id, analyze_time, action, action_time) VALUES(@stock_id, NOW(), @action, NOW())", //TIMESTAMPADD(second, 6 * 3600, TIMESTAMPADD(DAY, 1, DATE(NOW())))
				//		//	"stock_id", Info.ID,
				//		//	"action", sign);
				//	}
				//}
			}

			private static void FindExtremums(DataTable Table, SpotList List, Func<int, int, int, bool> Comparison)
			{
				Spot prevSpot = null;
				for (int i = 1; i < Table.Rows.Count - 1; ++i)
				{
					DataRow prevRow = Table.Rows[i - 1];
					DataRow currRow = Table.Rows[i];
					DataRow nextRow = Table.Rows[i + 1];

					int prevClose = Convert.ToInt32(prevRow["close"]);
					int currClose = Convert.ToInt32(currRow["close"]);
					int nextClose = Convert.ToInt32(nextRow["close"]);

					if (!Comparison(prevClose, currClose, nextClose))
						continue;

					if (prevSpot != null && prevSpot.Close == currClose)
						continue;

					prevSpot = new Spot() { Index = i, Time = Convert.ToInt32(currRow["take_time"]), High = Convert.ToInt32(currRow["high"]), Low = Convert.ToInt32(currRow["low"]), Close = currClose };

					List.Add(prevSpot);
				}
			}

			private static void ValidateSpots(SpotList List, Func<float, bool> CheckSlope, Func<Spot, float, bool> CheckInfiltrate)
			{
				int startIndex = 0;
				int hitCount = 0;
				while (startIndex < List.Count - 2)
				{
					Spot firstSpot = List[startIndex++];
					Spot secondSpot = List[startIndex];

					float slope = CalculateSlope(firstSpot, secondSpot);
					if (!CheckSlope(slope))
					{
						hitCount = 0;

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
							lineBroke = true;

							break;
						}

						++hitCount;
					}

					if (lineBroke)
						continue;

					break;
				}

				if (hitCount != 0)
				{
					Console.WriteLine("{0} {1} {2}%", 1/*Info.ID*/, hitCount, ((hitCount + 2) / (float)List.Count) * 100);
				}
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