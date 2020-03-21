# Fluxor - Blazor Web Tutorials

## State, Actions, and Reducers

### Goal
This tutorial will recreate the `Counter` page in a standard Blazor app.

### Steps

#### Add Fluxor.Blazor.Web and initialize
- First create a new Blazor app, either a Server app or Web Assembly app will do.

- Add a NuGet package reference to `Fluxor.Blazor.Web`.

- Edit either `Pages\_Host.cshtml` (Server-side) or `wwwroot\index.html` (Web Assembly) and
just above the `</body>` tag add the following:

```html
<script src="_content/Fluxor.Blazor.Web/scripts/index.js"></script>
```

- We now need to register Fluxor. Find where the app registers its services
(in a Server-side app this will be in `Startup.ConfigureServices`, in a Web Assembly app this will be
in `Program.Main`).

```c#
// Server-side
using Fluxor;

public void ConfigureServices(IServiceCollection services)
{
	...

	// Add the following
	var currentAssembly = typeof(Startup).Assembly;
	services.AddFluxor(options => options.ScanAssemblies(currentAssembly));
}
```

```c#
// Web Assembly
using Fluxor;

public static async Task Main(string[] args)
{
	...

	// Add the following
	var currentAssembly = typeof(Program).Assembly;
	builder.Services.AddFluxor(options => options.ScanAssemblies(currentAssembly));

	// This line already exists and should come last
	await builder.Build().RunAsync();
}
```

- Then we need to ensure the store is initialized. Edit `App.razor` and at the top of the file add
the following mark-up.

```html
<Fluxor.Blazor.Web.StoreInitializer/>
```

#### Adding the Counter to the store

Note, the folder structure used here is only a recommendation.
- Create a folder named `Store`.
- Within that folder create another folder named `CounterUseCase`.
 
*Note: I recommend building your store and your application around use
cases (e.g. FindSupplier, EditSupplier, etc).*

- Within the `CounterUseCase` folder create a new class named `CounterState`. This is the class that
will hold the values of your state to be displayed in your application.

```c#
public class CounterState
{
	public int ClickCount { get; }

	public CounterState(int clickCount)
	{
		ClickCount = clickCount;
	}
}
```

*Note: State should be immutable*

- Create a new class named `Feature`. This class describes the state to the store.

```c#
public class Feature : Feature<CounterState>
{
	public override string GetName() => "Counter";
	protected override CounterState GetInitialState() =>
		new CounterState(clickCount: 0);
}
```

#### Displaying state in a component

*Note: C# code can be added to the `@code {}` block within Razor files, but I prefer to add a
code-behind file.*

- Find the `Pages` folder and add a new file named `Counter.razor.cs`
- Mark the class `partial`.
- Add the following `using` declarations

```c#
using Fluxor;
using Microsoft.AspNetCore.Components;
using YourAppName.Store.CounterUseCase;
```

- Next we need to inject the `CounterState` into our component

```c#
public partial class Counter
{
	[Inject]
	private IState<CounterState> CounterState { get; set; }
}
```

- Edit `Counter.razor` and change `currentCount` to `@CounterState.Value.ClickCount`.

```html
<p>Current count: @CounterState.Value.ClickCount</p>
```

Also, add the following line to the top of the razor file
```
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
```

*Note: This is required to ensure the component re-renders whenever its state changes. If you are unable
to descend from this component, you can instead subcribe to the `IState.StateChanged` event and execute
`InvokeAsync(StateHasChanged)`. If you do use the event, remember to implement `IDisposable` and
unsubscribe from the event too, otherwise your app will leak memory.*

Running the app will now show a `0` value fo the current count, but clicking the "Click me" button does nothing.

#### Using an Action and a Reducer to alter state

- In `Store\CounterUseCase` create a new class `IncrementCounterAction`. This class can remain empty.
- Edit `Counter.Razor` and remove the `@code {}` section.
- In `Counter.razor.cs` we need to inject `IDispatcher` and then use it to dispatch an instance
of our new `IncrementCounterAction` when the button is clicked.

```c#
public partial class Counter
{
	[Inject]
	private IState<CounterState> CounterState { get; set; }

	[Inject]
	public IDispatcher Dispatcher { get; set; }

	private void IncrementCount()
	{
		var action = new IncrementCounterAction();
		Dispatcher.Dispatch(action);
	}
}
```

*Note: `IncrementCount` is executed when the `<button>` is clicked. Look in `Counter.razor` to see how
this is done.*

Now our UI is dispatching our intention to increment the counter, but the state remains unchanged because
we do not handle this action. We will fix that next.

- In the `Store\CounterUseCase` folder, create a new class `Reducers`.
- Make the class static, and add the following code.

```c#
public static class Reducers
{
	[ReducerMethod]
	public static CounterState ReduceIncrementCounterAction(CounterState state, IncrementCounterAction action) =>
		new CounterState(clickCount: state.ClickCount + 1);
}
```

Running the app will now work as expected and increment the counter whenever the button is clicked.

#### Tips

##### Splitting reducers across classes

You may have as many reducer classes as you wish, the `Reducer` class name used in this tutorial is
not a special tag or anything. If the method is decorated with `[ReducerMethod]` and has the correct
signature, it will be used.

```c#
public static class SomeReducerClass
{
	[ReducerMethod]
	public static SomeState(SomeState state, SomeAction action) => new SomeState();

	[ReducerMethod]
	public static SomeState(SomeState state, SomeAction2 action) => new SomeState();
}

public static class SomeOtherReducerClass
{
	[ReducerMethod]
	public static SomeState(SomeState state, SomeAction3 action) => new SomeState();

	[ReducerMethod]
	public static SomeState(SomeState state, SomeAction4 action) => new SomeState();
}
```

##### Injecting dependencies

**Tip: Do not inject state into reducers!**

It is possible to decorate instance methods with `[ReducerMethod]`.
Any dependencies in the constructor will be injected automatically.

I strongly recommend the use of static methods. Reducers should ideally be
[pure functions](https://en.wikipedia.org/wiki/Pure_function),
if you find yourself needing to inject dependencies into a reducer then you might be
taking the wrong approach.

##### The `Reducer<TState, TAction>` class

It is also possible to create a reducer per state+action combination like this...

```c#
public class IncrementCounterReducer : Reducer<CounterState, IncrementCounterAction>
{
	public override CounterState Reduce(CounterState state, IncrementCounterAction action) =>
		new CounterState(clickCount: state.ClickCount + 1);
}
```

This pattern use is not recommended.