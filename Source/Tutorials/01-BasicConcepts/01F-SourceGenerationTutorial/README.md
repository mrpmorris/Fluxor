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

Next, expand the project's `Dependencies` node, then `Analyzers`, then `Fluxor.StoreBuilderSourceGenerator` and observe
the auto-generated files. You can open these files and inspect them, the file of interest is
named `FluxorModule.cs`.

```c#
using System.Collections.Immutable;
namespace BasicConcepts.SourceGenerationTutorial
{
	public class FluxorModule : Fluxor.IFluxorModule
	{
		public IEnumerable<System.Type> Dependencies => // Classes
		public IEnumerable<System.Type> Effects => // Classes
		public IEnumerable<System.Type> Features => // Classes
		public IEnumerable<System.Type> Middlewares => // Classes
		public IEnumerable<System.Type> Reducers => // Classes
	}
}
```

1. **Dependencies:** A list of classes that are required for the generated `Effects` classes.
An `[EffectMethod]` may decorate an instance member on a class (unlike '[ReducerMethod]` which can
only decorate a static method). Because of this, the generated `Effect` method will need an instance
of the class that implements the `[EffectMethod]` code.  The `Dependencies` property lists these
types so they may be registered with your dependency injection container.
1. **Effects:** A list of `Effect` classes in the assembly, whether generated from `[EffectMethod]`
decorated methods, or a class that implements `Fluxor.IEffect`.
1. **Features:** A list of `Feature` classes in the assembly, whether generated from
`[FeatureState]` decorated classes, or a class that implements `IFeature`.
1. **Middlewares:** A list of `Middleware` classes in the assembly that implement `Fluxor.IMiddleware`.
1. **Reducers:** A list of `Reducer` classes in the assembly, whether generated from `[ReducerMethod]`
decorated methods, or a class that implements `Fluxor.IReducer<TState>`.

#### Bootstrapping our app
Replace the `ScanAssemblies` method call in `Program.cs` with `ImportModules`, and pass in
a collection of module instances you wish to import.

```c#
static void Main(string[] args)
{
  var services = new ServiceCollection();
  services.AddScoped<App>();
  services.AddFluxor(o => o
    .ImportModules(new MyAppNamespace.FluxorModule());

  IServiceProvider serviceProvider = services.BuildServiceProvider();

  var app = serviceProvider.GetRequiredService<App>();
  app.Run();
}
```

### Running our app

If we run our app now, and select option 1 or option 2, we should see the
same output as our Effects Tutorial project.