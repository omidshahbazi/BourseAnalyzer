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

				int changeSum = 0;
				for (int i = 0; i < Info.HistoryData.Rows.Count; i++)
				{
					DataRow currRow = Info.HistoryData.Rows[i];

					changeSum += Math.Abs(Convert.ToInt32(currRow["close"]) - Convert.ToInt32(currRow["open"]));
				}

				float changeAvg = changeSum / (float)Info.HistoryData.Rows.Count;

				Spot prevSpot = null;
				SpotList lowestLows = new SpotList();
				for (int i = 1; i < Info.HistoryData.Rows.Count - 1; ++i)
				{
					DataRow prevRow = Info.HistoryData.Rows[i - 1];
					DataRow currRow = Info.HistoryData.Rows[i];
					DataRow nextRow = Info.HistoryData.Rows[i + 1];

					int prevClose = Convert.ToInt32(prevRow["close"]);
					int currClose = Convert.ToInt32(currRow["close"]);
					int nextClose = Convert.ToInt32(nextRow["close"]);

					if (currClose > prevClose || nextClose < currClose)
						continue;

					if (prevSpot != null && prevSpot.Close == currClose)
						continue;

					prevSpot = new Spot() { Index = i, Time = Convert.ToInt32(currRow["take_time"]), Close = currClose };

					lowestLows.Add(prevSpot);
				}

				SpotList highestHighs = new SpotList();
				for (int i = 1; i < Info.HistoryData.Rows.Count - 1; ++i)
				{
					DataRow prevRow = Info.HistoryData.Rows[i - 1];
					DataRow currRow = Info.HistoryData.Rows[i];
					DataRow nextRow = Info.HistoryData.Rows[i + 1];

					int prevClose = Convert.ToInt32(prevRow["close"]);
					int currClose = Convert.ToInt32(currRow["close"]);
					int nextClose = Convert.ToInt32(nextRow["close"]);

					if (prevClose > currClose || currClose < nextClose)
						continue;

					if (prevSpot != null && prevSpot.Close == currClose)
						continue;

					prevSpot = new Spot() { Index = i, Time = Convert.ToInt32(currRow["take_time"]), Close = currClose };

					highestHighs.Add(prevSpot);

					if (highestHighs.Count == 3)
						break;
				}

				int startIndex = 0;
				int hitCount = 0;
				while (startIndex < lowestLows.Count - 2)
				{
					Spot firstSpot = lowestLows[startIndex++];
					Spot secondSpot = lowestLows[startIndex];

					float slope = CalculateSlope(firstSpot, secondSpot);
					if (slope < 0)
					{
						hitCount = 0;

						continue;
					}

					bool lineBroke = false;
					for (int i = startIndex + 1; i < lowestLows.Count; ++i)
					{
						Spot spot = lowestLows[i];

						float y = CalculateClose(firstSpot, slope, spot.Time);

						if (y > spot.Close && y - spot.Close > changeAvg)
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
					Console.WriteLine("{0} {1} {2}%", Info.ID, hitCount, ((hitCount + 2) / (float)lowestLows.Count) * 100);
				}

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

			public static void MakePowerTrendline(DataTable Data)
			{
				double[] sums = new double[4];

				for (int i = 0; i < Data.Rows.Count; ++i)
				{
					DataRow row = Data.Rows[i];

					double logX = Math.Log(Convert.ToInt32(row["take_time"]));
					double logY = Math.Log(Convert.ToInt32(row["close"]));

					sums[0] += logX;
					sums[1] += logY;
					sums[2] += logX * logY;
					sums[3] += logX * logX;
				}

				double b = (Data.Rows.Count * sums[2] - sums[0] * sums[1]) / (Data.Rows.Count * sums[3] - (sums[0] * sums[0]));
				double a = Math.Pow(Math.E, (sums[1] - b * sums[0]) / Data.Rows.Count);

				for (int i = 0; i < Data.Rows.Count; ++i)
				{
					DataRow row = Data.Rows[i];

					int x = Convert.ToInt32(row["take_time"]);
					int y = Convert.ToInt32(row["close"]);

					row["close"] = a * (x * x);
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