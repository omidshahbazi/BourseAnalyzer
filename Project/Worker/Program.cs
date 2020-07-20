using Core;

namespace Worker
{
	class Program
	{
		private static void Main(string[] args)
		{
			Manager manager = new Manager();
			manager.Run();
		}
	}
}