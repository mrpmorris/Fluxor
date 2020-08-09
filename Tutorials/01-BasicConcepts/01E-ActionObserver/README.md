# Fluxor - Basic concepts

## IActionObserver
`IActionObserver` allows us to subscribe to the dispatch pipeline and be notifified
whenever an action has been dispatched.

One particularly useful example of this is when we wish to retrieve
a mutable object from an API server that we can edit in our application
without having to store that mutable object in our immutable state.

### Goal
This tutorial will demonstrate how to subscribe to be notified whenever
a specific action is dispatched. When the subscriber is notified, a JSON
representation of the action will be displayed in the console.

### Steps

Create a folder named `ApiObjects` and then add the following class. This class would normally be
in a separate assembly that is shared between the client and the server.

```C#
public class CustomerEdit
{
	public int Id { get; private set; } = 42;
	public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
	public string Name { get; set; }

	public CustomerEdit(int id, byte[] rowVersion, string name)
	{
		Id = id;
		RowVersion = rowVersion;
		Name = name;
	}
}
```

Create a `Store` folder, and in that folder create a folder named `EditCustomerUseCase`. Within
that folder we need the standard classes to define our feature, our state, and the
actions/reducers/effects that we need to emulate retrieving a mutable object from an API service.

```C#
public class EditCustomerState
{
	public bool IsLoading { get; private set; }

	public EditCustomerState(bool isLoading)
	{
		IsLoading = isLoading;
	}
}

public class Feature : Feature<EditCustomerState>
{
	public override string GetName() => "EditCustomer";

	protected override EditCustomerState GetInitialState() =>
		new EditCustomerState(isLoading: false);
}

public class GetCustomerForEditAction
{
	public int Id { get; }

	public GetCustomerForEditAction(int id)
	{
		Id = id;
	}
}

public class GetCustomerForEditResultAction
{
	public CustomerEdit Customer { get; }

	public GetCustomerForEditResultAction(CustomerEdit customer)
	{
		Customer = customer;
	}
}

public static class Reducers
{
	[ReducerMethod]
	public static EditCustomerState Reduce(EditCustomerState state, GetCustomerForEditAction action) =>
		new EditCustomerState(isLoading: true);

	[ReducerMethod]
	public static EditCustomerState Reduce(EditCustomerState state, GetCustomerForEditResultAction action) =>
		new EditCustomerState(isLoading: false);
}

public class Effects
{
	[EffectMethod]
	public async Task HandleGetCustomerForEditAction(GetCustomerForEditAction action, IDispatcher dispatcher)
	{
		Console.WriteLine("Getting customer with Id: 42");

		await Task.Delay(1000);

		string jsonFromServer = $"{{\"Id\":42,\"RowVersion\":\"AQIDBAUGBwgJCgsMDQ4PEA==\",\"Name\":\"Our first customer\"}}";
		var objectFromServer = JsonConvert.DeserializeObject<CustomerEdit>(jsonFromServer);
		dispatcher.Dispatch(new GetCustomerForEditResultAction(objectFromServer));
	}
}
```

#### Create an App to test our store

```C#
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
		SubscribeToResultAction();
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
					var getCustomerAction = new GetCustomerForEditAction(42);
					Dispatcher.Dispatch(getCustomerAction);
					break;

				case "x":
					Console.WriteLine("Program terminated");
					return;
			}
		} while (true);
	}

	private void SubscribeToResultAction()
	{
		// We'll implement this shortly
	}


	void IDisposable.Dispose()
	{
		// We'll implement this shortly
	}
}
```

#### Bootstrapping our app
Replace the `Main` method in `Program.cs` to execute our test app.

```C#
static void Main(string[] args)
{
	var services = new ServiceCollection();
	services.AddScoped<App>();
	services.AddFluxor(o => o
		.ScanAssemblies(typeof(Program).Assembly));

	IServiceProvider serviceProvider = services.BuildServiceProvider();

	var app = serviceProvider.GetRequiredService<App>();
	app.Run();
}
```

#### Subscribing to actions
Edit the `App` class and implement the following two methods:

```C#
private void SubscribeToResultAction()
{
	Console.WriteLine($"Subscribing to action {nameof(GetCustomerForEditResultAction)}");
	ActionSubscriber.SubscribeToAction<GetCustomerForEditResultAction>(this, action =>
	{
		// Show the object from the server in the console
		string jsonToShowInConsole = JsonConvert.SerializeObject(action.Customer, Formatting.Indented);
		Console.WriteLine("Action notification: " + action.GetType().Name);
		Console.WriteLine(jsonToShowInConsole);
	});
}

void IDisposable.Dispose()
{
	// IMPORTANT: Unsubscribe to avoid memory leaks!
	ActionSubscriber.UnsubscribeFromAllActions(this);
}
```

### Running our app

If we run our app now, and select option 1 (and press enter), our
console output should look something like this.

```
> Action notification: GetCustomerForEditResultAction
{
  "Id": 42,
  "RowVersion": "AQIDBAUGBwgJCgsMDQ4PEA==",
  "Name": "Our first customer"
}
```