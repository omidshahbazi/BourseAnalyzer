﻿using Core;

namespace Worker
{
	//TODO: add more info to each analyze like (percent in RSI)
	//TOOD: add more indicators
	//TODO: create client
	class Program
	{
		private static void Main(string[] args)
		{
			Manager manager = new Manager();
			manager.Run();
		}
	}
}