# Scaling of Fluxor registration: generated against the reflection scan

The analysis is written by hand; the measured sections after it are written by `ScalingTests` in
`Source/Performance/Fluxor.PerformanceTests` (on demand: `FLUXOR_PERF_TESTS=1`, Release), each replaced when its
test runs, and the run log at the end gets one line per test run. The samples are written at build time by
`Fluxor.Performance.SampleWriter` (`ScalingSample`) into the obj folder of each sample project under
`Source/Performance/Samples/Scaling`, and measured in cold processes of `Fluxor.Performance.Runner` (CoreCLR).

The performance projects and the scaling samples are opt-in, so a normal `dotnet build Source` does not build them
(the scaling samples are tens of MB of generated code). To run the tests:

    dotnet build Source -c Release -p:FluxorPerformance=true -p:FluxorScalingSamples=true
    FLUXOR_PERF_TESTS=1 dotnet test Source/Performance/Fluxor.PerformanceTests -c Release --no-build --filter FullyQualifiedName~ScalingTests

Without the scaling samples the tests skip and say so. A run takes about 10 minutes.

<!-- analysis: begin -->
## Analysis

Notation: `T` = all types in the scanned assemblies; `M` = all public instance and static methods of those types,
inherited ones included (what `GetMethods(Public | Instance | Static | FlattenHierarchy)` returns); `A` = attribute
reads; `F` = features (feature classes + `[FeatureState]`), `R` = reducers, `E` = effects, `K = F + R + E +
middleware` = the Fluxor members; `X` = middleware namespaces; `I` = include-list entries; `n` = the types a generated
registration describes (states, reducer and effect hosts, reducer, effect and feature classes, middleware; `n <= K`).
Files are under `Source/Lib/Fluxor/DependencyInjection/` unless named otherwise.

The samples: `K1` is the standard sample (200 states, 10 reducers and 10 effects each: F = 200, R = 2,000, E = 2,000,
K = 4,200, T = 2,500, M = 34,820); `K2`, `K4`, `K8` multiply every count (states, hosts, reducer and effect classes);
`K1Plain2000/4000/8000` add 2,000/4,000/8,000 plain classes of 10 methods each (no Fluxor code) to K1. Measured
values are the range over five runs with the median of 9 cold processes per variant and size (2026-11-26, Ryzen 9
5950X, .NET 10.0.15, SDK 11.0.100). One run was clean: no other test or build process ran on the machine during it
(checked every 10 s), and its tables are the ones below (run log, 10:37-10:38 UTC). The other four overlapped another
project's test runs on the same machine (between a third and four fifths of the samples). The direction of every
result was the same in all five: every pass and every fail agree, and the ranges are given where the numbers differ.
The run log at the end also has three earlier runs with the median of 5. The sort test was named
`WhenEntriesGrow_ThenTheMetadataTokenSortIsNLogNAndNegligible` in the run log rows written before it was renamed.

