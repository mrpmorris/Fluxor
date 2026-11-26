using Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.SourceGenerator.UnitTests;

/// <summary>
/// For every form Fluxor discovers, the store built from generated registration equals the store built by the
/// reflection scan of the same assembly, and dispatching every action gives the same states.
/// </summary>
/// <remarks>
/// Each fixture (Fixtures/*.cs, embedded) is compiled with the generator, then loaded twice, each time into a
/// new collectible <see cref="System.Runtime.Loader.AssemblyLoadContext"/>: once for the reflection scan (switch on) and once for the
/// generated path (switch off). The reflection scan is the first thing to touch its freshly loaded types, so
/// the order of <c>GetMethods</c> is not influenced by earlier reflection calls on the same types.
/// Fixtures/Shared.cs is added to every fixture: its method-based reducer proves which path built the store.
/// </remarks>
[Collection(nameof(GeneratedRegistrationSwitchCollection))]
public class GeneratedEquivalenceTests
{
	private const string ReferencedAssemblyName = "Fluxor.SourceGenerator.ReferencedAssembly";
	private const string MiddlewareTypeName = "Fixture.WithMiddleware.SomeMiddleware";

	private sealed record Form(
		string[] Files,
		string[] ReferencedFiles = null,
		string MiddlewareToAdd = null,
		string[] Expected = null,
		string[] Unexpected = null);

