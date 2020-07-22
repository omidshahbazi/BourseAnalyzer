using Core;

namespace Worker
{
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