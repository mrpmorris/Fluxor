# Generated registration

`AddFluxor` normally finds your features, reducers, effects and middleware by reflection: it reads every type
and every public method of the assemblies you pass to `ScanAssemblies` and looks for Fluxor attributes and
interfaces. Building the store (the first time `IStore` is resolved) then creates each reducer and effect
wrapper by reflection too.

With generated registration, a source generator that ships in the `Fluxor` package writes that registration as
plain C# at build time. At run time Fluxor uses the generated list instead of reading the assembly, and creates
the wrappers through generated, typed calls. The store you get is the same: the same services, features,
reducers and effects, in the same order.

It is opt-in, per assembly. Nothing changes for a project that does not turn it on, and no call to `AddFluxor`
or `ScanAssemblies` changes.

## When it helps

* **Blazor WebAssembly start-up.** WebAssembly runs .NET on an interpreter, where reflection is slow and the
  start-up blocks the browser thread. This is where the gain is largest (see [Measured numbers](#measured-numbers)).
* **Large apps.** The reflection scan reads all the code in the scanned assemblies, not only the Fluxor code, so
  it gets slower as the app grows. The generated path only looks at the Fluxor members it lists (see
  [Complexity](#complexity)).

On CoreCLR (Blazor Server, ASP.NET Core, console and desktop apps) it is also faster, but by less: about 0.7x
of the reflection time for `AddFluxor` plus the store build. MAUI on Android and iOS runs on Mono, which was
not measured.

Trimming is not changed by this feature: Fluxor is not annotated or tested for trimming, and the generated path
still uses some reflection (it creates the generated registration from the assembly attribute, and some initial
states by reflection, see below). Keep your trimming settings as they are.

## Turning it on

Add one property to **each project that declares Fluxor code** (states, reducers, effects, middleware). That is
not necessarily the host: if your store lives in a class library, set it in the class library.

For example, the project of the [State, actions, and reducers](../Source/Tutorials/01-BasicConcepts/01A-StateActionsReducersTutorial/)
tutorial, with the `Fluxor` NuGet package referenced, becomes:

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<PropertyGroup>
		<OutputType>Exe</OutputType>
		<TargetFramework>net9.0</TargetFramework>
		<FluxorGenerateRegistration>true</FluxorGenerateRegistration>
	</PropertyGroup>
	<!-- the rest of the project is unchanged -->
</Project>
```

`Program.cs` stays as it is:

```c#
services.AddFluxor(o => o
	.ScanAssemblies(typeof(Program).Assembly));
```

To turn it on for every project in a solution, put the property in a `Directory.Build.props`. Projects that
do not reference Fluxor are ignored.

## Mixing generated and scanned assemblies

Each assembly decides for itself. When `AddFluxor` scans several assemblies, those with a generated
registration use it, and the others are scanned by reflection as before. The two are joined exactly as the
scan joins them today:

* a reducer in one assembly can reduce a state declared in another, whichever of the two is generated;
* `ScanTypes`, the middleware namespace filter and `AddMiddleware<T>` work the same;
* an assembly compiled against a Fluxor whose generated format this Fluxor does not know is scanned by
  reflection (correct, just not faster).

## What falls back: FLXG001

The generator only writes a registration when it can reproduce the reflection scan exactly. When it cannot,
it writes nothing for that assembly and reports warning **FLXG001**, once per problem, pointing at the member:

> `Shop.Store.Cart.Reducers.Reduce (is declared on a type that is not accessible from generated code (private or protected)) cannot be registered by generated code; this assembly uses the reflection scan`

The assembly then keeps the reflection scan as a whole, so the app behaves exactly as it does without the
property; it is just not faster. Common causes:

* a type the generated code cannot name: a reducer or effect on a private or protected nested class, a private
  state or action type, a tuple;
* an attribute derived from `ReducerMethodAttribute`, `EffectMethodAttribute` or `FeatureStateAttribute`;
* a member that is obsolete as an error, or experimental in another assembly;
* a Razor file the generator cannot see into (next section);
* a referenced Fluxor version without generated registration;
* a member the reflection scan rejects too (a wrong signature, a generic method, `ref` parameters or returns,
  an instance method on an abstract class or a struct): the app throws at start-up, as it does without the
  property.

To fix it, change the member (for example make the host class `internal` instead of `private`), or move it
to a project that does not opt in.

To keep the reflection scan for that assembly and silence the warning, suppress it in the project:

```xml
<PropertyGroup>
	<NoWarn>$(NoWarn);FLXG001</NoWarn>
</PropertyGroup>
```

With `TreatWarningsAsErrors`, FLXG001 fails the build. That is intended: the project asked for a generated
registration and did not get one. To keep the warning but not fail the build, add
`<WarningsNotAsErrors>$(WarningsNotAsErrors);FLXG001</WarningsNotAsErrors>` instead.

### Razor files

The classes Razor generates from `.razor` and `.cshtml` files are added by the Razor source generator, so this
generator cannot see them. A component could still carry members the scan registers, through `@inherits`,
`@implements` or attributed methods in `@code`. So when a Razor file in the project mentions, by name:

* `ReducerMethod`, `EffectMethod`, `FeatureState` (with or without `Attribute`) or an attribute derived from them;
* `IEffect`, `IMiddleware`, `IReducer<T>`, `IFeature<T>`, or an interface deriving from them;
* a class that is not sealed or static and implements one of those interfaces, or has a public
  `[ReducerMethod]` or `[EffectMethod]` (its own or inherited), such as `Effect<T>`, `Reducer<TState, TAction>`,
  `Feature<T>` or `Middleware`,

the assembly falls back with FLXG001 naming the file and the word. Classes in referenced assemblies count too,
including assemblies that reach Fluxor only through another assembly (a component base class in library C
that derives from a class in library B, where only B references Fluxor). The code-behind partial class of a
flagged file is reported as well (every partial class, when the flagged file is an `_Imports.razor`).

Components that only inject `IState<T>` or `IDispatcher`, or inherit from `FluxorComponent` or `FluxorLayout`,
are not affected. The check matches words, so a page whose text says "Middleware" also falls back; the
warning names the word. When a Razor project does fall back, keep its Fluxor code in a class library that opts
in.

### Known limitation: code from other source generators

Like the Razor classes, code that another source generator adds to the project is not visible to this generator:
every generator sees the project as it was before any of them ran. Razor is guarded (above); other generators are
not, and there is no reliable way to detect them. So if another generator in an opted-in project produces Fluxor
code, such as a `[ReducerMethod]`, `[EffectMethod]` or `[FeatureState]`, a class implementing `IEffect`,
`IReducer<T>`, `IFeature<T>` or `IMiddleware`, or the half of a `partial` class that adds such a base class or
interface, the generated registration does not list it and there is **no FLXG001**. The reflection scan would
find it, so the store built with generated registration silently lacks those features, reducers, effects or
middleware.

To notice it, compare the store with and without generated registration: start the app once with the
`Fluxor.DisableGeneratedRegistration` switch (see [Switching it off at run time](#switching-it-off-at-run-time))
and compare `IStore.Features` and the app's behaviour (an action handled only by a reducer or effect that the
other generator wrote then does nothing). A difference means members came from another generator.

What to do: do not opt in an assembly in which another source generator produces Fluxor code. Either leave
`FluxorGenerateRegistration` off for that project, or move the hand-written Fluxor code that should be generated
to a project without such a generator and opt in only that one.

## Private constructors and `required` members

Both are allowed. The generated code creates the initial state with `new T()` or a direct call to
`CreateInitialStateMethodName` when it can. When it cannot (a non-public parameterless constructor, `required`
members, a non-public factory method), it makes the same single reflection call the scan makes for that state
(`Activator.CreateInstance(type, nonPublic: true)`, or the factory method found by name), and only for that
state. The rest of the assembly stays generated. That call also gives the same result as the scan: an abstract
state type, for example, still throws at start-up, as it does without the property.

```c#
[FeatureState]
public class CounterState
{
	public int ClickCount { get; }

	private CounterState() { } // Required for creating initial state

	public CounterState(int clickCount)
	{
		ClickCount = clickCount;
	}
}
```

## Checking it is active

A generated assembly carries the `FluxorGeneratedRegistration` assembly attribute:

```c#
using System.Reflection;
using Fluxor.DependencyInjection.Generated;

bool isGenerated = typeof(CounterState).Assembly
	.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>() is not null;
```

To see the generated code, look under the project's analyzers in your IDE
(`Fluxor.SourceGenerator` > `Fluxor.SourceGenerator.FluxorRegistrationGenerator`), or write it to disk:

```xml
<PropertyGroup>
	<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

The file is then at
`obj/<configuration>/<target framework>/generated/Fluxor.SourceGenerator/Fluxor.SourceGenerator.FluxorRegistrationGenerator/FluxorGeneratedRegistration.g.cs`.
The class is `<assembly name>.FluxorGenerated.FluxorGeneratedRegistration`.

No attribute and no file, with `FluxorGenerateRegistration` set: look for FLXG001 in the build output. If
there is no FLXG001 either, the generator did not run: it ships in the `Fluxor` NuGet package, so the property
silently does nothing when the project references a Fluxor package from before this feature, or references
Fluxor's source as a `ProjectReference` (as the tutorials in this repository do). The assembly is then scanned
by reflection as before.

## Switching it off at run time

The AppContext switch `Fluxor.DisableGeneratedRegistration` makes `AddFluxor` ignore every generated
registration and scan by reflection, without a rebuild of the libraries. Set it before `AddFluxor`:

```c#
AppContext.SetSwitch("Fluxor.DisableGeneratedRegistration", true);
```

or in the app's project file (the host, since it writes the app's runtime configuration):

```xml
<ItemGroup>
	<RuntimeHostConfigurationOption Include="Fluxor.DisableGeneratedRegistration" Value="true" />
</ItemGroup>
```

## Measured numbers

### Blazor WebAssembly

A Blazor WebAssembly app (Release, Mono interpreter, no AOT) loaded in headless Chromium, each build loaded 10
times after a warm-up, the builds taking turns; median in milliseconds. "Fluxor start-up" is `AddFluxor` plus
everything from after `builder.Build()` until `IStore.Initialized` completed (`Build()` itself is not included). Full tables and the method:
[wasm-benchmark.md](../Source/Performance/results/wasm-benchmark.md).

| Sample | Fluxor start-up, scanned | Fluxor start-up, generated | Faster | `AddFluxor`, scanned / generated | First render, scanned / generated |
|---|---:|---:|---:|---:|---:|
| 200 features, 2,000 reducers, 2,000 effects | 2,232 | 894 | 2.5x | 713 / 116 | 2,520 / 1,175 |
| 80 features, 600 reducers, 600 effects | 908 | 458 | 2.0x | 307 / 65 | 1,200 / 752 |
| 20 features, 100 reducers, 100 effects | 392 | 283 | 1.4x | 105 / 47 | 693 / 586 |

The "Fluxor start-up" figure includes the host start and the first render, which take the same time with
both builds, so the ratio understates the gain in Fluxor's own work.

In a production Blazor WebAssembly app with ~200 features, the scan was 640-760 ms of the start-up.

### CoreCLR (Blazor Server, ASP.NET Core, console and desktop apps)

`SpeedTests` starts a sample with 200 states, 2,000 reducers and 2,000 effects in 7 cold processes per build
and compares `AddFluxor` plus the store build: generated took 0.69x to 0.76x of the scanned time over the runs
made during development (it asserts that generated is never slower). The same sample in
[scaling.md](../Source/Performance/results/scaling.md) (`K1`, median of 9 cold processes): 306.92 ms scanned,
230.28 ms generated, 0.75x. `AddFluxor` itself takes about a third of the scanned time; the store build saves
less, because the JIT compiles the generated factories on first call.

`Source/Performance/Fluxor.Benchmarks` has a BenchmarkDotNet cold-start benchmark of the same sample. No
results are checked in; run it with `dotnet run -c Release --project Source/Performance/Fluxor.Benchmarks`. Its
mean mixes in warm iterations, so for cold start the speed test above is the better measure.

### Running the measurements

None of this is built or run by a normal `dotnet build Source` or `dotnet test Source`: the performance projects
are in `Fluxor.sln` but opt-in, with `-p:FluxorPerformance=true` (the scaling samples also need
`-p:FluxorScalingSamples=true`, which implies it). They run in Release, from the repository root (shown for bash;
in PowerShell, set `$env:FLUXOR_PERF_TESTS = "1"` or `$env:FLUXOR_WASM_BENCHMARK = "1"` first):

```
# CoreCLR speed test
dotnet build Source -c Release -p:FluxorPerformance=true
FLUXOR_PERF_TESTS=1 dotnet test Source/Performance/Fluxor.PerformanceTests -c Release --no-build --filter FullyQualifiedName~SpeedTests

# Scaling tests (about 10 minutes; the scaling samples are opt-in because they are tens of MB of code)
dotnet build Source -c Release -p:FluxorPerformance=true -p:FluxorScalingSamples=true
FLUXOR_PERF_TESTS=1 dotnet test Source/Performance/Fluxor.PerformanceTests -c Release --no-build --filter FullyQualifiedName~ScalingTests

# Blazor WebAssembly benchmark (publishes the app once per variant; needs Playwright's Chromium)
dotnet build Source -c Release -p:FluxorPerformance=true
pwsh Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark/bin/Release/net10.0/playwright.ps1 install chromium
FLUXOR_WASM_BENCHMARK=1 dotnet test Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark -c Release --no-build
```

The scaling and browser tests rewrite their sections of the result files. The samples are not checked in: each sample project runs
`Source/Performance/Fluxor.Performance.SampleWriter` at build time and compiles what it writes into its obj folder.

## Complexity

The full derivation, with every step and its code, is in [scaling.md](../Source/Performance/results/scaling.md).
Notation: `T` = all types in the scanned assemblies, `M` = all their public methods (inherited ones included),
`F` features, `R` reducers, `E` effects, `K = F + R + E + middleware`, `n <= K` the types a generated
registration describes, `I` include-list entries, `X` middleware namespaces. Files are under
`Source/Lib/Fluxor/DependencyInjection/`.

| Step | Reflection scan | Generated |
|---|---|---|
| `AddFluxor` | **O(T * (I + X) + M + K)**: `Assembly.GetTypes()` and `GetInterfaces()` per type (`MiddlewareClassesDiscovery`, `ReflectionScanner`), `GetMethods` plus an attribute read per method (`AssemblyScanSettings`), the info factories, then one registration per member | **O(K log K + K * (I + X))**: the generated table is read once (`Generated/GeneratedRegistrationTable.CreateEntries`), sorted by `MetadataToken` (`GeneratedRegistrationCatalog`, the only step above linear), then the same filters and registration on the listed types only |
| Store build (first `IStore` resolve) | O(F + R + E), each wrapper made by `MakeGenericType`, `Activator` and `Delegate.CreateDelegate` | O(F + R + E), each wrapper made by a generated switch: a smaller constant, the same order |
| First call of generated code | none | O(emitted methods): one JIT compile (or interpreter transform) per method. The generator emits about one method per reducer or effect host plus a few per assembly (`Source/Lib/Fluxor.SourceGenerator/RegistrationEmitter.cs`): 454, 122 and 37 methods for the three browser samples above, 291 for the 4,200-member desktop sample, 2,270 at 33,600 members |
| Per dispatch | O(F + R + E) checks (`Source/Lib/Fluxor/Store.cs`, `Feature.cs`) | the same code |

So the scan grows with all the code in the scanned assemblies, and the generated path only with the Fluxor
members. Measured on CoreCLR (cold processes, median of 9, five runs: one on a quiet machine, whose numbers are
given here, and four that overlapped another project's test runs on the same machine; every pass and fail
agreed in all five):

* **Fluxor members doubled** (K = 4,200 to 33,600): `AddFluxor` plus store build grows linearly on both paths,
  log-log slope 0.94 scanned and 0.93 generated, generated 0.72x to 0.75x of scanned at every size. The store
  build alone: slope 0.97 and 0.98, generated cheaper per item at every size.
* **Non-Fluxor code added** (K fixed, 0 to 8,000 plain classes, 80,000 more methods): scanned `AddFluxor` grows
  by 1.05 ms per 1,000 methods (1.81x at 8,000 classes), generated stays flat (0.97x). In the browser, 4,000
  plain classes make scanned `AddFluxor` 1.73x and generated 1.01x.
* **The sort**: creating the catalog, sort included, is 0.30 % of the generated `AddFluxor` at the largest size.
  Its time per `n log n` varies by 2.3x over the sizes, because of one size; the cause of that is not
  established.
* **Dispatch** costs the same on both paths (generated / scanned 0.92 to 1.06 at every size).

**Open item: dispatch.** The number of checks per dispatch is linear, but Fluxor's existing dispatch wall time
grows about F^1.7 to F^2.1 over these sizes (about 140-170 µs per dispatch at 200 features to 5-8 ms at
1,600), on both paths. It is the existing dispatch path, not changed by this feature, and is to be
investigated separately.
The opt-in test `WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures` documents it and currently
fails on that slope; its generated / scanned ratio, the part this feature answers for, passes.

## What it does not do

* It does not make the store build lazy: the first `IStore` resolve still creates every feature, reducer and
  effect (there is no lazy effect construction or sliced store build).
* It does not change dispatching.
* It does not replace the reflection scan, which stays the default and the fallback.

An earlier, per-entry shape of the generated code was measured and rejected; see the historical section of
[wasm-benchmark.md](../Source/Performance/results/wasm-benchmark.md).