	private static readonly Dictionary<string, Form> Forms = new()
	{
		["01 [FeatureState] record, public constructor"] = new(
			new[] { "Form01_PublicConstructor.cs" },
			Expected: new[]
			{
				"PublicCtor: Fixture.PublicCtorState = PublicCtorState { Count = 7 } (max 5/s, browsable False)", "PublicCtorClassState 3",
				"PublicCtorState { Count = 15 }", "PublicCtorClassState 24", "PublicCtorState { Count = 5 }", "PublicCtorClassState -24"
			}),
		["02 [FeatureState] private constructor"] = new(
			new[] { "Form02_PrivateConstructor.cs" },
			Expected: new[]
			{
				"PrivateCtorState { Count = 42 }", "= ProtectedCtorState initial",
				"PrivateCtorState { Count = 24 }", "PrivateCtorState { Count = 576 }", "ProtectedCtorState [initial+x]+other"
			}),
		["03 CreateInitialStateMethodName, public and private"] = new(
			new[] { "Form03_InitialStateMethod.cs" },
			Expected: new[]
			{
				"PublicFactoryState { Count = 1 }", "PrivateFactoryState { Count = 2 }",
				"PublicFactoryState { Count = 44 }", "PrivateFactoryState { Count = 13 }",
				"PublicFactoryState { Count = 43 }", "PrivateFactoryState { Count = 100 }"
			}),
		["04 required members"] = new(
			new[] { "Form04_RequiredMembers.cs" },
			Expected: new[]
			{
				"RequiredMemberState 0 null", "RequiredMemberRecord { Value = 0 }",
				"RequiredMemberState 2 n0", "RequiredMemberRecord { Value = 54 }",
				"RequiredMemberState 2 other", "RequiredMemberRecord { Value = -54 }"
			}),
		["05 Feature<T> class"] = new(
			new[] { "Form05_FeatureClass.cs" },
			Expected: new[] { "FeatureClass: Fixture.FeatureClassState = FeatureClassState { Name = initial }", "FeatureClassState { Name = set }" }),
		["06 static [ReducerMethod] (state, action)"] = new(
			new[] { "Form06_StaticReducerMethod.cs" },
			Expected: new[] { "CounterReducers.ReduceIncrement(", "StaticClassReducers.ReduceIncrementAgain(", "CounterState { Count = -27 }" },
			Unexpected: new[] { "NotAReducer" }),
		["07 instance [ReducerMethod(typeof(A))] (state)"] = new(
			new[] { "Form07_InstanceReducerMethod.cs" },
			Expected: new[] { "CounterReducers.ReduceIncrement(Fixture.CounterState)", "CounterState { Count = 12 }" }),
		["08 IReducer<T> class"] = new(
			new[] { "Form08_ReducerClass.cs" },
			Expected: new[] { "Fixture.IncrementReducer", "Fixture.DoubleReducer", "CounterState { Count = 1 }" },
			Unexpected: new[] { "AbstractReducer" }),
		["09 inherited reducer and effect from a base class in a second assembly without opt-in"] = new(
			new[] { "Form09_InheritedFromReferencedAssembly.cs" },
			ReferencedFiles: new[] { "Form09_Referenced.cs" },
			Expected: new[] { "Referenced.ReducerAndEffectBase.ReduceVirtual(", "base effect on InheritsOnly", "static base effect" }),
		["10 override of an attributed virtual method"] = new(
			new[] { "Form10_Override.cs" },
			Expected: new[] { "Fixture.OverridesWithAttribute.ReduceVirtual(", "Fixture.OverridesTwice.ReduceVirtual(", "override effect" },
			Unexpected: new[] { "unattributed" }),
		["11 new-hiding method"] = new(
			new[] { "Form11_NewHiding.cs" },
			Expected: new[] { "derived static;", "derived instance;", "derived effect" },
			Unexpected: new[] { "unattributed" }),
		["12 partial host across two files, A then B"] = new(
			new[] { "Form12_PartialA.cs", "Form12_PartialB.cs" },
			Expected: new[] { "PartialHost.ReduceA1(", "PartialHost.ReduceB2(" }),
		["12 partial host across two files, B then A"] = new(
			new[] { "Form12_PartialB.cs", "Form12_PartialA.cs" },
			Expected: new[] { "PartialHost.ReduceA1(", "PartialHost.ReduceB2(" }),
		["13 nested public types"] = new(
			new[] { "Form13_NestedTypes.cs" },
			Expected: new[]
			{
				"Fixture.Outer+Inner+Deeper.Reduce(", "Fixture.Outer+Inner+Deeper.ReduceAgain(", "Fixture.Outer.ReduceOnOuterAgain(",
				"nested effect", "[deeper first]", "[deeper second]",
				// Outer's two reducers, then Inner's, then Deeper's, each pair in declaration order
				"NestedState { Text = <({o;}i;)d;> }"
			}),
		["14 static [EffectMethod] (action, dispatcher)"] = new(
			new[] { "Form14_StaticEffectMethod.cs" },
			Expected: new[] { "[first]", "[first again]", "[second]" }),
		["15 instance [EffectMethod(typeof(A))] (dispatcher)"] = new(
			new[] { "Form15_InstanceEffectMethod.cs" },
			Expected: new[] { "[instance first]", "[instance first with action]", "[instance second]" }),
		["16 IEffect class"] = new(
			new[] { "Form16_EffectClass.cs" },
			Expected: new[] { "[object effect FirstAction]", "[typed effect]" },
			Unexpected: new[] { "AbstractEffect" }),
		["17 effect on a base action type and an interface action"] = new(
			new[] { "Form17_BaseAndInterfaceActions.cs" },
			Expected: new[] { "[base DerivedAction]", "[marker MarkerOnlyAction]", "[marker without action]", "[object PlainAction]" }),
		["18 two reducers for the same action on one state"] = new(
			new[] { "Form18_ReducerOrder.cs" },
			Expected: new[] { "TextState { Text = Z;A;M;B; }" }),
		["19 IMiddleware in a namespace with a reducer, not added with AddMiddleware"] = new(
			new[] { "Form19_MiddlewareNamespace.cs" },
			Expected: new[] { "outside;" },
			Unexpected: new[] { "WithMiddleware", "next to middleware" }),
		["19 IMiddleware in a namespace with a reducer, added with AddMiddleware"] = new(
			new[] { "Form19_MiddlewareNamespace.cs" },
			MiddlewareToAdd: MiddlewareTypeName,
			Expected: new[]
			{
				"Fixture.WithMiddleware.SomeMiddleware -> Fixture.WithMiddleware.SomeMiddleware",
				"outside;", "next to middleware;", "below middleware;", "[next to middleware]", "StateNextToMiddleware { Value = 9 }"
			}),
		["20 internal host class"] = new(
			new[] { "Form20_InternalHost.cs" },
			Expected: new[] { "Fixture.InternalHost.ReduceInstance(", "[internal effect]", "[internal effect class]" }),
		["21 attributed methods that are not public"] = new(
			new[] { "Form21_NonPublicMethods.cs" },
			Expected: new[] { "VisibilityState { Text = public; }" },
			Unexpected: new[] { "internal;", "private;", "protected", "private effect", "internal effect" }),
	};

