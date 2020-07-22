using Core;

namespace Worker
{
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