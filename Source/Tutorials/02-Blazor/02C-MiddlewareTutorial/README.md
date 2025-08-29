# Fluxor - Basic concepts

## Middleware
Middleware allows us to hook into various execution points in the Store's lifetime. These allow us
to perform additional actions across the store regardless of which action is dispatched, and regardless
of which states are affected by that action.

Two examples of Middleware in Fluxor are
1. [Routing Middleware][1] for Blazor Web UI, to ensure navigation events have a corresponding action
that reducers can hook into.
2. [Redux Dev Tools Middleware][2] for Blazor Web UI, which integrates with the
[Redux Dev Tools][3] plugin for Google Chrome.

### Goal
This tutorial will demonstrate how to create a Middleware plugins for Fluxor that will log to the console
whenever a Middleware hook point is called, and to output the current state after each action has been
reduced into state.

### Steps
The quickest way to get started is to build upon the project we built in the first two parts of these
tutorials. The source code is available [here][4] if you wish to skip manually creating the starting
project for this tutorial. But before continuing, remove any `Console.WriteLine` statements so as not
to clutter the console output too much. Leave in the calls to `Console.WriteLine` that correspond to displaying
user options, and the line `Console.WriteLine("Initializing store");`

In the `Store` folder, create a file `Middlewares\Logging\LoggingMiddleware.cs`

Descend the class from `Fluxor.Middleware`. We could instead implement `Fluxor.IMiddleware`, but the
`Middleware` class is recommended when possible as it enables us to override only the methods we wish
to implement.

Start with the following code, which will intercept when the store is first initialised, and then keep a
reference to the `IStore`.

```c#
public class LoggingMiddleware : Middleware
{
  public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
  {
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
InitializeAsync
```

#### Middleware lifecycle

1. **Task InitializeAsync(IDispatcher dispatcher, IStore store)**
- Executed when the store is first initialised. This gives us an opportunity to store
  away a reference to the store that has been initialized and/or the `IDispatcher`.

2. **void AfterInitializeAllMiddlewares()**
- Once the store has been initialised, and `InitializeAsync` has been executed on all
  Middleware, this method will be executed.

3. **bool MayDispatchAction(object action)**
- Every time `IDispatcher.Dispatch` is executed, the action dispatched will first be passed
  to every Middleware in turn to give it the chance to veto the action. If the method
  returns `true` then the action will be dispatched. The first Middleware to return `false`
  will terminate the dispatch process. An example of this is the
  [ReduxDevToolsMiddleware.cs class][5] which prevents the dispatching of new actions
  when the user is viewing a historical state.

4. **void BeforeDispatch(object action)**
- Once all Middlewares have approved, this method is called to inform us the action is about
  to be reduced into state.

5. **void AfterDispatch(object action)**
- After the action has been processed by all reducers this method will be called.

#### Adding logging to our LoggingMiddleware

Override the base virtual methods and add some additional `Console.WriteLine` calls, and we
end up with a `LoggingMiddleware` that looks like this.

```c#
public class LoggingMiddleware : Middleware
{
  public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
  {
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
    => ": " + obj.GetType().Name + " " + JsonSerializer.Serialize(obj);
}
```

#### Running our Middleware

If we run our app now, our browser's console output should look something like the following.
```
InitializeAsync
AfterInitializeAllMiddlewares
MayDispatchAction: StoreInitializedAction {}
BeforeDispatch: StoreInitializedAction {}
AfterDispatch: StoreInitializedAction {}
```

The first action dispatched when a store is initialised is the `StoreInitializedAction`. This is automatically
dispatched by the store and should always be the first action we see.

Navigate to the Counter page and click the button to increment the counter, and the following
will be added to the console output.

```
MayDispatchAction: IncrementCounterAction {}
BeforeDispatch: IncrementCounterAction {}
AfterDispatch: IncrementCounterAction {}
```

Now navigate to the Fetch Data page, the output should look something like the following. Comments
have been added to explain what is happening.

```
*** The FetchForecastsAction is dispatched.
MayDispatchAction: FetchForecastsAction {}
BeforeDispatch: FetchForecastsAction {}
AfterDispatch: FetchForecastsAction {}

*** As there is an effect to fetch data that is triggered by FetchForecastsAction, the store triggers it here but does not wait for it to complete.

*** The dispatch of FetchForecastsAction is now complete.

*** Later, the effect receives data from the mock server and dispatches that data in a new FetchForecastsResultAction.

MayDispatchAction: FetchForecastsResultAction {
  "Forecasts": [...data...]
}
BeforeDispatch: FetchForecastsResultAction {
  "Forecasts": [...data...]
}
AfterDispatch: FetchForecastsResultAction {
  "Forecasts": [...data...]
}
```

#### Additional information
With some additional code in the `void AfterDispatch(object action)` we can interate through all the features
within the store and output their state too.

First, keep a reference to the `IStore` passed in via `InitializeAsync`.

```c#
  private IStore Store = null!;
   private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

  public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
  {
    Store = store;
    Console.WriteLine(nameof(InitializeAsync));
    return Task.CompletedTask;
  }
```


```c#
  public override void AfterDispatch(object action)
  {
    Console.WriteLine(nameof(AfterDispatch) + ObjectInfo(action));
    Console.WriteLine("\t===========STATE AFTER DISPATCH===========");
    foreach (KeyValuePair<string, IFeature> feature in Store.Features)
    {
      string json = JsonSerializer.Serialize(feature.Value, JsonOptions)
        .Replace("\n", "\n\t");
      Console.WriteLine("\t" + feature.Key + ": " + json);
    }
    Console.WriteLine();
  }
```

The output of which should look like the following:

```
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

*** Increment counter

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
```

### Real-life uses

**Task InitializeAsync(IDispatcher dispatcher, IStore store)**

[Redux Dev Tools Middleware class][5] uses this method to execute JavaScript to initialise the Chrome plugin.

**bool MayDispatch(object action)**

[Redux Dev Tools Middleware class][5] uses this method to ensure no new actions may be executed when the
user has used the Chrome plugin to navigate to a historical state. Once the user uses the plugin to navigate
back to the current state, the plugin will allow actions to be dispatched again.

**void AfterDispatch(object action)**

[Redux Dev Tools Middleware class][5] uses this method to notify the Chrome plugin that an
action has been dispatched, and also to send it the current state so the user can view the action/state
history.

  [1]: <https://github.com/mrpmorris/Fluxor/tree/master/Source/Lib/Fluxor.Blazor.Web/Middlewares/Routing>
  [2]: <https://github.com/mrpmorris/Fluxor/tree/master/Source/Lib/Fluxor.Blazor.Web.ReduxDevTools>
  [3]: <https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd>
  [4]: <https://github.com/mrpmorris/Fluxor/tree/master/Source/Tutorials/02-Blazor/02B-EffectsTutorial>
  [5]: <https://github.com/mrpmorris/Fluxor/blob/master/Source/Lib/Fluxor.Blazor.Web.ReduxDevTools/ReduxDevToolsMiddleware.cs>
