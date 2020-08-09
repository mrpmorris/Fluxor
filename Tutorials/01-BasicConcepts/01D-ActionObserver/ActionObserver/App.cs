using BasicConcepts.ActionObserver.Store.EditCustomerUseCase;
using Fluxor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BasicConcepts.ActionObserver
{
	public class App : IDisposable
	{
		private readonly IStore Store;
		public readonly IDispatcher Dispatcher;
		private readonly IActionSubscriber ActionSubscriber;

		public App(IStore store, IDispatcher dispatcher, IActionSubscriber actionSubscriber)
		{
			Store = store;
			Dispatcher = dispatcher;
			ActionSubscriber = actionSubscriber;
		}

		public void Run()
		{
			Console.Clear();
			Console.WriteLine("Initializing store");
			Store.InitializeAsync().Wait();
			Console.WriteLine($"Subscribing to action {nameof(GetCustomerForEditResultAction)}");
			ActionSubscriber.SubscribeToAction<GetCustomerForEditResultAction>(this, action => 
			{
				string jsonToShowInConsole = JsonConvert.SerializeObject(action.Customer, Formatting.Indented);
				Console.WriteLine("Action notification:");
				Console.WriteLine(jsonToShowInConsole);
			});
			string input = "";
			do
			{
				Console.WriteLine("1: Get mutable object from API server");
				Console.WriteLine("x: Exit");
				Console.Write("> ");
				input = Console.ReadLine();

				switch (input.ToLowerInvariant())
				{
					case "1":
						var getCustomerAction = new GetCustomerForEditAction(Guid.NewGuid());
						Dispatcher.Dispatch(getCustomerAction);
						break;

					case "x":
						Console.WriteLine("Program terminated");
						return;
				}
			} while (true);
		}

		void IDisposable.Dispose()
		{
			ActionSubscriber.UnsubscribeFromAllActions(this);
		}
	}

}
