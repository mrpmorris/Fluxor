# Blazor WebAssembly start-up: generated registration against the reflection scan

Written by `WasmBenchmarkTests.WhenAppStartsInTheBrowser_ThenGeneratedRegistrationIsFaster` in
`Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark` (on demand: `FLUXOR_WASM_BENCHMARK=1`). Do not edit,
except the historical comparison at the end, which the benchmark keeps as it is.

To run it (it builds only with the opt-in performance projects):

    dotnet build Source -c Release -p:FluxorPerformance=true
    pwsh Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark/bin/Release/net10.0/playwright.ps1 install chromium
    FLUXOR_WASM_BENCHMARK=1 dotnet test Source/Performance/Fluxor.Performance.BlazorWasm.Benchmark -c Release --no-build

The app (`Source/Performance/Fluxor.Performance.BlazorWasm`) is published with `dotnet publish -c Release`
(Mono interpreter, no AOT) once per variant, selected with `-p:FluxorSampleSize=Small|Medium|Complex` and
`-p:FluxorSample=Scanned|Generated`; `Generated` is the generated registration, labelled by the shape of its
code, Compact. Each is served as static files from Kestrel and loaded in headless
Chromium. Phases come from `performance.mark` in the app: Boot = navigation start to `Main`; Host builder =
`WebAssemblyHostBuilder.CreateDefault`; AddFluxor = the `AddFluxor` call; Build = `builder.Build()`; Store
initialized = after `Build()` to `IStore.Initialized` completed (host start, first render, creating and
initializing the store); First render = navigation start to the page's first render. Total blocked = the
part over 50 ms of every long task (`PerformanceObserver` `longtask`) that started before the later of
first render and store initialized; Longest task = the longest of those tasks.

<!-- small: begin -->
## small: 20 features, 100 reducers, 100 effects

Median of 10 loads per variant (after 1 discarded warm-up load per variant), the variants taking turns, each load in a new browser context. Milliseconds; ratio = AddFluxor + store initialized against Scanned.

| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Scanned | 223.85 | 62.80 | 105.40 | 12.85 | 282.10 | **391.55** | **1.00** | 693.40 | 392.50 | 277.00 |
| Compact | 224.55 | 63.20 | 46.75 | 12.60 | 235.00 | **282.55** | **0.72** | 586.40 | 315.50 | 230.50 |

Store shape (the same in every load of a variant): method reducers/effects are those made from `[ReducerMethod]`/`[EffectMethod]`; in brackets, how many of them generated code created.

| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |
|---|---:|---:|---:|---:|---:|---|
| Scanned | 20 | 100 | 100 | 100 (0) | 100 (0) | no |
| Compact | 20 | 100 | 100 | 100 (100) | 100 (100) | yes |

Bar (Compact): ratio < 1.00; measured 0.72: pass.

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; Chromium 156.0.8078.4 headless (Playwright 1.64.1); app runtime .NET 10.0.15 browser-wasm; .NET SDK 10.0.404.
<!-- small: end -->

<!-- medium: begin -->
## medium: 80 features, 600 reducers, 600 effects

Median of 10 loads per variant (after 1 discarded warm-up load per variant), the variants taking turns, each load in a new browser context. Milliseconds; ratio = AddFluxor + store initialized against Scanned.

| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Scanned | 213.90 | 62.45 | 307.10 | 12.50 | 591.85 | **908.20** | **1.00** | 1199.65 | 916.00 | 595.50 |
| Compact | 211.40 | 62.15 | 65.00 | 11.65 | 394.60 | **458.25** | **0.50** | 752.05 | 464.50 | 388.50 |

Store shape (the same in every load of a variant): method reducers/effects are those made from `[ReducerMethod]`/`[EffectMethod]`; in brackets, how many of them generated code created.

| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |
|---|---:|---:|---:|---:|---:|---|
| Scanned | 80 | 600 | 600 | 596 (0) | 596 (0) | no |
| Compact | 80 | 600 | 600 | 596 (596) | 596 (596) | yes |