Why 1x = 200 states and not 25: a cold process has a fixed cost (Fluxor's own JIT, the container) of about 60 ms
here. At 25 to 200 states that fixed cost is most of addFluxor + resolveStore, and the log-log slope of a cold start
would be about 0.5 whatever the order of growth; from 200 states the part that grows with K dominates.

### Reflection scan at `AddFluxor` (`ReflectionScanner.Scan`)

- `GeneratedRegistrationCatalog.Create`: one `GetCustomAttribute` per assembly, O(assemblies).
- `MiddlewareClassesDiscovery.FindMiddlewareLocations`: `Assembly.GetTypes()` and `GetInterfaces()` per type: O(T).
- `ReflectionScanner.GetCandidateTypes`: `GetTypes()` again, `Distinct`/`Union` (hashing) O(T), and
  `AssemblyScanSettings.FilterClasses`: per type `scanIncludeList.Any` + `scanExcludeList.Any`: O(T * (I + X)).
- `AssemblyScanSettings.FilterMethods` / `GetReflectedMethods`: `GetMethods(FlattenHierarchy)` per type and
  `GetCustomAttributes(true)` per method: O(M) method enumerations with an attribute read each, A = O(M).
- Info factories (`ReducerClassInfoFactory`, `EffectClassInfoFactory`, `FeatureClassInfoFactory`: `GetInterfaces()`
  per type, O(T); `FeatureStateInfoFactory`: `GetCustomAttribute<FeatureStateAttribute>` per type, O(T), and per
  state `MakeGenericType` twice plus a constructor or factory lookup, O(F); `ReducerMethodInfoFactory` /
  `EffectMethodInfoFactory`: a second attribute read and `GetParameters()` per Fluxor method, O(R + E)).
- `StoreRegistration.Register` / `FeatureRegistration.Register`: `GroupBy` + `ToDictionary` by state type and one
  service registration per member, O(K).

Total **O(T * (I + X) + M + K)**: it grows with all the code in the scanned assemblies, not only Fluxor code.
Measured (`WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower`, K fixed, M from 34,820 to 162,820): scanned
addFluxor grows linearly with the added methods, **+1.05 ms per 1,000 methods** in the quiet run (1.05 to 1.14 over
all five; bar >= 0.50 ms per 1,000 methods, the least squares slope over 0, 2,000, 4,000 and 8,000 classes), so the
8,000-class sample takes 1.81x the 0-class one (1.81 to 1.92; not a bar: the ratio depends on how large the fixed K
part of the 1x sample is, here about 100 ms).
In the browser (Mono interpreter, `wasm-benchmark.md`, section "complex with 0 and 4,000 plain classes", reported
only, one run of 10 loads per variant): 4,000 plain classes make the scanned AddFluxor **1.73x** (500.55 to
863.85 ms) and the generated one **1.01x** (79.65 to 80.45 ms); AddFluxor + store initialized 1.22x scanned, 1.02x
generated.

### Generated path at `AddFluxor` (same `ReflectionScanner.Scan`, the catalog branch of each step)

- `GeneratedRegistrationCatalog.Create`: per assembly one attribute read and one `Activator.CreateInstance` of the
  registration; the first access of `Types` builds the entries from the int table
  (`Generated/GeneratedRegistrationTable.CreateEntries`, one pass over the table, O(K)) and caches them in a static;
  then per entry `Dictionary.TryAdd` O(n) and the `MetadataToken` sort `OrderBy(x => x.MetadataToken)`,
  **O(n log n)**, the only step above linear.
- `MiddlewareClassesDiscovery.GetMiddlewareTypes`, `ReflectionScanner.GetTypes`: the catalog's types, O(n); no
  `Assembly.GetTypes()`.
- `AssemblyScanSettings.FilterClasses` on those types only: O(n * (I + X)).
- `AssemblyScanSettings.GetGeneratedMethods`: the entry's reducer and effect lists, O(R + E); no `GetMethods`, no
  attribute reads.
- Info factories: `TryGetEntry` per type, O(n); infos built from the generated descriptors with their typed
  delegates, O(K), no `MakeGenericType`.
- Registration: as the scan, O(K).

Total **O(K log K + K * (I + X))** (n <= K), independent of `T` and `M`. Measured: generated addFluxor stays flat as
non-Fluxor code grows, **0.97x** at 8,000 classes in the quiet run (0.97 to 1.03; bar <= 1.25x), slope -0.01 to
+0.02 ms per 1,000 methods. Together with the scan: addFluxor + resolveStore grows linearly in K on both paths,
log-log slope **scanned 0.94, generated 0.93** (0.94 to 0.97 and 0.93 to 0.94 over all runs; bar [0.70, 1.30],
`WhenFluxorMembersDouble_ThenBothPathsGrowLinearly`), generated about 0.7x scanned at every size. Mixed apps: an assembly without a generated registration (or with a format this Fluxor
does not know) is scanned as above, so its own T and M still count.

The sort: `WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible` times `GeneratedRegistrationCatalog.Create`
alone (warm: 2 s of calls first, then the median of 31 batches of 500 calls per size, the sizes taking turns;
n = 500 to 4,000). It asserts one thing: Create, sort included, is **0.30 %** of the cold generated addFluxor at 8x
in the clean run (0.26 % to 0.44 % over all five; bar <= 5 %). The sort is negligible next to the rest of
addFluxor. Reported, not asserted: time / (n log2 n) varies by 2.30x over the sizes in the clean run (1.70 to 2.56),
and the slope of time against n is 1.31 (1.23 to 1.42), where n log n alone would give about 1.15 over this range.
The spread comes from one size: in every one of the eight recorded runs K4 (n = 2,000) has the highest time per
n log2 n (0.0131-0.0141 µs) and K8 (n = 4,000) a lower one (0.0058-0.0107 µs), with K1 and K2 lower still. Its
cause is not established. A cache-capacity effect would not make 8x cheaper per entry than 4x, so that is not the
explanation; something specific to the K4 sample or its size (for example GC or allocation behaviour at that size)
is possible but was not investigated, as the test asserts only the share.

### Store build: first `IStore` resolve (`ServiceRegistration/StoreRegistration.Register`, the `Store` factory)

Both paths run the same loop: `Store.AddFeature` per feature, each feature's factory
(`ServiceRegistration/FeatureRegistration.RegisterStateInfos` / `RegisterFeatureClassInfos`) adds its reducers
(`AddReducers`), then `Store.AddEffect` per effect: **O(F + R + E)** service resolutions and wrapper creations on both.
The scan creates each wrapper with `MakeGenericType` + `Activator.CreateInstance` + `Delegate.CreateDelegate`
(`WrapperFactories/ReducerWrapperFactory.Create`, `EffectWrapperFactory.Create`, `Wrappers/ReducerWrapper`) and
adds reducers by `MethodInfo.Invoke` (`FeatureRegistration.CreateReflectionAddReducer`); the generated path calls
the generated factory (`CreateReducersN(host, index)`, a switch) and the generated typed `AddReducer` switch. That
removes a per-item constant, not the order of growth. Measured
(`WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem`): resolveStore slope
**scanned 0.97, generated 0.98** (0.97 to 0.99 and 0.98 to 0.99 over all runs); per item roughly 40-50 µs on both,
generated cheaper at every size in every run with the median of 9 (generated / scanned 0.90 to 0.99 at 8x). The
constant saved is small on CoreCLR at a cold start because the generated factories cost first-call JIT (next
paragraph) that the scan does not have (resolveStore JIT at 1x: generated about 145-170 ms, scanned 110-125 ms).
With the median of 5, one of three runs had a tie at 1x (1.002x).

First call of the generated code (JIT on CoreCLR, the interpreter's transform on Mono/WebAssembly): one compile per
emitted method, each in proportion to its size, so **O(emitted methods)** calls and O(K) code in all
(`Source/Lib/Fluxor.SourceGenerator/RegistrationEmitter.cs`: O(hosts + types / 256) methods, one factory per host
with one case per reducer or effect, one switch each for feature creation, initial state and AddReducer). The
Compact shape emits, measured by the runner (methods and constructors of the registration class): Small 37,
Medium 122, Complex 454 (and ComplexPlain4000 454: plain classes add none), standard sample / K1 291, K2 574, K4 1,139, K8 2,270 (about 0.068 per
Fluxor member).

### Per dispatch (`Store.DequeueActions`; not changed by this feature)

Every feature is notified (`FeaturesByName.Values`), each feature asks **every** one of its reducers
`ShouldReduceStateForAction` (`Feature<TState>.ReceiveDispatchNotificationFromStore`, `Reducers.Where(...)`), the
store asks every effect `ShouldReactToAction` (`Store.TriggerEffects`), plus middleware and action subscribers:
**O(F + R + E)** checks, the same on both paths (correcting the brief's "O(F + matching reducers + E)": all reducers
are asked, only the matching ones run). Both paths use the same wrapper types (`ReducerWrapper<TState, TAction>`,
`EffectWrapper<TAction>`, check `action is TAction`). Measured
(`WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures`): generated / scanned per dispatch **0.92 to 1.06**
at every size in the quiet run (0.84 to 1.06 in the three earlier runs; bar [0.80, 1.25]; three of the four runs
that overlapped another project's tests had one size outside it, 0.75 to 1.32): unaffected. **The wall-clock slope
against F is 1.93 scanned, 1.86 generated in the quiet run, 1.71 to 2.09 over all runs, outside the bar
[0.70, 1.30]** on both paths: about 140-170 µs at 200 features to 5-8 ms at 1,600 (wall time about F^1.7 to F^2.1). The number of checks is linear (the
code above); the cost of one check is not constant over this range: each wrapper is a distinct generic
instantiation, and `action is TAction` in shared generic code goes through the runtime's cast cache, whose hit rate
falls as the number of distinct (action type, TAction) pairs grows, plus the CPU caches. Measured in isolation (a
scratch program, `o is A` over N distinct `Checker<A>` instantiations of the K8 sample's action types): 11 ns per
check at N = 1,000, 12 at 2,000, 30 at 4,000, 66 at 8,000, 75 at 16,000 (levelling off), against 9-11 ns at every N
when only 500 distinct instantiations are used. So per dispatch is O(F + R + E) operations with a per-operation
cost that rises until those caches are exhausted, and is then bounded: superlinear in wall-clock time between about
2,000 and 16,000 reducers + effects on this machine, linear beyond. This is the existing dispatch path, the same on
both registration paths; this feature neither causes nor changes it.

**Open item:** the dispatcher's superlinear wall time is to be investigated as separate work, outside the generated
registration. Until then `WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures` keeps its bars and is
expected to fail on the slope (opt-in timing test); its generated / scanned ratio is the part this feature answers for.

### No quadratic step in the changed code

Checked in the code above: every step the generated path adds or changes is one pass with hashing
(`Dictionary.TryAdd`, `Distinct`, `Union`, `GroupBy`, `ToDictionary`) or a lookup by index (the table, the factory and
feature switches), except the O(n log n) `MetadataToken` sort. `FilterClasses` is O(n * (I + X)), linear in n for a
given configuration (X is the number of middleware namespaces, I the include list). Nothing in the changed code
compares every member with every other. Measured: no slope above 1.0 + noise on any cold registration phase
(addFluxor + resolveStore 0.88-0.97, resolveStore 0.93-0.99 over all runs); the warm catalog alone has slope
1.18-1.42 against n (the sort section: above n log n because of the 4x size, cause not established; 0.3 % of addFluxor); the only slope above 1.5
is the dispatch path above, which is unchanged code and the same on both paths (open item).
<!-- analysis: end -->

<!-- WhenFluxorMembersDouble_ThenBothPathsGrowLinearly: begin -->
## WhenFluxorMembersDouble_ThenBothPathsGrowLinearly

addFluxor + resolveStore, median of 9 cold processes per variant and size, milliseconds.

| Sample | F | R | E | K | Scanned ms | Generated ms | Generated / Scanned | Generated registration methods |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| K1 | 200 | 2,000 | 2,000 | 4,200 | 306.92 | 230.28 | 0.75 | 291 |
| K2 | 400 | 4,000 | 4,000 | 8,400 | 555.68 | 402.08 | 0.72 | 574 |
| K4 | 800 | 8,000 | 8,000 | 16,800 | 1066.08 | 765.13 | 0.72 | 1,139 |
| K8 | 1,600 | 16,000 | 16,000 | 33,600 | 2181.99 | 1591.20 | 0.73 | 2,270 |

Slope of log(time) against log(K): Scanned **0.94**, Generated **0.93**; bar [0.70, 1.30].

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; .NET 10.0.15 (CoreCLR, the runner's runtime); built with .NET SDK 11.0.100, Release.
<!-- WhenFluxorMembersDouble_ThenBothPathsGrowLinearly: end -->

<!-- WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem: begin -->
## WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem

resolveStore (the store build), median of 9 cold processes per variant and size; per item = resolveStore / K in microseconds. JIT columns: what the runtime reports for the phase (not asserted).

| Sample | K | Scanned ms | Generated ms | Scanned µs per item | Generated µs per item | Generated / Scanned | Scanned JIT ms (methods) | Generated JIT ms (methods) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| K1 | 4,200 | 203.05 | 191.28 | 48.34 | 45.54 | 0.94 | 117.20 (773) | 154.37 (706) |
| K2 | 8,400 | 379.31 | 354.16 | 45.16 | 42.16 | 0.93 | 212.70 (1,344) | 290.10 (1,245) |
| K4 | 16,800 | 743.58 | 697.00 | 44.26 | 41.49 | 0.94 | 401.79 (2,484) | 571.05 (2,291) |
| K8 | 33,600 | 1534.91 | 1474.58 | 45.68 | 43.89 | 0.96 | 805.39 (4,765) | 1205.99 (4,462) |

Slope of log(resolveStore) against log(K): Scanned **0.97**, Generated **0.98**; bar [0.70, 1.30].

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; .NET 10.0.15 (CoreCLR, the runner's runtime); built with .NET SDK 11.0.100, Release.
<!-- WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem: end -->

<!-- WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures: begin -->
## WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures

Microseconds per dispatch of an action handled by one reducer and one effect: in each cold process, after the store is built and 1,000 warm-up dispatches, the median of 5 batches of 1,000; then the median of 3 processes.

| Sample | F | R | E | Scanned µs | Generated µs | Generated / Scanned |
|---|---:|---:|---:|---:|---:|---:|
| K1 | 200 | 2,000 | 2,000 | 158.98 | 166.93 | 1.05 |
| K2 | 400 | 4,000 | 4,000 | 481.35 | 512.46 | 1.06 |
| K4 | 800 | 8,000 | 8,000 | 2185.04 | 2150.19 | 0.98 |
| K8 | 1,600 | 16,000 | 16,000 | 8339.71 | 7667.84 | 0.92 |

Slope of log(per dispatch) against log(F): Scanned **1.93**, Generated **1.86**; bar [0.70, 1.30].

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; .NET 10.0.15 (CoreCLR, the runner's runtime); built with .NET SDK 11.0.100, Release.
<!-- WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures: end -->

<!-- WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible: begin -->
## WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible

`GeneratedRegistrationCatalog.Create` for one generated sample assembly, warm (2 s of calls first), in the test process: median of 31 batches of 500 calls, the sizes taking turns. n = types the catalog sorts by MetadataToken (the described Fluxor types: states, hosts, reducer and effect classes).

| Sample | K | n | µs per Create | µs / (n log2 n) |
|---|---:|---:|---:|---:|
| K1 | 4,200 | 500 | 29.54 | 0.0066 |
| K2 | 8,400 | 1,000 | 57.25 | 0.0057 |
| K4 | 16,800 | 2,000 | 289.39 | 0.0132 |
| K8 | 33,600 | 4,000 | 358.63 | 0.0075 |

Largest / smallest µs / (n log2 n): 2.30; slope of log(time) against log(n): 1.31 (both reported, not asserted: n log n alone gives a slope of about 1.15 over n = 500 to 4,000; the excess comes from the 4x size, which has the highest µs / (n log2 n), cause not established). At K8, Create is 0.36 ms of the cold generated addFluxor 120.11 ms: **0.30 %** (bar <= 5%).

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; .NET 10.0.15 (CoreCLR, the runner's runtime); built with .NET SDK 11.0.100, Release.
<!-- WhenEntriesGrow_ThenTheMetadataTokenSortStaysNegligible: end -->

<!-- WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower: begin -->
## WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower

addFluxor, median of 9 cold processes per variant and size, milliseconds. K is fixed (4,200); the plain classes have 10 public methods each and no Fluxor attributes. T and M are counted by the runner after measuring.

| Sample | Plain classes | Added methods | T | M | Scanned ms | Generated ms | Scanned / 0 | Generated / 0 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| K1 | 0 | 0 | 2,500 | 34,820 | 102.59 | 36.96 | 1.00 | 1.00 |
| K1Plain2000 | 2,000 | 20,000 | 4,500 | 66,820 | 118.78 | 35.95 | 1.16 | 0.97 |
| K1Plain4000 | 4,000 | 40,000 | 6,500 | 98,820 | 140.76 | 37.73 | 1.37 | 1.02 |
| K1Plain8000 | 8,000 | 80,000 | 10,500 | 162,820 | 185.38 | 35.91 | 1.81 | 0.97 |

Least squares slope against added methods: Scanned **1.05 ms per 1,000 methods** (bar >= 0.50), Generated -0.01 ms per 1,000 methods. Largest / 0 classes: Scanned 1.81 (not a bar), Generated **0.97** (bar <= 1.25).

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; .NET 10.0.15 (CoreCLR, the runner's runtime); built with .NET SDK 11.0.100, Release.
<!-- WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower: end -->

<!-- runs: begin -->
## Run log

| Date (UTC) | Test | Result | SDK |
|---|---|---|---|
| 2026-11-26 02:22 | WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures | slopes scanned 2.05, generated 1.97; ratios 1.03 / 0.95 / 0.99 / 0.84 | 11.0.100 |
| 2026-11-26 02:22 | WhenEntriesGrow_ThenTheMetadataTokenSortIsNLogNAndNegligible | n log n spread 1.77, slope vs n 1.29, share of addFluxor at 8x 0.39 % | 11.0.100 |
| 2026-11-26 02:22 | WhenFluxorMembersDouble_ThenBothPathsGrowLinearly | slopes scanned 0.95, generated 0.92 | 11.0.100 |
| 2026-11-26 02:22 | WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower | scanned 1.08 ms per 1,000 methods, x1.79; generated 0.01 ms per 1,000 methods, x1.01 | 11.0.100 |
| 2026-11-26 02:22 | WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem | slopes scanned 0.96, generated 0.97; generated / scanned at 8x 0.94 | 11.0.100 |
| 2026-11-26 02:38 | WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures | slopes scanned 2.03, generated 2.07; ratios 0.90 / 0.99 / 1.01 / 0.98 | 11.0.100 |
| 2026-11-26 02:38 | WhenEntriesGrow_ThenTheMetadataTokenSortIsNLogNAndNegligible | n log n spread 1.49, slope vs n 1.18, share of addFluxor at 8x 0.41 % | 11.0.100 |
| 2026-11-26 02:38 | WhenFluxorMembersDouble_ThenBothPathsGrowLinearly | slopes scanned 0.96, generated 0.88 | 11.0.100 |
| 2026-11-26 02:38 | WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower | scanned 1.05 ms per 1,000 methods, x1.78; generated -0.02 ms per 1,000 methods, x0.97 | 11.0.100 |
| 2026-11-26 02:38 | WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem | slopes scanned 0.99, generated 0.93; generated / scanned at 8x 0.88 | 11.0.100 |
| 2026-11-26 03:06 | WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures | slopes scanned 1.74, generated 1.71; ratios 1.06 / 1.03 / 1.04 / 0.98 | 11.0.100 |
| 2026-11-26 03:06 | WhenEntriesGrow_ThenTheMetadataTokenSortIsNLogNAndNegligible | n log n spread 2.52, slope vs n 1.21, share of addFluxor at 8x 0.23 % | 11.0.100 |
| 2026-11-26 03:06 | WhenFluxorMembersDouble_ThenBothPathsGrowLinearly | slopes scanned 0.96, generated 0.94 | 11.0.100 |
| 2026-11-26 03:06 | WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower | scanned 1.00 ms per 1,000 methods, x1.86; generated -0.00 ms per 1,000 methods, x1.00 | 11.0.100 |
| 2026-11-26 03:06 | WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem | slopes scanned 0.98, generated 0.98; generated / scanned at 8x 0.96 | 11.0.100 |
| 2026-11-26 10:37 | WhenDispatching_ThenCostIsTheSameOnBothPathsAndLinearInFeatures | slopes scanned 1.93, generated 1.86; ratios 1.05 / 1.06 / 0.98 / 0.92 | 11.0.100 |
| 2026-11-26 10:37 | WhenEntriesGrow_ThenTheMetadataTokenSortIsNLogNAndNegligible | n log n spread 2.30, slope vs n 1.31, share of addFluxor at 8x 0.30 % | 11.0.100 |
| 2026-11-26 10:37 | WhenFluxorMembersDouble_ThenBothPathsGrowLinearly | slopes scanned 0.94, generated 0.93 | 11.0.100 |
| 2026-11-26 10:38 | WhenNonFluxorCodeGrows_ThenOnlyTheReflectionScanGetsSlower | scanned 1.05 ms per 1,000 methods, x1.81; generated -0.01 ms per 1,000 methods, x0.97 | 11.0.100 |
| 2026-11-26 10:38 | WhenFluxorMembersDouble_ThenStoreBuildGrowsLinearlyOnBothPathsAndGeneratedIsCheaperPerItem | slopes scanned 0.97, generated 0.98; generated / scanned at 8x 0.96 | 11.0.100 |
<!-- runs: end -->
