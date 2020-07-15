using System;

namespace Core
{
	public abstract class Worker
	{
		private DateTime nextUpdateTime = DateTime.MinValue;

		protected abstract float WorkHour
		{
			get;
		}

		public Worker()
		{
			nextUpdateTime = DateTime.Now.Date;

			//if (DateTime.Now.TimeOfDay.TotalHours > WorkHour)
			//	nextUpdateTime = nextUpdateTime.AddDays(1);

			//nextUpdateTime = nextUpdateTime.AddHours(WorkHour);
		}

		public void Update()
		{
			if (DateTime.Now < nextUpdateTime)
				return;

			if (!Do())
				return;

			nextUpdateTime = nextUpdateTime.AddDays(1);
		}

		protected abstract bool Do();
	}
}