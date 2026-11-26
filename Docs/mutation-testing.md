# Mutation testing

Line coverage only shows that code *ran* during the tests. Mutation testing shows whether the tests would
*notice if the code were wrong*. [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
makes many small, deliberate bugs ("mutants") in the library: `<` becomes `<=`, `&&` becomes `||`, a
statement is removed, `true` becomes `false`. It then runs the unit tests against each one.

* **Killed**: a test failed, so the tests caught that bug.
* **Survived**: every test still passed. Either a test is missing or too weak, or the mutant cannot change
  behaviour (an *equivalent* mutant).
* **No coverage**: no test executes that code at all.

The **mutation score** is the percentage of mutants detected (killed or timed out).

## Running it

Mutation runs take several minutes per library, so they run on demand, not in CI. From the repository
root:

```powershell
./Source/run-mutation-tests.ps1                         # every library
./Source/run-mutation-tests.ps1 -Library fluxor         # fluxor | blazor-web | reduxdevtools
```

The HTML reports are written to `StrykerOutput/<library>/reports/mutation-report.html`. That folder is
git-ignored. Stryker.NET is a local tool pinned in `.config/dotnet-tools.json`, and the script restores it.

**Troubleshooting:** on Windows with Visual Studio installed, Stryker analyses projects with Visual
Studio's MSBuild. With the .NET 11 SDK, Visual Studio's MSBuild must be 18.12 or later. Older versions,
such as 18.7, fail in the Razor static web assets step of `Fluxor.Blazor.Web`. That failure leaves empty
`obj/**/rjsmrazor.dswa.cache.json` files, and every later `dotnet build` then fails with "The input does
not contain any JSON tokens". To fix it, update Visual Studio, then delete `Source/Lib/Fluxor.Blazor.Web/obj`.

## What is mutated

| Library | Config | Tests used | Score | `break` |
|---|---|---|---|---|
| Fluxor | `Source/stryker-fluxor.json` | Fluxor.UnitTests | 80.72 % (70.5 % when added) | 80 |
| Fluxor.Blazor.Web | `Source/stryker-blazor-web.json` | Fluxor.Blazor.Web.UnitTests | 14.68 % | 10 |
| Fluxor.Blazor.Web.ReduxDevTools | `Source/stryker-reduxdevtools.json` | Fluxor.Blazor.Web.ReduxDevTools.UnitTests | 15.18 % | 15 |

Stryker needs one target framework for multi-targeted projects, so the configs use `net10.0`. A run fails
when its score drops below `break`. That threshold is the latest score rounded down to a multiple of 5, so
it catches regressions without failing on today's gaps.

## Working with survivors

For each surviving or uncovered mutant, either:

1. write a test that fails for that mutant, asserting observable behaviour rather than implementation
   details, or
2. if the mutant cannot change behaviour (equivalent), note why in the pull request.

When a score goes up, raise that config's `break` to the new score rounded down to a multiple of 5.
