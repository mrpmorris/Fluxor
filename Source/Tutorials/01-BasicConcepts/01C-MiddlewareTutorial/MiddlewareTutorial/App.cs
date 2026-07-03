using Fluxor;
using System;
using System.Threading.Tasks;
using BasicConcepts.MiddlewareTutorial.Store;

namespace BasicConcepts.MiddlewareTutorial
{
	public class App
	{
		private readonly IStore Store;
		public readonly IDispatcher Dispatcher;

		public App(
			IStore store,
			IDispatcher dispatcher)
		{
			Store = store;
			Dispatcher = dispatcher;
		}

		public async Task RunAsync()
		{
			Console.Clear();
			Console.WriteLine("Initializing store");
			await Store.InitializeAsync();
			string input = "";
			do
			{
				Console.WriteLine("1: Increment counter");
				Console.WriteLine("2: Fetch data");
				Console.WriteLine("x: Exit");
				Console.Write("> ");
				input = Console.ReadLine();

				switch(input.ToLowerInvariant())
				{
					case "1":
						var incrementCounterActionction = new IncrementCounterAction();
						await Dispatcher.DispatchAsync(incrementCounterActionction);
						break;

					case "2":
						var fetchDataAction = new FetchDataAction();
						await Dispatcher.DispatchAsync(fetchDataAction);
						break;

					case "x":
						Console.WriteLine("Program terminated");
						return;
				}

			} while (true);
		}
	}
}
