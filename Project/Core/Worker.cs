using System;

namespace Core
{
	public abstract class Worker
	{
		public abstract bool Enabled
		{
			get;
		}

		public abstract float WorkHour
		{
			get;
		}

		public abstract bool Do(DateTime CurrentDateTime);
	}
}