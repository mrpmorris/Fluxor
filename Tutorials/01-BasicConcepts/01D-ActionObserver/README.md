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
	public Guid Id { get; private set; } = Guid.NewGuid();
	public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
	public string Name { get; set; }

	public CustomerEdit(Guid id, byte[] rowVersion, string name)
	{
		Id = id;
		RowVersion = rowVersion;
		Name = name;
	}
}
```

Create a `Store` folder, and in that folder create a folder named `EditCustomerUseCase`.





Descend the class from `Fluxor.Middleware`. We could instead implement `Fluxor.IMiddleware`, but the
`Middleware` class is recommended when possible as it enables us to override only the methods we wish
to implement.

Start with the following code, which will intercept when the store is first initialised, and then keep a
reference to the `IStore`.

```c#
public class LoggingMiddleware : Middleware
{
	private IStore Store;

	public override Task InitializeAsync(IStore store)
	{
		Store = store;
		Console.WriteLine(nameof(InitializeAsync));
		return Task.CompletedTask;
	}
}
```

#### Registering Middleware
In the `Program` class, look for the code that calls `services.AddFluxor`. After the call to
`ScanAssemblies` add a call to `AddMiddleware`.

```c#
services.AddFluxor(o => o
	.ScanAssemblies(typeof(Program).Assembly)
	.AddMiddleware<LoggingMiddleware>());
```

Run the application now, and we should see the following console output

```
Initializing store
InitializeAsync
```

#### Middleware lifecycle

1. **Task InitializeAsync(IStore store)**
- Executed when the store is first initialised. This gives us an opportunity to store away a reference to
the store that has been initialized.
2. **void AfterInitializeAllMiddlewares()**
- Once the store has been initialised, and `InitializeAsync` has been executed on all Middleware, this method
will be executed.
3. **bool MayDispatchAction(object action)**
- Every time `IDispatcher.Dispatch` is executed, the action dispatched will first be passed to every
Middleware in turn to give it the chance to veto the action. If the method returns `true` then the action will
be dispatched. The first Middleware to return `false` will terminate the dispatch process. An example of this
is the [ReduxDevToolsMiddleware.cs class][5] which prevents the dispatching of new actions when the user is
viewing a historical state.
4. **void BeforeDispatch(object action)**
- Once all Middlewares have approved, this method is called to inform us the action is about to be reduced
into state.
5. **void AfterDispatch(object action)**
- After the action has been processed by all reducers this method will be called.

#### Adding logging to our LoggingMiddleware

Add `Newtonsoft.Json` to the project, and then in the Middleware class add the following method to
serialize any object into JSON so it can be displayed in the console window.

```c#
private string ObjectInfo(object obj)
	=> ": " + obj.GetType().Name + " " + JsonConvert.SerializeObject(obj, Formatting.Indented);
```

With some additional `Console.WriteLine` calls in our methods, we end up with a `LoggingMiddleware` that
looks like this.

```c#
public class LoggingMiddleware : Middleware
{
	private IStore Store;

	public override Task InitializeAsync(IStore store)
	{
		Store = store;
		Console.WriteLine(nameof(InitializeAsync));
		return Task.CompletedTask;
	}

	public override void AfterInitializeAllMiddlewares()
	{
		Console.WriteLine(nameof(AfterInitializeAllMiddlewares));
	}

	public override bool MayDispatchAction(object action)
	{
		Console.WriteLine(nameof(MayDispatchAction) + ObjectInfo(action));
		return true;
	}

	public override void BeforeDispatch(object action)
	{
		Console.WriteLine(nameof(BeforeDispatch) + ObjectInfo(action));
	}

	public override void AfterDispatch(object action)
	{
		Console.WriteLine(nameof(AfterDispatch) + ObjectInfo(action));
		Console.WriteLine();
	}

	private string ObjectInfo(object obj)
		=> ": " + obj.GetType().Name + " " + JsonConvert.SerializeObject(obj, Formatting.Indented);
}
```

#### Running our Middleware

If we run our app now, our console output should look something like the following.
```
Initializing store
InitializeAsync
AfterInitializeAllMiddlewares
MayDispatchAction: StoreInitializedAction {}
BeforeDispatch: StoreInitializedAction {}
AfterDispatch: StoreInitializedAction {}

1: Increment counter
2: Fetch data
x: Exit
```

The first action dispatched when a store is initialised is the `StoreInitializedAction`. This is automatically
dispatched by the store and should always be the first action we see.

Type `1` and press Enter.

```
> 1
MayDispatchAction: IncrementCounterAction {}
BeforeDispatch: IncrementCounterAction {}
AfterDispatch: IncrementCounterAction {}

