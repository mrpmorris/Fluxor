using Fluxor;
using FluxorConcepts.MiddlewareSample.Client.Store.WeatherUseCase;
using FluxorConcepts.MiddlewareSample.Shared;
using FluxorConcepts.MiddlewareSample.Store.CounterUseCase;
using System;
using System.Linq;

namespace FluxorConcepts.MiddlewareSample
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

		public void Run()
		{
			Console.Clear();
			Console.WriteLine("Initializing store");
			Store.InitializeAsync().Wait();
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
						Dispatcher.Dispatch(incrementCounterActionction);
						break;

					case "2":
						var fetchDataAction = new FetchDataAction();
						Dispatcher.Dispatch(fetchDataAction);
						break;

					case "x":
						Console.WriteLine("Program terminated");
						return;
				}

			} while (true);
		}
	}
}
