using System.Threading;

namespace Core
{
	public class Manager
	{
		private Worker[] workers = null;

		public Manager()
		{
			workers = new Worker[] { new DataUpdater(), new DataAnalyzer() };
		}

		public void Run()
		{
			while (true)
			{
				for (int i = 0; i < workers.Length; ++i)
					workers[i].Update();

				Thread.Sleep(86400);
			}
		}
	}
}
