# Fluxor - Basic concepts

## Source generation
The `Fluxor.StoreBuilderSourceGenerator` package is a `Roslyn` source generator.

Instead of having Fluxor scan assemblies at runtime, the source generator will perform this
scan during development and create a `FluxorModule` class which can then be used to discover
all of the `Features`, `Effects`, `Reducers`, and `Middlewares` without using reflection.

### Goal
This tutorial will be the same as the [Effects tutorial](../01B-EffectsTutorial/), but with
source code generation enabled.

### Steps
First, recreate the [Effects tutorial](../01B-EffectsTutorial/).

Next, add a NuGet package reference to `Fluxor.SotreBuilderSourceGenerator`.

Edit the `csproj` file, and ensure the reference to the source generator has the following
attributes

```xml
OutputItemType="Analyzer" ReferenceOutputAssembly="false"
```


#### Bootstrapping our app
Replace the `Main` method in `Program.cs` to execute our test app.

```c#
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

```c#
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