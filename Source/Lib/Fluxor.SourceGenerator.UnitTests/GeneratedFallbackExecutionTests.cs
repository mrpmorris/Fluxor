using Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.SourceGenerator.UnitTests;

/// <summary>
/// For every reason the generator reports FLXG001 (one row each), the opted-in assembly compiles without a
/// generated registration and <c>AddFluxor</c> builds exactly the store the reflection scan builds: a fallback
/// is never a different store. Where the reflection scan itself throws, both runs throw the same exception.
/// </summary>
/// <remarks>
/// Every fixture compiles: FLXG001 is about code the generator cannot reproduce, not about invalid C#.
/// Each row is added to <see cref="Prelude"/> (a feature state, an action and a valid reducer, so the store is
/// never empty) and Fixtures/Shared.cs. Reasons that only invalid C# can produce are not rows; see the
/// remarks on <see cref="Triggers"/>.
/// </remarks>
[Collection(nameof(GeneratedRegistrationSwitchCollection))]
public class GeneratedFallbackExecutionTests
{
	private const string ReferencedAssemblyName = "Fluxor.SourceGenerator.ReferencedAssembly";
	private const string MessageSuffix = " cannot be registered by generated code; this assembly uses the reflection scan";
	private const string GeneratedFileName = "FluxorGeneratedRegistration.g.cs";

	private const string Usings = """
		using Fluxor;
		using System;
		using System.Collections.Generic;
		using System.Threading.Tasks;

		""";

	private const string Prelude = Usings + """
		namespace Ns
		{
			[FeatureState]
			public record S(int Value)
			{
				public S() : this(0) { }
			}

			public record PingAction;

			public class ValidReducers
			{
				[ReducerMethod]
				public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 1 };
			}

			public class ValidEffects
			{
				[EffectMethod]
				public static Task Handle(PingAction action, IDispatcher dispatcher)
				{
					dispatcher.Dispatch(new Shared.Logged("valid effect"));
					return Task.CompletedTask;
				}
			}
		}
		""";

	// Compiled into a second assembly that the fixture references
	private const string ReferencedSource = """
		using Fluxor;
		using System.Diagnostics.CodeAnalysis;

		namespace Referenced;

		public record RefState(int Value);
		public record RefAction;

		public class ExperimentalMethodBase
		{
			[Experimental("FLXTESTEXP1")]
			[ReducerMethod]
			public static RefState ReduceExperimental(RefState state, RefAction action) => state with { Value = state.Value + 1 };
		}

		[Experimental("FLXTESTEXP2")]
		public class ExperimentalBase
		{
			[ReducerMethod]
			public static RefState ReduceOnBase(RefState state, RefAction action) => state with { Value = state.Value * 3 };
		}
		""";

	private const string RefStateFeature = """
		public class RefStateFeature : Feature<Referenced.RefState>
		{
			public override string GetName() => "Ref";
			protected override Referenced.RefState GetInitialState() => new(2);
		}
		public record DispatchRefAction;
		public class Forwarder
		{
			[EffectMethod(typeof(DispatchRefAction))]
			public static Task Forward(IDispatcher dispatcher)
			{
				dispatcher.Dispatch(new Referenced.RefAction());
				return Task.CompletedTask;
			}
		}
		""";

	private const string ComponentWithReducers = """
		public class ComponentWithReducers
		{
			[ReducerMethod]
			public static S Reduce(S state, PingAction action) => state with { Value = state.Value * 2 };
		}
		""";

	// A Fluxor of a version without generated registration: the same names, none of the Generated types
	private const string FluxorWithoutGeneratedRegistrationSource = """
		using System;
		using System.Threading.Tasks;

		namespace Fluxor
		{
			public interface IFeature { }
			public interface IFeature<TState> : IFeature { }
			public interface IReducer<TState> { }
			public interface IEffect { }
			public interface IMiddleware { }
			public interface IDispatcher { void Dispatch(object action); }
			[AttributeUsage(AttributeTargets.Class)]
			public class FeatureStateAttribute : Attribute
			{
				public string Name { get; set; }
			}
			[AttributeUsage(AttributeTargets.Method)]
			public class ReducerMethodAttribute : Attribute { }
		}
		""";