	/// <summary>Every form.</summary>
	public static TheoryData<string> FormNames() => new(Forms.Keys);

	[Theory]
	[MemberData(nameof(FormNames))]
	public async Task WhenFormIsGenerated_ThenStoreEqualsReflectionScan(string form)
	{
		Form fixture = Forms[form];
		Compilation referenced = fixture.ReferencedFiles is null
			? null
			: GeneratorTestHost.CreateCompilation(fixture.ReferencedFiles.Select(ReadFixture).ToArray(), ReferencedAssemblyName);
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			fixture.Files.Append("Shared.cs").Select(ReadFixture).ToArray(),
			"true",
			referenced is null ? null : new[] { GeneratorTestHost.CreateReference(referenced) });

		Assert.Empty(diagnostics);
		Assert.Empty(output.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));

		// Reflection first, each run on its own freshly loaded copy of the assembly
		RunResult reflection = await FixtureRunner.RunAsync(output, referenced, fixture.MiddlewareToAdd, disableGenerated: true);
		RunResult generated = await FixtureRunner.RunAsync(output, referenced, fixture.MiddlewareToAdd, disableGenerated: false);

		// The fixture exercises its form
		string reflectionText = reflection.Shape + string.Join(Environment.NewLine, reflection.DispatchLog);
		foreach (string expected in fixture.Expected ?? Array.Empty<string>())
			Assert.Contains(expected, reflectionText);
		foreach (string unexpected in fixture.Unexpected ?? Array.Empty<string>())
			Assert.DoesNotContain(unexpected, reflectionText);

		// Each path was the one intended: a silent fallback to reflection would not pass
		Assert.True(generated.HasGeneratedRegistration, "The assembly has no generated registration");
		Assert.Contains(generated.Shape.Wrappers, x => x.Description.Contains("Shared.LogReducers.Reduce("));
		Assert.All(generated.Shape.Wrappers, x => Assert.True(x.HasGeneratedHandler, x.Description));
		Assert.Equal(generated.Shape.Wrappers.Count, reflection.Shape.Wrappers.Count);
		Assert.All(reflection.Shape.Wrappers, x => Assert.False(x.HasGeneratedHandler, x.Description));

		StoreShape.AssertEqual(reflection.Shape, generated.Shape);
		Assert.Equal(reflection.DispatchLog, generated.DispatchLog);
	}

	[Fact]
	public void WhenFixtureIsEmbedded_ThenAFormUsesIt()
	{
		string[] used = Forms.Values
			.SelectMany(x => x.Files.Concat(x.ReferencedFiles ?? Array.Empty<string>()))
			.Append("Shared.cs")
			.Distinct()
			.OrderBy(x => x, StringComparer.Ordinal)
			.ToArray();
		string[] embedded = typeof(GeneratedEquivalenceTests).Assembly.GetManifestResourceNames()
			.Where(x => x.StartsWith("Fixtures."))
			.Select(x => x.Substring("Fixtures.".Length))
			.OrderBy(x => x, StringComparer.Ordinal)
			.ToArray();

		Assert.Equal(embedded, used);
	}

	private static string ReadFixture(string fileName) => FixtureRunner.ReadFixture(fileName);
}
