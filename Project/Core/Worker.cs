using System;

namespace Core
{
	public abstract class Worker
	{
		public abstract float WorkHour
		{
			get;
		}

		public abstract bool Do(DateTime CurrentDateTime);
	}
}