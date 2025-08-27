# Fluxor - Blazor Web Tutorials

## State, Actions, and Reducers

First read the [Basic-concepts - State, Actions, and Reducers](../../01-BasicConcepts/01A-StateActionsReducersTutorial/) section for a user-interface-agnostic
explanation of how these concepts work together. This tutorial will explain how to adjust those concepts
to work with the Blazor Web user-interface.

### Goal
This tutorial will recreate the `Counter` page in a standard Blazor app.

### Steps

#### Add Fluxor.Blazor.Web and initialise
.Net has different templates for Blazor applications. Some will create a
single project (Standalone Wasm, Interactive Server) and others will create
a pair of projects (one for the Client and one for the Server).

When there is a `{YourAppName}.Client.csproj` project you should perform these
steps on that project. When there is only `{YourAppName}.csproj` you should
perform them on that app instead.

- First create a new Blazor app.
- Add a NuGet package reference `Fluxor.Blazor.Web`
- Register Fluxor
  - Open `Program.cs`
  - Add `using Fluxor;` to the top of the file
  - Find the line of code that creates `builder` and add the following

```c#
builder.Services.AddFluxor(x =>
    x.ScanAssemblies(typeof({SomeType}).Assembly));
```

If your states + actions etc are in your Blazor app then you can
use `Program` for `{SomeType}`, whereas if they are in a different project
then it should be replaced with a type from that project instead.

*Blazor tip: Because of the way Blazor works, if your app has both `{App}.csproj`
and `{App}.Client.csproj` projects then you need to call `Services.AddFluxor` to
the `Program.cs` files in both. I would recommend having a static method in your
{App}.Client.csproj for registering shared services like Fluxor and calling it from
both the Server and Client `Program.cs`.*

- Then we need to ensure the store is initialized. Add the following markup 
      above the `<Router>` component.

  1. Blazor Wasm Standalone App: Put it in `App.razor`
  1. For all other apps, in the `Routes.razor` file
  

```html
<Fluxor.Blazor.Web.StoreInitializer/>
```

#### Adding the Counter to the store
You can achieve this by putting the store into the same app, or by creating
a class library with a reference to the `Fluxor` NuGet package and reference
that from your Blazor app (recommended). I have chosen to create a folder
named `Store` in the same project just to keep this tutorial more simple.

- Create a folder named `CounterFeature`.
- Within the `CounterFeature` folder create a new class named `CounterState`. This is the class that
will hold the values of your state to be displayed in your application.

```c#
[FeatureState]
public record CounterState(int ClickCount)
{
    // Required for creating initial state
    public CounterState() : this(0)
    {
    }
}
```

Or you can use a non-record class
```c#
[FeatureState]
public class CounterState
{
  public int ClickCount { get; }

  // Required for creating initial state
  private CounterState() {}

  public CounterState(int clickCount)
  {
    ClickCount = clickCount;
  }
}
```

- _Notes:_
  - State should be decorated with `[FeatureState]` for automatic discovery
    when `services.AddFluxor` is called. 
  - State should be immutable.
  - A parameterless constructor is required on state for determining the initial state,
    and can be private or public.
  - The folder structure used here is only a recommendation.
  - I recommend building your store and your application around application
    features cases (e.g. FindSupplier, EditSupplier, etc) rather than a single monolith state.

#### Displaying state in a component

- Find the `Counter.razor` file.
- Add the following `using` declarations

```razor
@using Fluxor
@using {Namespace for your CounterFeature folder}
```

If you don't want to have to keep adding references to `Fluxor`, edit the
`_Imports.razor` file and add `@using Fluxor`.

- Next we need to inject the `CounterState` into our `Counter` page.

```razor
@inject IState<CounterState> CounterState
```

- Next change `currentCount` to `@CounterState.Value.ClickCount`.

```html
<p>Current count: @CounterState.Value.ClickCount</p>
```

Also, add the following line to the top of the razor file
```
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
```

*Note: FluxorComponent and FluxorLayout override OnInitialized in order to subscribe
to state changes. If you override `OnInitialized` or `OnInitializedAsync` please
remember to call `base`.*

*Note: If you are unable to descend from this component, you can instead subcribe to
the `StateChanged` event on `IState<T>` and execute `InvokeAsync(StateHasChanged)`.
If you do use the event, remember to implement `IDisposable` and
unsubscribe from the event too, otherwise your app will leak memory.*

Running the app will now show a `0` value for the current count, but clicking the "Click me" button does nothing.

#### Using an Action and a Reducer to alter state

- Create a new class `IncrementCounterAction` in the folder with `CounterFeature`.
  This class can remain empty.
- Edit `Counter.Razor` and add `@inject IDispatcher Dispatcher`.
- Remove the `currentCount` field as it is no longer needed.
- Change the `IncrementCount` method to dispatch `IncrementCounterAction`.

```razor
@page "/counter"
@using {YourNamespace}.CounterFeature
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
@inject IDispatcher Dispatcher
@inject IState<CounterState> CounterState

... existing razor markup

@code
{
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

- In the `CounterFeature` folder, create a new class `Reducers`.
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

##### Removing the `unused parameter` warning

When a reducer method is executed, it is passed the action that triggered it.
This allows the reducer to access any property values in the action that was dispatched,
for example, `new CustomerSearchAction('Bob')`.

In our simple case here, we do not need any values from `IncrementCounterAction` and
including it as a parameter might result in a `unusused parameter` warning at compile time.

We can circumvent this by including the action type in the `[ReducerMethod]` instead
of the method signature.

```c#
  [ReducerMethod(typeof(IncrementCounterAction))]
  public static CounterState ReduceIncrementCounterAction(CounterState state) =>
    new CounterState(clickCount: state.ClickCount + 1);
```

##### Splitting reducer methods across multiple classes

You may have as many reducer classes as you wish, the `Reducer` class name used in this tutorial is
not a special tag or anything. If the method is decorated with `[ReducerMethod]` and has the correct
signature, it will be used.

```c#
public static class SomeReducerClass
{
  [ReducerMethod]
  public static SomeState ReduceSomeAction(SomeState state, SomeAction action) => new SomeState();

  [ReducerMethod]
  public static SomeState ReduceSomeAction2(SomeState state, SomeAction2 action) => new SomeState();
}

public static class SomeOtherReducerClass
{
  [ReducerMethod]
  public static SomeState ReduceSomeAction3(SomeState state, SomeAction3 action) => new SomeState();

  [ReducerMethod]
  public static SomeState ReduceSomeAction4(SomeState state, SomeAction4 action) => new SomeState();
}
```

##### Injecting dependencies

**Tip: Do not inject state into reducers!**

It is also possible to decorate __instance__ methods with `[ReducerMethod]`.
Any dependencies in the class's constructor will be injected automatically.

I strongly recommend the use of static methods. Reducers should ideally be
[pure functions](https://en.wikipedia.org/wiki/Pure_function),
if you find yourself needing to inject dependencies into a reducer then you might be
taking the wrong approach, and should instead be using an Effect (covered later).

##### The `Reducer<TState, TAction>` class

It is also possible to create a reducer per state+action combination like this...

```c#
public class IncrementCounterReducer : Reducer<CounterState, IncrementCounterAction>
{
  public override CounterState Reduce(CounterState state, IncrementCounterAction action) =>
    new CounterState(clickCount: state.ClickCount + 1);
}
```

This pattern requires a lot more code, therefore its use is not recommended.
