using Fluxor;
using BasicConcepts.StateActionsReducersTutorial.Store;
using BasicConcepts.StateActionsReducersTutorial.Store.CounterUseCase;
using System;
using System.Threading.Tasks;

namespace BasicConcepts.StateActionsReducersTutorial
{
	public class App
	{
		private readonly IStore Store;
		public readonly IDispatcher Dispatcher;
		public readonly IState<CounterState> CounterState;

		public App(IStore store, IDispatcher dispatcher, IState<CounterState> counterState)
		{
			Store = store;
			Dispatcher = dispatcher;
			CounterState = counterState;
			CounterState.StateChanged += CounterState_StateChanged;
		}

		private void CounterState_StateChanged(object sender, EventArgs e)
		{
			Console.WriteLine("");
			Console.WriteLine("==========================> CounterState");
			Console.WriteLine("ClickCount is " + CounterState.Value.ClickCount);
			Console.WriteLine("<========================== CounterState");
			Console.WriteLine("");
		}

		public async Task RunAsync()
		{
			Console.Clear();
			Console.WriteLine("Initializing store");
			await Store.InitializeAsync();
			string input;
			do
			{
				Console.WriteLine("1: Increment counter");
				Console.WriteLine("x: Exit");
				Console.Write("> ");
				input = Console.ReadLine();

				switch (input.ToLowerInvariant())
				{
					case "1":
						var action = new IncrementCounterAction();
						await Dispatcher.DispatchAsync(action);
						break;

					case "x":
						Console.WriteLine("Program terminated");
						return;
				}

			} while (true);
		}
	}
}
