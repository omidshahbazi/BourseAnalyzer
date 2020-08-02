using System;
using System.Data;
using System.Diagnostics;

namespace Core
{
	public static partial class Analyzer
	{
		private static readonly Func<Indicator.Info, DataTable>[] Indicators = new Func<Indicator.Info, DataTable>[] { Indicator.MovingAverageConvergenceDivergence.Generate, Indicator.RelativeStrengthIndex.Generate };
		private static readonly Func<Indicator.Info, Result>[] Analyzers = new Func<Indicator.Info, Result>[] { RelativeStrengthIndex.Analyze, MovingAverageConvergenceDivergence.Analyze };

		public class Signal
		{
			public double Worthiness;
		}

		public class Result
		{
			public Signal[] Signals;
		}

		public class AnalyzeInfo
		{
			public double Worthiness;
		}

		public static AnalyzeInfo Analyze(Indicator.Info Info, int BacklogCount)
		{
			GenerateData(Info);

			Result[] results = new Analyzer.Result[Analyzers.Length];

			for (int i = 0; i < Analyzers.Length; ++i)
				results[i] = Analyzers[i](Info);

			double buyWorthiness = 0;
			float buySignalPower = 0;
			FindSignal(results, BacklogCount, 1, out buyWorthiness, out buySignalPower);

			double sellWorthiness = 0;
			float sellSignalPower = 0;
			FindSignal(results, BacklogCount, -1, out sellWorthiness, out sellSignalPower);

			Debug.Assert(buySignalPower == 0 || buySignalPower != sellSignalPower);

			double worthiness = 0;

			if (buySignalPower > sellSignalPower)
				worthiness = buyWorthiness;
			else
				worthiness = sellWorthiness;

			return new AnalyzeInfo() { Worthiness = worthiness };
		}

		private static void GenerateData(Indicator.Info Info)
		{
			DataTable historyData = Info.HistoryData;

			for (int i = 0; i < Indicators.Length; ++i)
			{
				DataTable indicatorData = Indicators[i](Info);
				if (indicatorData == null)
					continue;

				int startIndex = historyData.Rows.Count - indicatorData.Rows.Count;

				for (int j = 0; j < indicatorData.Columns.Count; ++j)
				{
					DataColumn column = indicatorData.Columns[j];

					historyData.Columns.Add(column.ColumnName, column.DataType);

					for (int l = 0; l < indicatorData.Rows.Count; ++l)
						historyData.Rows[startIndex + l][column.ColumnName] = indicatorData.Rows[l][column.ColumnName];
				}
			}
		}

		private static void FindSignal(Result[] Results, int BacklogCount, int Action, out double Worthiness, out float SignalPower)
		{
			Worthiness = 0;
			SignalPower = 0;

			int confirmedSignalCount = 0;

			int lastSingalIndex = BacklogCount - 1;

			int[] signalIndex = new int[Results.Length];
			for (int i = 0; i < signalIndex.Length; ++i)
				signalIndex[i] = -1;

			for (int i = 0; i < Results.Length; ++i)
			{
				if (Results[i] == null || signalIndex[i] != -1)
					continue;

				Signal signal = Results[i].Signals[lastSingalIndex];

				int action = Math.Sign(signal.Worthiness);

				if (action == 0)
					continue;

				signalIndex[i] = i;

				if (action != Action)
					continue;

				Worthiness += signal.Worthiness;
				SignalPower += 1;

				for (int j = 0; j < Results.Length; ++j)
				{
					if (i == j)
						continue;

					if (Results[j] == null || signalIndex[j] != -1)
						continue;

					for (int l = lastSingalIndex; l > -1; --l)
					{
						Signal refSignal = Results[j].Signals[l];

						int refAction = Math.Sign(refSignal.Worthiness);

						if (refAction == 0)
							continue;

						signalIndex[j] = j;

						if (refAction != Action)
							break;

						Worthiness += refSignal.Worthiness;
						++confirmedSignalCount;
						SignalPower += (l + 1) / (float)BacklogCount;

						break;
					}
				}
			}

			if (confirmedSignalCount < ConfigManager.Config.DataAnalyzer.SignalConfirmationCount)
			{
				SignalPower = 0;
				return;
			}

			Worthiness /= (confirmedSignalCount + 1);
			SignalPower /= Results.Length;
		}

		private static bool CheckCrossover(double AveragePrevious, double AverageCurrent, double SignalPrevious, double SignalCurrent, out int Direction)
		{
			Direction = 0;

			if ((AveragePrevious <= SignalPrevious && AverageCurrent > SignalCurrent) ||
				(AveragePrevious < SignalPrevious && AverageCurrent >= SignalCurrent))
			{
				Direction = 1;
			}
			else if ((AveragePrevious >= SignalPrevious && AverageCurrent < SignalCurrent) ||
					 (AveragePrevious > SignalPrevious && AverageCurrent <= SignalCurrent))
			{
				Direction = -1;
			}
			else
				return false;

			return true;
		}

		private static bool CheckPointCrossover(double AveragePrevious, double AverageCurrent, double SignalPoint, out int Direction)
		{
			Direction = 0;

			if ((AveragePrevious <= SignalPoint && SignalPoint < AverageCurrent) ||
				(AveragePrevious < SignalPoint && SignalPoint <= AverageCurrent))
			{
				Direction = 1;
			}
			else if ((AveragePrevious >= SignalPoint && SignalPoint > AverageCurrent) ||
					  (AveragePrevious > SignalPoint && SignalPoint >= AverageCurrent))
			{
				Direction = -1;
			}
			else
				return false;

			return true;
		}
	}
}