Bar (Compact): ratio <= 0.75; measured 0.50: pass.

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; Chromium 156.0.8078.4 headless (Playwright 1.64.1); app runtime .NET 10.0.15 browser-wasm; .NET SDK 10.0.404.
<!-- medium: end -->

<!-- complex: begin -->
## complex: 200 features, 2000 reducers, 2000 effects

Median of 10 loads per variant (after 1 discarded warm-up load per variant), the variants taking turns, each load in a new browser context. Milliseconds; ratio = AddFluxor + store initialized against Scanned.

| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Scanned | 209.75 | 59.60 | 712.90 | 14.70 | 1504.80 | **2232.20** | **1.00** | 2520.20 | 2228.00 | 1503.00 |
| Compact | 214.60 | 62.10 | 116.35 | 15.00 | 775.60 | **893.75** | **0.40** | 1175.40 | 887.50 | 770.00 |

Store shape (the same in every load of a variant): method reducers/effects are those made from `[ReducerMethod]`/`[EffectMethod]`; in brackets, how many of them generated code created.

| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |
|---|---:|---:|---:|---:|---:|---|
| Scanned | 200 | 2000 | 2000 | 1990 (0) | 1970 (0) | no |
| Compact | 200 | 2000 | 2000 | 1990 (1990) | 1970 (1970) | yes |

Bar (Compact): ratio <= 0.50; measured 0.40: pass.

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; Chromium 156.0.8078.4 headless (Playwright 1.64.1); app runtime .NET 10.0.15 browser-wasm; .NET SDK 10.0.404.
<!-- complex: end -->

<!-- complex-non-fluxor-code: begin -->
## complex with 0 and 4,000 plain classes: code that is not Fluxor's

Written by `WasmBenchmarkTests.WhenComplexSampleGetsNonFluxorCode_ThenTheStartUpIsReported`. Reported only, no bar. ComplexPlain4000 is the complex sample (200 features, 2,000 reducers, 2,000 effects) plus 4,000 plain classes of 10 public methods each in the same assembly, with no Fluxor code (`ScalingSample.ComplexPlain4000`, written at build time). Median of 10 loads per variant (after 1 discarded warm-up load per variant), the four variants taking turns, each load in a new browser context. Milliseconds; vs 0 classes = against the same build of the complex sample without plain classes.

| Sample | Variant | Boot (to Main) | AddFluxor | AddFluxor vs 0 classes | Store initialized | **AddFluxor + store initialized** | **vs 0 classes** | First render |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| Complex | Scanned | 167.25 | 500.55 | 1.00 | 1184.90 | **1685.20** | **1.00** | 1914.00 |
| Complex | Compact | 169.30 | 79.65 | 1.00 | 593.70 | **673.40** | **1.00** | 905.35 |
| ComplexPlain4000 | Scanned | 174.40 | 863.85 | 1.73 | 1190.80 | **2053.45** | **1.22** | 2291.80 |
| ComplexPlain4000 | Compact | 176.80 | 80.45 | 1.01 | 603.70 | **683.60** | **1.02** | 924.75 |

| Sample | Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) |
|---|---|---:|---:|---:|---:|---:|
| Complex | Scanned | 200 | 2000 | 2000 | 1990 (0) | 1970 (0) |
| Complex | Compact | 200 | 2000 | 2000 | 1990 (1990) | 1970 (1970) |
| ComplexPlain4000 | Scanned | 200 | 2000 | 2000 | 1990 (0) | 1970 (0) |
| ComplexPlain4000 | Compact | 200 | 2000 | 2000 | 1990 (1990) | 1970 (1970) |

Measured 2026-11-26 (UTC) on AMD Ryzen 9 5950X 16-Core Processor (32 logical processors), Microsoft Windows 10.0.26200; Chromium 156.0.8078.4 headless (Playwright 1.64.1); app runtime .NET 10.0.15 browser-wasm; .NET SDK 11.0.100.
<!-- complex-non-fluxor-code: end -->

<!-- history: begin -->
## Historical comparison: the PerEntry shape (removed)

