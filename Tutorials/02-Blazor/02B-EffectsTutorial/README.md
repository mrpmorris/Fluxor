# Fluxor - Blazor Web Tutorials

## Effects

Flux state is supposed to be immutable, and that state replaced only by
[pure functions](https://en.wikipedia.org/wiki/Pure_function), which should only take input from their
parameters.

With this in mind, we need something that will enable us to access other sources of data such as
web services, and then reduce the results into our state.

### Goal
This tutorial will recreate the `Fetch data` page in a standard Blazor app.

### Steps

- Under the `Store` folder, create a new folder named `WeatherUseCase`.
- Create a new state class to hold the state for this use case.

```c#
public class WeatherState
{
  public bool IsLoading { get; }
  public IEnumerable<WeatherForecast> Forecasts { get; }

  public WeatherState(bool isLoading, IEnumerable<WeatherForecast> forecasts)
  {
    IsLoading = isLoading;
    Forecasts = forecasts ?? Array.Empty<WeatherForecast>();
  }
}
```

This state holds a property indicating whether or not the data is currently being retrieved from
the server, and an enumerable holding zero to many `WeatherForecast` objects.

#### Displaying state in the component

- Find the `Pages` folder and add a new file named `FetchData.razor.cs`
- Mark the class `partial`.
- Add the following `using` declarations

```c#
using Fluxor;
using Microsoft.AspNetCore.Components;
using YourAppName.Store.WeatherUseCase;
```

- Next we need to inject the `WeatherState` into our component

```c#
public partial class FetchData
{
  [Inject]
  private IState<WeatherState> WeatherState { get; set; }
}
```

- Edit `FetchData.razor` and make the page descend from `FluxorComponent`.

```
@inherits Fluxor.Blazor.Web.Components.FluxorComponent
```

- Change the mark-up so it uses our `IsLoading` state to determine if data is being
retrieved from the server or not.

Change

`@if (forecasts == null)`

to

`@if (WeatherState.Value.IsLoading)`

- Change the mark-up so it uses our `Forecasts` state.

Change

`@foreach (var forecast in forecasts)`

to

`@foreach (var forecast in WeatherState.Value.Forecasts)`

- Remove `@inject WeatherForecastService ForecastService`

#### Using an Action and a Reducer to alter state

- Create an empty class `FetchDataAction`.
- Create a static `Reducers` class, which will set `IsLoading` to true when our 
`FetchDataAction` action is dispatched.

```c#
public static class Reducers
{
  [ReducerMethod]
  public static WeatherState ReduceFetchDataAction(WeatherState state, FetchDataAction action) =>
    new WeatherState(
      isLoading: true,
      forecasts: null);
}
```

Alternatively, because we aren't using any values from the `FetchDataAction action` we
can declare our reducer method without that parameter, like so:

```c#
public static class Reducers
{
  [ReducerMethod(typeof(FetchDataAction))]
  public static WeatherState ReduceFetchDataAction(WeatherState state) =>
    new WeatherState(
      isLoading: true,
      forecasts: null);
}
```

- In `Fetchdata.razor.cs` inject `IDispatcher` and dispatch our action from the `OnInitialized`
lifecycle method. The code-behind class should now look like this

```c#
public partial class FetchData
{
  [Inject]
  private IState<WeatherState> WeatherState { get; set; }

  [Inject]
  private IDispatcher Dispatcher { get; set; }

  protected override void OnInitialized()
  {
    base.OnInitialized();
    Dispatcher.Dispatch(new FetchDataAction());
  }
}
```

#### Requesting data from the server via an `Effect`

Effect handlers cannot (and should not) affect state directly. They are triggered when the action
they are interested in is dispatched through the store, and as a response they can dispatch new actions.

Effect handlers can be written in one of three ways.


1. As with `[ReducerMethod]`, it is possible to use `[EffectMethod]` without
  the action parameter being needed in the method signature.

```
  [EffectMethod(typeof(FetchDataAction))]
  public async Task HandleFetchDataAction(IDispatcher dispatcher)
  {
    var forecasts = await WeatherForecastService.GetForecastAsync(DateTime.Now);
    dispatcher.Dispatch(new FetchDataResultAction(forecasts));
  }
```

2. By decorating instance or static methods with `[EffectMethod]`. The name of the class and the
method are unimportant.

```c#
public class Effects
{
  private readonly IWeatherForecastService WeatherForecastService;

  public Effects(IWeatherForecastService weatherForecastService)
  {
    WeatherForecastService = weatherForecastService;
  }

  [EffectMethod]
  public async Task HandleFetchDataAction(FetchDataAction action, IDispatcher dispatcher)
  {
    var forecasts = await WeatherForecastService.GetForecastAsync(DateTime.Now);
    dispatcher.Dispatch(new FetchDataResultAction(forecasts));
  }
}
```

3. By descending from the `Effect<TAction>` class. The name of the class is unimportant, and `TAction`
identifies the type of action that should trigger this `Effect`.

```c#
public class FetchDataActionEffect : Effect<FetchDataAction>
{
  private readonly IWeatherForecastService WeatherForecastService;

  public FetchDataActionEffect(IWeatherForecastService weatherForecastService)
  {
    WeatherForecastService = weatherForecastService;
  }

  public override async Task HandleAsync(FetchDataAction action, IDispatcher dispatcher)
  {
    var forecasts = await WeatherForecastService.GetForecastAsync(DateTime.Now);
    dispatcher.Dispatch(new FetchDataResultAction(forecasts));
  }
}
```


These approaches work equally well, which you choose is an organisational choice. But keep
in mind the following

1. An `[EffectMethod]` can be declared either as static or instance.
2. If declared as an instance method, then an instance of the owning class will be created.
3. Any dependencies that class requires will be injected, this means that multiple
  `[EffectMethod]`s can share property values (i.e. a CancellationToken).

I recommend you use approach 1 when you do not need to access values in the action object,
otherwise use approach 2. Approach 3 is not recommended due to the amount of code involved.

#### Reducing the `Effect` result into state

- Create a new class `FetchDataResultAction`, which will hold the results of the call to the server
so they can be "reduced" into our application state.

```c#
public class FetchDataResultAction
{
  public IEnumerable<WeatherForecast> Forecasts { get; }

  public FetchDataResultAction(IEnumerable<WeatherForecast> forecasts)
  {
    Forecasts = forecasts;
  }
}
```

This is the action that is dispatched by our `Effect` earlier, after it has retrieved the data from
the server via an HTTP request.

- Edit the `Reducers.cs` class and add a new `[ReducerMethod]` to reduce the contents of this result
action into state.

```c#
[ReducerMethod(typeof(FetchDataResultAction))]
public static WeatherState ReduceFetchDataResultAction(WeatherState state) =>
  new WeatherState(
    isLoading: false,
    forecasts: action.Forecasts);
```

This reducer simply sets the `IsLoading` state back to false, and sets the `Forecasts` state to the
values in the action that was dispatched by our effect.