	private const string StubFixtureSource = """
		using Fluxor;

		namespace Ns;

		[FeatureState(Name = "Stub")]
		public record S(int Value)
		{
			public S() : this(5) { }
		}

		public record PingAction;

		public class ValidReducers
		{
			[ReducerMethod]
			public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 1 };
		}
		""";

	private sealed record Trigger(
		string Source,
		string ExpectedMember,
		bool ReflectionThrows = false,
		string RazorPath = null,
		string RazorText = null,
		bool UsesReferencedAssembly = false,
		bool UsesReferenceChain = false,
		bool WithFluxorWithoutGeneratedRegistration = false,
		string At = null,
		int Count = 1);

	/// <summary>
	/// One row per reason for FLXG001 (FluxorRegistrationGenerator, RegistrationModelBuilder, RazorFileGuard).
	/// The key names the reason; <see cref="Trigger.ExpectedMember"/> is the start of the exact message and
	/// <see cref="Trigger.At"/> the source text the diagnostic points at ("" for no location), by default the
	/// member's own name. <see cref="Trigger.Count"/> is the number of FLXG001 diagnostics: one per problem.
	/// </summary>
	/// <remarks>
	/// Not rows, because no compiling C# reaches them: a type that does not compile (TypeKind.Error); a
	/// FeatureStateAttribute named argument other than its four properties, or of another type; a method
	/// name that is not a C# identifier; an open type parameter as a state or action type (only a generic
	/// method or a generic host has one, and both are reported or skipped before the type is looked at).
	/// </remarks>
	private static readonly Dictionary<string, Trigger> Triggers = new()
	{
		// FluxorRegistrationGenerator
		["referenced Fluxor has no generated registration support"] = new(
			StubFixtureSource,
			"This assembly (the referenced Fluxor does not support generated registration)",
			WithFluxorWithoutGeneratedRegistration: true,
			At: ""),

		// Types
		["generic type definition implementing IMiddleware"] = new(
			"namespace Mw { public class GenericMiddleware<T> : Middleware { } }",
			"Ns.Mw.GenericMiddleware<T> (a generic type definition implementing IMiddleware)"),
		["type with a role that is private"] = new(
			"""
			public class Outer
			{
				private class HiddenEffect : Effect<PingAction>
				{
					public override Task HandleAsync(PingAction action, IDispatcher dispatcher)
					{
						dispatcher.Dispatch(new Shared.Logged("hidden effect"));
						return Task.CompletedTask;
					}
				}
			}
			""",
			"Ns.Outer.HiddenEffect (is not accessible from generated code (private or protected))"),
		["type with a role that is file-local"] = new(
			"""
			file class FileLocalReducer : Reducer<S, PingAction>
			{
				public override S Reduce(S state, PingAction action) => state with { Value = state.Value * 10 };
			}
			""",
			"Ns.FileLocalReducer (is file-local)"),
		["type with a role that is obsolete as an error"] = new(
			"""
			namespace Mw
			{
				[Obsolete("gone", true)]
				public class ObsoleteMiddleware : Middleware { }
			}
			""",
			"Ns.Mw.ObsoleteMiddleware (is obsolete as an error or experimental)"),
		["implements IFeature<TState> twice"] = new(
			"""
			public class TwoFeatures : Feature<S>, IFeature<PingAction>
			{
				public override string GetName() => "Two";
				protected override S GetInitialState() => new(5);
				PingAction IFeature<PingAction>.State => null;
				void IFeature<PingAction>.AddReducer(IReducer<PingAction> reducer) { }
			}
			""",
			"Ns.TwoFeatures (implements IFeature<TState> more than once)",
			ReflectionThrows: true),
		["implements IReducer<TState> twice"] = new(
			"""
			public class TwoReducers : IReducer<S>, IReducer<Shared.LogState>
			{
				S IReducer<S>.Reduce(S state, object action) => state with { Value = state.Value * 10 };
				bool IReducer<S>.ShouldReduceStateForAction(object action) => action is PingAction;
				Shared.LogState IReducer<Shared.LogState>.Reduce(Shared.LogState state, object action) => new(state.Text + "[two]");
				bool IReducer<Shared.LogState>.ShouldReduceStateForAction(object action) => action is PingAction;
			}
			""",
			"Ns.TwoReducers (implements IReducer<TState> more than once)"),
		["feature class whose state type generated code cannot name"] = new(
			"""
			public class TupleFeature : Feature<(int, int)>
			{
				public override string GetName() => "Tuple";
				protected override (int, int) GetInitialState() => (1, 2);
			}
			""",
			"Ns.TupleFeature (state type (int, int) is a tuple type)"),
		["reducer class whose state type generated code cannot name"] = new(
			"""
			public class TupleReducer : IReducer<(int, int)>
			{
				public (int, int) Reduce((int, int) state, object action) => (state.Item1 + 1, state.Item2);
				public bool ShouldReduceStateForAction(object action) => action is PingAction;
			}
			""",
			"Ns.TupleReducer (state type (int, int) is a tuple type)"),

		// [FeatureState]
		["attribute derived from FeatureStateAttribute"] = new(
			"""
			public class MyFeatureStateAttribute : FeatureStateAttribute { }
			[MyFeatureState(Name = "Derived")]
			public record DerivedAttributeState(int Value)
			{
				public DerivedAttributeState() : this(3) { }
			}
			""",
			"Ns.DerivedAttributeState (uses an attribute derived from FeatureStateAttribute)"),
		["CreateInitialStateMethodName names no static method"] = new(
			"""[FeatureState(CreateInitialStateMethodName = "Nope")] public record NoFactory(int Value);""",
			"Ns.NoFactory.Nope (CreateInitialStateMethodName must name exactly one static method of the state)",
			ReflectionThrows: true,
			At: "NoFactory"),
		["CreateInitialStateMethodName names two static methods"] = new(
			"""
			[FeatureState(CreateInitialStateMethodName = "Create")]
			public record TwoFactories(int Value)
			{
				public static TwoFactories Create() => new(1);
				private static TwoFactories Create(int value) => new(value);
			}
			""",
			"Ns.TwoFactories.Create (CreateInitialStateMethodName must name exactly one static method of the state)",
			ReflectionThrows: true,
			At: "TwoFactories"),
		["initial state method with parameters"] = new(
			"""
			[FeatureState(CreateInitialStateMethodName = "Create")]
			public record WrongFactory(int Value)
			{
				public static WrongFactory Create(int value) => new(value);
			}
			""",
			"Ns.WrongFactory.Create (must be a parameterless method returning Ns.WrongFactory)",
			ReflectionThrows: true),
		["initial state method returning another type"] = new(
			"""
			[FeatureState(CreateInitialStateMethodName = "Create")]
			public record ObjectFactory(int Value)
			{
				public static object Create() => new ObjectFactory(4);
			}
			""",
			"Ns.ObjectFactory.Create (must be a parameterless method returning Ns.ObjectFactory)",
			ReflectionThrows: true),
		["[FeatureState] without a parameterless constructor"] = new(
			"[FeatureState] public record NoConstructor(int Value);",
			"Ns.NoConstructor (a [FeatureState] needs a parameterless constructor or a CreateInitialStateMethodName)",
			ReflectionThrows: true),

		// Attributed methods: how they are called
		["attributed operator"] = new(
			"""
			public class OperatorHost
			{
				[ReducerMethod]
				public static S operator +(S state, OperatorHost action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.OperatorHost.op_Addition (is not an ordinary method)",
			At: "+"),
		["generic method"] = new(
			"""
			public class GenericMethodHost
			{
				[ReducerMethod]
				public static S Reduce<T>(S state, T action) => state;
			}
			""",
			"Ns.GenericMethodHost.Reduce (is a generic method)",
			ReflectionThrows: true),
		["static abstract interface member"] = new(
			"""
			public interface IStaticAbstractHost
			{
				[ReducerMethod]
				static abstract S Reduce(S state, PingAction action);
			}
			""",
			"Ns.IStaticAbstractHost.Reduce (is a static abstract or virtual interface member)",
			ReflectionThrows: true),
		["instance method on an abstract type"] = new(
			"""
			public abstract class AbstractHost
			{
				[ReducerMethod]
				public S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.AbstractHost.Reduce (is an instance method on an abstract type, which the scan has no instance of)",
			ReflectionThrows: true),
		["instance method on a struct"] = new(
			"""
			public struct StructHost
			{
				[ReducerMethod]
				public S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.StructHost.Reduce (is an instance method on a struct)",
			ReflectionThrows: true),
		["ref parameter"] = new(
			"""
			public class RefHost
			{
				[ReducerMethod]
				public static S Reduce(ref S state, PingAction action) => state;
			}
			""",
			"Ns.RefHost.Reduce (has ref, in or out parameters or returns by reference)",
			ReflectionThrows: true),
		["ref return"] = new(
			"""
			public class RefReturnHost
			{
				private static S Current = new();

				[ReducerMethod]
				public static ref S Reduce(S state, PingAction action) => ref Current;
			}
			""",
			"Ns.RefReturnHost.Reduce (has ref, in or out parameters or returns by reference)",
			ReflectionThrows: true),
		["ref readonly return"] = new(
			"""
			public class RefReadonlyReturnHost
			{
				private static S Current = new();

				[ReducerMethod]
				public static ref readonly S Reduce(S state, PingAction action) => ref Current;
			}
			""",
			"Ns.RefReadonlyReturnHost.Reduce (has ref, in or out parameters or returns by reference)",
			ReflectionThrows: true),
		["method obsolete as an error"] = new(
			"""
			public class ObsoleteMethodHost
			{
				[Obsolete("gone", true)]
				[ReducerMethod]
				public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.ObsoleteMethodHost.Reduce (is obsolete as an error or experimental)"),
		["obsolete DiagnosticId that cannot be written in a pragma"] = new(
			"""
			public class PragmaHost
			{
				[Obsolete("old", DiagnosticId = "MY OBS")]
				[ReducerMethod]
				public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.PragmaHost.Reduce (is obsolete as an error or experimental)"),
		["empty obsolete DiagnosticId"] = new(
			"""
			public class EmptyDiagnosticIdHost
			{
				[Obsolete("old", DiagnosticId = "")]
				[ReducerMethod]
				public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.EmptyDiagnosticIdHost.Reduce (is obsolete as an error or experimental)"),
		["inherited method experimental in another assembly"] = new(
			RefStateFeature + Environment.NewLine + """
			#pragma warning disable FLXTESTEXP1
			public class ExperimentalMethodHost : Referenced.ExperimentalMethodBase { }
			#pragma warning restore FLXTESTEXP1
			""",
			"Ns.ExperimentalMethodHost.ReduceExperimental (is obsolete as an error or experimental)",
			UsesReferencedAssembly: true,
			At: "ExperimentalMethodHost"),
		["inherited method declared on a base type experimental in another assembly"] = new(
			RefStateFeature + Environment.NewLine + """
			#pragma warning disable FLXTESTEXP2
			public class ExperimentalBaseHost : Referenced.ExperimentalBase { }
			#pragma warning restore FLXTESTEXP2
			""",
			"Ns.ExperimentalBaseHost.ReduceOnBase (is declared on Referenced.ExperimentalBase, which is obsolete as an error or experimental)",
			UsesReferencedAssembly: true,
			At: "ExperimentalBaseHost"),
		["method on a private nested host"] = new(
			"""
			public class Outer
			{
				private class Inner
				{
					[ReducerMethod]
					public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
				}
			}
			""",
			"Ns.Outer.Inner.Reduce (is declared on a type that is not accessible from generated code (private or protected))"),
		["attribute derived from ReducerMethodAttribute"] = new(
			"""
			public class MyReducerAttribute : ReducerMethodAttribute { }
			public class DerivedReducerHost
			{
				[MyReducer]
				public static S Reduce(S state, PingAction action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.DerivedReducerHost.Reduce (uses an attribute derived from ReducerMethodAttribute)"),
		["attribute derived from EffectMethodAttribute"] = new(
			"""
			public class MyEffectAttribute : EffectMethodAttribute { }
			public class DerivedEffectHost
			{
				[MyEffect]
				public static Task Handle(PingAction action, IDispatcher dispatcher)
				{
					dispatcher.Dispatch(new Shared.Logged("derived attribute effect"));
					return Task.CompletedTask;
				}
			}
			""",
			"Ns.DerivedEffectHost.Handle (uses an attribute derived from EffectMethodAttribute)"),

		// [ReducerMethod] signature
		["reducer attribute given a null action type"] = new(
			"""
			public class NullReducerHost
			{
				[ReducerMethod(null)]
				public static S Reduce(S state) => state;
			}
			""",
			"Ns.NullReducerHost.Reduce (has a null action type)",
			ReflectionThrows: true),
		["reducer (state, action) with one parameter"] = new(
			"""
			public class OneParameterReducerHost
			{
				[ReducerMethod]
				public static S Reduce(S state) => state;
			}
			""",
			"Ns.OneParameterReducerHost.Reduce (must have 2 parameters (state, action))",
			ReflectionThrows: true),
		["reducer with the action type in the attribute and two parameters"] = new(
			"""
			public class TwoParameterReducerHost
			{
				[ReducerMethod(typeof(PingAction))]
				public static S Reduce(S state, PingAction action) => state;
			}
			""",
			"Ns.TwoParameterReducerHost.Reduce (must have 1 parameter (state) when the action type is in the attribute)",
			ReflectionThrows: true),
		["reducer not returning its state type"] = new(
			"""
			public class WrongReturnReducerHost
			{
				[ReducerMethod]
				public static object Reduce(S state, PingAction action) => state;
			}
			""",
			"Ns.WrongReturnReducerHost.Reduce (must return its state parameter's type)",
			ReflectionThrows: true),
		["reducer state type that generated code cannot name"] = new(
			"""
			public class TupleStateReducerHost
			{
				[ReducerMethod]
				public static (int, int) Reduce((int, int) state, PingAction action) => state;
			}
			""",
			"Ns.TupleStateReducerHost.Reduce (state type (int, int) is a tuple type)"),
		["reducer whose state and action types generated code cannot name"] = new(
			"""
			public class TupleStateAndActionReducerHost
			{
				[ReducerMethod]
				public static (int, int) Reduce((int, int) state, (string, string) action) => state;
			}
			""",
			"Ns.TupleStateAndActionReducerHost.Reduce (state type (int, int) is a tuple type)"),
		["reducer action type that is a static class"] = new(
			"""
			public static class StaticActionType { }
			public class StaticActionReducerHost
			{
				[ReducerMethod(typeof(StaticActionType))]
				public static S Reduce(S state) => state;
			}
			""",
			"Ns.StaticActionReducerHost.Reduce (action type Ns.StaticActionType is a static class)"),
		["reducer action type void"] = new(
			"""
			public class VoidActionReducerHost
			{
				[ReducerMethod(typeof(void))]
				public static S Reduce(S state) => state;
			}
			""",
			"Ns.VoidActionReducerHost.Reduce (action type void is void)",
			ReflectionThrows: true),
		["reducer action type that is a ref struct"] = new(
			"""
			public ref struct RefStructAction { }
			public class RefStructReducerHost
			{
				[ReducerMethod(typeof(RefStructAction))]
				public static S Reduce(S state) => state;
			}
			""",
			"Ns.RefStructReducerHost.Reduce (action type Ns.RefStructAction is a ref struct)",
			ReflectionThrows: true),
		["reducer action type that is an open generic type"] = new(
			"""
			public class OpenGenericReducerHost
			{
				[ReducerMethod(typeof(List<>))]
				public static S Reduce(S state) => state;
			}
			""",
			"Ns.OpenGenericReducerHost.Reduce (action type System.Collections.Generic.List<> is an open generic type)",
			ReflectionThrows: true),
		["reducer action type dynamic"] = new(
			"""
			public class DynamicReducerHost
			{
				[ReducerMethod]
				public static S Reduce(S state, dynamic action) => state with { Value = state.Value + 100 };
			}
			""",
			"Ns.DynamicReducerHost.Reduce (action type dynamic is a dynamic, pointer or function pointer type)"),
		["reducer action type that is an array of a tuple"] = new(
			"""
			public class TupleArrayReducerHost
			{
				[ReducerMethod]
				public static S Reduce(S state, (int, int)[] action) => state;
			}
			""",
			"Ns.TupleArrayReducerHost.Reduce (action type (int, int)[] is a tuple type)"),
		["reducer action type with a type argument generated code cannot name"] = new(
			"""
			public class TupleArgumentReducerHost
			{
				[ReducerMethod]
				public static S Reduce(S state, List<(int, int)> action) => state;
			}
			""",
			"Ns.TupleArgumentReducerHost.Reduce (action type System.Collections.Generic.List<(int, int)> has a type argument that is a tuple type)"),
		["reducer action type that is private"] = new(
			"""
			public class PrivateActionHost
			{
				[ReducerMethod(typeof(HiddenAction))]
				public static S Reduce(S state) => state with { Value = state.Value + 100 };
				private record HiddenAction;
			}
			""",
			"Ns.PrivateActionHost.Reduce (action type Ns.PrivateActionHost.HiddenAction is not accessible from generated code (private or protected))"),

		// [EffectMethod] signature
		["effect attribute given a null action type"] = new(
			"""
			public class NullEffectHost
			{
				[EffectMethod(null)]
				public static Task Handle(IDispatcher dispatcher) => Task.CompletedTask;
			}
			""",
			"Ns.NullEffectHost.Handle (has a null action type)",
			ReflectionThrows: true),
		["effect (action, dispatcher) with one parameter"] = new(
			"""
			public class OneParameterEffectHost
			{
				[EffectMethod]
				public static Task Handle(PingAction action) => Task.CompletedTask;
			}
			""",
			"Ns.OneParameterEffectHost.Handle (must have 2 parameters (action, IDispatcher))",
			ReflectionThrows: true),
		["effect with the action type in the attribute and two parameters"] = new(
			"""
			public class TwoParameterEffectHost
			{
				[EffectMethod(typeof(PingAction))]
				public static Task Handle(PingAction action, IDispatcher dispatcher) => Task.CompletedTask;
			}
			""",
			"Ns.TwoParameterEffectHost.Handle (must have 1 parameter (IDispatcher) when the action type is in the attribute)",
			ReflectionThrows: true),
		["effect whose last parameter is not an IDispatcher"] = new(
			"""
			public class NotDispatcherEffectHost
			{
				[EffectMethod]
				public static Task Handle(PingAction action, object dispatcher) => Task.CompletedTask;
			}
			""",
			"Ns.NotDispatcherEffectHost.Handle (its last parameter must be an IDispatcher)",
			ReflectionThrows: true),
		["effect not returning Task"] = new(
			"""
			public class ValueTaskEffectHost
			{
				[EffectMethod]
				public static ValueTask Handle(PingAction action, IDispatcher dispatcher) => default;
			}
			""",
			"Ns.ValueTaskEffectHost.Handle (must return Task)",
			ReflectionThrows: true),
		["effect action type that generated code cannot name"] = new(
			"""
			public class TupleEffectHost
			{
				[EffectMethod]
				public static Task Handle((int, int) action, IDispatcher dispatcher) => Task.CompletedTask;
			}
			""",
			"Ns.TupleEffectHost.Handle (action type (int, int) is a tuple type)"),

		// RazorFileGuard
		["Razor file that mentions a Fluxor registration type"] = new(
			ComponentWithReducers,
			"Pages/Index.razor (a Razor file that mentions ComponentWithReducers; the classes Razor generates are not visible to this generator)",
			RazorPath: "Pages/Index.razor",
			RazorText: "@inherits ComponentWithReducers\n<p>Hi</p>",
			At: ""),
		["Razor file that mentions a class whose assembly reaches Fluxor only through another assembly"] = new(
			"public class Plain { }",
			"Pages/Index.razor (a Razor file that mentions Derived; the classes Razor generates are not visible to this generator)",
			RazorPath: "Pages/Index.razor",
			RazorText: "@inherits C.Derived",
			UsesReferenceChain: true,
			At: ""),
		["code-behind partial class that a Razor file mentioning Fluxor completes"] = new(
			ComponentWithReducers + "public partial class Counter { }",
			"Ns.Counter (a partial class that may be completed by a Razor file that mentions Fluxor types)",
			RazorPath: "Pages/Counter.razor",
			RazorText: "@inherits ComponentWithReducers",
			Count: 2),
	};

	public static TheoryData<string> TriggerNames => new(Triggers.Keys);

	[Theory]
	[MemberData(nameof(TriggerNames))]
	public async Task WhenGeneratorReportsFLXG001_ThenStoreEqualsReflectionScan(string trigger)
	{
		Trigger row = Triggers[trigger];
		Compilation[] referenced = GetReferencedCompilations(row);
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = Compile(row, referenced);

		// FLXG001 for the trigger, and nothing generated
		Assert.Equal(row.Count, diagnostics.Length);
		Assert.All(diagnostics, x =>
		{
			Assert.Equal("FLXG001", x.Id);
			Assert.Equal(DiagnosticSeverity.Warning, x.Severity);
		});
		string expectedMessage = row.ExpectedMember + MessageSuffix;
		string[] messages = diagnostics.Select(x => x.GetMessage(CultureInfo.InvariantCulture)).ToArray();
		Assert.True(messages.Contains(expectedMessage), $"Expected \"{expectedMessage}\" in:\n{string.Join("\n", messages)}");
		Location location = diagnostics.First(x => x.GetMessage(CultureInfo.InvariantCulture) == expectedMessage).Location;
		string expectedAt = row.At ?? GetDefaultLocationText(row.ExpectedMember);
		if (expectedAt.Length == 0)
			Assert.Equal(Location.None, location);
		else
			Assert.Equal(expectedAt, location.SourceTree?.GetText().ToString(location.SourceSpan));
		Assert.DoesNotContain(output.SyntaxTrees, x => x.FilePath.EndsWith(GeneratedFileName, StringComparison.Ordinal));
		Assert.Empty(output.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error));

		// Reflection first, each run on its own freshly loaded copy of the assembly
		(RunResult reflection, Exception reflectionError) = await TryRunAsync(output, referenced, disableGenerated: true);
		(RunResult fallback, Exception fallbackError) = await TryRunAsync(output, referenced, disableGenerated: false);

		if (row.ReflectionThrows)
		{
			Assert.True(reflectionError is not null, "The reflection scan was expected to throw");
			Assert.True(fallbackError is not null, "The fallback was expected to throw like the reflection scan");
			Assert.Equal(Describe(reflectionError), Describe(fallbackError));
			return;
		}

		Assert.True(reflectionError is null, "The reflection scan threw " + reflectionError);
		Assert.True(fallbackError is null, "The fallback threw " + fallbackError);
		Assert.False(fallback.HasGeneratedRegistration);
		Assert.All(fallback.Shape.Wrappers, x => Assert.False(x.HasGeneratedHandler, x.Description));
		Assert.Contains(fallback.Shape.Wrappers, x => x.Description.Contains("Ns.ValidReducers.Reduce("));
		StoreShape.AssertEqual(reflection.Shape, fallback.Shape);
		Assert.Equal(reflection.DispatchLog, fallback.DispatchLog);
	}

	private static Compilation[] GetReferencedCompilations(Trigger row)
	{
		if (row.UsesReferencedAssembly)
			return new[] { GeneratorTestHost.CreateCompilation(new[] { ReferencedSource }, ReferencedAssemblyName) };
		if (row.UsesReferenceChain)
		{
			(Compilation b, Compilation c) = RazorFileGuardTests.CreateReferenceChain(
				"public class ReducerComponentBase { [ReducerMethod] public static RefState Reduce(RefState state, RefAction action) => state; }",
				"ReducerComponentBase");
			return new[] { b, c };
		}
		return Array.Empty<Compilation>();
	}

	private static (Compilation, ImmutableArray<Diagnostic>) Compile(Trigger row, Compilation[] referenced)
	{
		if (row.WithFluxorWithoutGeneratedRegistration)
		{
			Compilation oldFluxor = GeneratorTestHost.CreateCompilation(
				new[] { FluxorWithoutGeneratedRegistrationSource },
				assemblyName: "Fluxor",
				referenceFluxor: false);
			return GeneratorTestHost.Run(
				new[] { row.Source },
				"true",
				new[] { GeneratorTestHost.CreateReference(oldFluxor) },
				referenceFluxor: false);
		}

		string fixture = Usings + "namespace Ns\n{\n" + row.Source + "\n}\n";
		return GeneratorTestHost.Run(
			new[] { Prelude, FixtureRunner.ReadFixture("Shared.cs"), fixture },
			"true",
			referenced.Select(GeneratorTestHost.CreateReference).ToArray(),
			additionalTexts: row.RazorPath is null
				? null
				: new[] { new GeneratorTestHost.InMemoryAdditionalText(row.RazorPath, row.RazorText) });
	}

	private static async Task<(RunResult, Exception)> TryRunAsync(Compilation output, Compilation[] referenced, bool disableGenerated)
	{
		try
		{
			return (await FixtureRunner.RunAsync(output, referenced, middlewareToAdd: null, disableGenerated), null);
		}
		catch (Exception exception) when (exception is not Xunit.Sdk.XunitException)
		{
			return (null, exception);
		}
	}

	// "Ns.Outer.Inner.Reduce (...)" is reported at Reduce, "Ns.Mw.GenericMiddleware<T> (...)" at GenericMiddleware
	private static string GetDefaultLocationText(string expectedMember)
	{
		string member = expectedMember.Substring(0, expectedMember.IndexOf(" (", StringComparison.Ordinal));
		int genericStart = member.IndexOf('<');
		if (genericStart >= 0)
			member = member.Substring(0, genericStart);
		return member.Substring(member.LastIndexOf('.') + 1);
	}

	// The exception and every inner exception, by type and message
	private static string Describe(Exception exception)
	{
		var parts = new List<string>();
		for (Exception current = exception; current is not null; current = current.InnerException)
			parts.Add(current.GetType().FullName + ": " + current.Message);
		return string.Join(" ---> ", parts);
	}
}