Not written by the benchmark. The generator had a second shape, PerEntry, kept for measuring only (one method per
entry and per state, one lambda per reducer, effect and constructed initial state; selected with the MSBuild property
`FluxorGeneratedRegistrationShape=PerEntry`, sample build `GeneratedPerEntry`). It was measured as an alternative to
Compact in the same runs as the sections above (same loads, same machine, same date), and then removed by decision:
Compact is faster on WebAssembly at every size and, on CoreCLR, Compact is faster than the scan while PerEntry is
about twice as slow. The PerEntry numbers are kept here as the evidence for that choice; the Scanned and Compact
rows are repeated from the sections above for comparison.

### small: 20 features, 100 reducers, 100 effects

| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Scanned | 223.85 | 62.80 | 105.40 | 12.85 | 282.10 | **391.55** | **1.00** | 693.40 | 392.50 | 277.00 |
| Compact | 224.55 | 63.20 | 46.75 | 12.60 | 235.00 | **282.55** | **0.72** | 586.40 | 315.50 | 230.50 |
| PerEntry | 223.80 | 63.70 | 50.70 | 11.55 | 239.60 | **289.65** | **0.74** | 588.15 | 309.00 | 234.50 |

| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |
|---|---:|---:|---:|---:|---:|---|
| Scanned | 20 | 100 | 100 | 100 (0) | 100 (0) | no |
| Compact | 20 | 100 | 100 | 100 (100) | 100 (100) | yes |
| PerEntry | 20 | 100 | 100 | 100 (100) | 100 (100) | yes |

### medium: 80 features, 600 reducers, 600 effects

| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Scanned | 213.90 | 62.45 | 307.10 | 12.50 | 591.85 | **908.20** | **1.00** | 1199.65 | 916.00 | 595.50 |
| Compact | 211.40 | 62.15 | 65.00 | 11.65 | 394.60 | **458.25** | **0.50** | 752.05 | 464.50 | 388.50 |
| PerEntry | 219.70 | 64.25 | 80.40 | 11.85 | 414.80 | **496.70** | **0.55** | 788.35 | 500.00 | 409.50 |

| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |
|---|---:|---:|---:|---:|---:|---|
| Scanned | 80 | 600 | 600 | 596 (0) | 596 (0) | no |
| Compact | 80 | 600 | 600 | 596 (596) | 596 (596) | yes |
| PerEntry | 80 | 600 | 600 | 596 (596) | 596 (596) | yes |

### complex: 200 features, 2000 reducers, 2000 effects

| Variant | Boot (to Main) | Host builder | AddFluxor | Build | Store initialized | **AddFluxor + store initialized** | **Ratio** | First render | Total blocked | Longest task |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Scanned | 209.75 | 59.60 | 712.90 | 14.70 | 1504.80 | **2232.20** | **1.00** | 2520.20 | 2228.00 | 1503.00 |
| Compact | 214.60 | 62.10 | 116.35 | 15.00 | 775.60 | **893.75** | **0.40** | 1175.40 | 887.50 | 770.00 |
| PerEntry | 224.15 | 61.95 | 160.60 | 13.85 | 806.55 | **972.45** | **0.44** | 1282.80 | 967.50 | 802.00 |

| Variant | Features | Reducers | Effects | Method reducers (generated) | Method effects (generated) | Generated registration |
|---|---:|---:|---:|---:|---:|---|
| Scanned | 200 | 2000 | 2000 | 1990 (0) | 1970 (0) | no |
| Compact | 200 | 2000 | 2000 | 1990 (1990) | 1970 (1970) | yes |
| PerEntry | 200 | 2000 | 2000 | 1990 (1990) | 1970 (1970) | yes |

On CoreCLR (`SpeedTests`, the standard sample: 200 states, 2,000 reducers, 2,000 effects; median of 7 cold processes,
three runs), addFluxor + resolveStore against the scan was 0.73 / 0.74 / 0.74 for Compact and 2.03 to 2.06 for
PerEntry; the generated registration had 291 methods in the Compact shape and 5,086 in the PerEntry shape.
<!-- history: end -->