1: Increment counter
2: Fetch data
x: Exit
```

Now type `2` and press Enter. The output should look something like the following. Comments have been added
to explain what is happening.

```
> 2
*** The FetchDataAction is dispatched.
MayDispatchAction: FetchDataAction {}
BeforeDispatch: FetchDataAction {}
AfterDispatch: FetchDataAction {}

*** As there is an effect to fetch data that is triggered by FetchDataAction, the store triggers it here.

*** The dispatch of FetchDataAction is now complete, so the main app loop continues and redisplays the menu.
1: Increment counter
2: Fetch data
x: Exit

*** The effect receives data from the mock server, and dispatches the received data in a new FetchDataResultAction.

> MayDispatchAction: FetchDataResultAction {
  "Forecasts": [
    {
      "Date": "2020-03-22T10:39:14.6862635+00:00",
      "TemperatureC": 17,
      "Summary": "Hot",
      "TemperatureF": 62
    },
    {
      "Date": "2020-03-23T10:39:14.6862635+00:00",
      "TemperatureC": -12,
      "Summary": "Freezing",
      "TemperatureF": 11
    }
  ]
}
BeforeDispatch: FetchDataResultAction {
  "Forecasts": [
    ...(as above)...
  ]
}
AfterDispatch: FetchDataResultAction {
  "Forecasts": [
    ...(as above)...
  ]
}
```

#### Additional information
With some additional code in the `void AfterDispatch(object action)` we can interate through all the features
within the store and output their state too.

```c#
public override void AfterDispatch(object action)
{
	Console.WriteLine(nameof(AfterDispatch) + ObjectInfo(action));
	Console.WriteLine("\t===========STATE AFTER DISPATCH===========");
	foreach (KeyValuePair<string, IFeature> feature in Store.Features)
	{
		string json = JsonConvert.SerializeObject(feature.Value, Formatting.Indented)
			.Replace("\n", "\n\t");
		Console.WriteLine("\t" + feature.Key + ": " + json);
	}
	Console.WriteLine();
}
```

The output of which should look like the following:

```
Initializing store
InitializeAsync
AfterInitializeAllMiddlewares
MayDispatchAction: StoreInitializedAction {}
BeforeDispatch: StoreInitializedAction {}
AfterDispatch: StoreInitializedAction {}
        ===========STATE AFTER DISPATCH===========
        Weather: {
          "State": {
            "IsLoading": false,
            "Forecasts": []
          }
        }
        Counter: {
          "State": {
            "ClickCount": 0
          }
        }

1: Increment counter
2: Fetch data
x: Exit
> 1
MayDispatchAction: IncrementCounterAction {}
BeforeDispatch: IncrementCounterAction {}
AfterDispatch: IncrementCounterAction {}
        ===========STATE AFTER DISPATCH===========
        Weather: {
          "State": {
            "IsLoading": false,
            "Forecasts": []
          }
        }
        Counter: {
          "State": {
            "ClickCount": 1
          }
        }

1: Increment counter
2: Fetch data
x: Exit
>
```

### Real-life uses

**Task InitializeAsync(IStore store)**

[Redux Dev Tools Middleware class][5] uses this method to execute JavaScript to initialise the Chrome plugin.

**bool MayDispatch(object action)**

[Redux Dev Tools Middleware class][5] uses this method to ensure no new actions may be executed when the
user has used the Chrome plugin to navigate to a historical state. Once the user uses the plugin to navigate
back to the current state, the plugin will allow actions to be dispatched again.

**void AfterDispatch(object action)**

[Redux Dev Tools Middleware class][5] uses this method to notify the Chrome plugin that an
action has been dispatched, and also to send it the current state so the user can view the action/state
history.

  [1]: <https://github.com/mrpmorris/Fluxor/tree/master/Source/Fluxor.Blazor.Web/Middlewares/Routing>
  [2]: <https://github.com/mrpmorris/Fluxor/tree/master/Source/Fluxor.Blazor.Web.ReduxDevTools>
  [3]: <https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd>
  [4]: <https://github.com/mrpmorris/Fluxor/tree/master/Tutorials/01-BasicConcepts/01B-EffectsTutorial>
	[5]: <https://github.com/mrpmorris/Fluxor/blob/master/Source/Fluxor.Blazor.Web.ReduxDevTools/ReduxDevToolsMiddleware.cs>