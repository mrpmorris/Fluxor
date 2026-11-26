# Mutation testing

Line coverage tells you which code the tests ran. It does not tell you whether a test would notice if that code
were wrong. Mutation testing does: [Stryker.NET](https://stryker-mutator.io/docs/stryker-net/introduction/)
makes many small deliberate bugs in the code ("mutants"), for example `>` becomes `>=`, `true` becomes
`false`, a statement is removed or a `return` gets an early exit, and runs the tests once for each. When a test
fails, the mutant is **killed**. When every test still passes, the mutant **survived**: either the code has
behaviour no test checks, or the mutant is equivalent (it cannot change behaviour). The score is the share of
mutants that were killed.

## Why it helps

* **It proves the tests assert behaviour.** A test that runs code but checks nothing gives coverage and kills
  no mutants.
* **It finds untested branches and weak assertions.** A survivor points at an exact line and change, which
  usually shows the missing test case.
* **It protects refactoring.** A suite that kills mutants fails when a refactoring changes behaviour.
* **It gives a measurable bar.** Each scope has a break threshold; a run below it fails.

## Why generated registration uses it

The generated path must give exactly the store the reflection scan gives: the same features, reducers and
effects, in the same order. The dangerous bugs here are quiet ones. A generator that silently falls back to
reflection, or registers two reducers in a different order, still builds a working store, and ordinary tests
pass. Mutation testing checks that the equivalence and fallback tests notice such changes.

## Real examples

Survivors from the first runs, and the tests that now kill them:

* **Razor files and assemblies two references away.** A mutant in `RazorFileGuard` that changed how
  referenced assemblies are selected survived, and was first written off as equivalent. A review showed it hid
  a real gap: when library C has a component base class deriving from a class in library B, only B references
  Fluxor, so the guard skipped C, and a Razor file with `@inherits C.Derived` was not reported. The generated
  store would then have differed from the scan's. The guard now follows references transitively, and
  `RazorFileGuardTests.WhenRazorFileMentionsAClassWhoseAssemblyReachesFluxorOnlyThroughAnotherAssembly_ThenItIsReported`
  plus a fallback row that runs both stores kill the mutant.
* **Methods the scan does not register.** Mutants in the code that picks the methods to register survived,
  because no test had an attributed method that is internal, private or protected. Equivalence form 21 (`Form21_NonPublicMethods.cs`)
  has all four, and the generated path must register none of them, like the scan.
* **One diagnostic per problem.** Removing a `return false;` in `RegistrationModelBuilder` survived. The row
  "reducer whose state and action types generated code cannot name" in `GeneratedFallbackExecutionTests` now
  expects exactly one FLXG001.
* **The compact table.** Mutants in the table emitter survived until `GeneratorShapeTests` covered an assembly
  without features, the staticness of reducers and effects, and filling the type table at the chunk
  boundaries (1, 128, 129, 256 and 257 states).
* **A private constructor.** The runtime creates the generated registration with `nonPublic: true`. Mutating
  that survived until the hand-written test registration got a private constructor.

The survivors looked at so far were equivalent, for example whitespace in the generated code, `x >> 1`
becoming `x >>> 1` on values that are never negative, or `Delegate.CreateDelegate(type, null, method)` for a
static method binding the same delegate either way. Not every survivor of the latest run has been classified:
the generator run behind the score below had 48 survivors and 5 mutants without coverage. A fresh run lists
each of them, with its file, line and change, in the HTML report under the `-O` folder (see
[Running it](#running-it)). When a change moves the score, the new survivors are looked at and either killed
with a test or judged equivalent.

## Scores

| Scope | Config | Mutated code | Score | Thresholds (high / low / break) |
|---|---|---|---:|---|
| Generator | `Source/stryker-generator.json` | `Fluxor.SourceGenerator` | 94.00% | 95 / 90 / 90 |
| Runtime | `Source/stryker-runtime.json` | the registration code in `Fluxor/DependencyInjection` | 97.12% | 97 / 95 / 95 |

The first runs scored 74.69% (generator) and 76.89% (runtime). The dispatcher fix in
[#584](https://github.com/mrpmorris/Fluxor/pull/584) used the same approach.

## Running it

From the repository root (the configs use paths relative to it):

```
dotnet tool restore
dotnet stryker -f Source/stryker-generator.json -O <a folder outside the repository>
dotnet stryker -f Source/stryker-runtime.json -O <a folder outside the repository>
```

Pass `-O` with a folder outside the repository, so the reports do not land in the working tree. Each run
writes an HTML and a JSON report there. The Stryker version is pinned in `.config/dotnet-tools.json`.

It runs on demand, not in CI: a run takes about 15 minutes per scope, since every mutant is a test run of its
own, and the generator tests compile and load assemblies. Run it when you change the generator or the
registration code.

## Handling a surviving mutant

1. Open the HTML report and find the survivor: the file, the line and the change.
2. Ask what behaviour the change would break. If there is one, write a test that checks that behaviour (not the
   text of the code), see it fail with the mutant applied by hand, and keep it.
3. If the mutant cannot change behaviour (equivalent), leave it, and say why in the pull request. Be careful:
   the Razor example above was first thought equivalent and was not.
