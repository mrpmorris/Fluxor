using Fluxor.DependencyInjection.Generated;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Fluxor.SourceGenerator.UnitTests;

public class GeneratorDiagnosticsTests
{
	private const string GeneratedFileName = "FluxorGeneratedRegistration.g.cs";

	private const string ReferencedBaseSource = """
		using Fluxor;
		using System.Threading.Tasks;

		namespace Referenced;

		public record BaseState(int Value);
		public record BaseAction;
		public record BaseEffectAction;

		public abstract class ReducerAndEffectBase
		{
			[ReducerMethod]
			public static BaseState ReduceBase(BaseState state, BaseAction action) => state with { Value = state.Value + 1 };

			[EffectMethod]
			public Task HandleBase(BaseEffectAction action, IDispatcher dispatcher) => Task.CompletedTask;

			[ReducerMethod(typeof(BaseAction))]
			public virtual BaseState ReduceVirtual(BaseState state) => state;
		}
		""";

	private const string AllFormsSourceA = """
		using Fluxor;
		using System.Threading.Tasks;

		namespace Forms
		{
			public record ActionA;
			public record ActionB;
			public interface IMarkerAction { }
			public record ActionWithInterface : IMarkerAction;
			public record DerivedAction : ActionA;

			// 1 public constructor
			[FeatureState(Name = "PublicCtor", MaximumStateChangedNotificationsPerSecond = 5, DebuggerBrowsable = false)]
			public record PublicCtorState(int Count)
			{
				public PublicCtorState() : this(0) { }
			}

			// 2 private constructor
			[FeatureState]
			public record PrivateCtorState(int Count)
			{
				private PrivateCtorState() : this(0) { }
			}

			// 3 factory methods, public and private
			[FeatureState(CreateInitialStateMethodName = nameof(Create))]
			public record PublicFactoryState(int Count)
			{
				public static PublicFactoryState Create() => new(1);
			}

			[FeatureState(CreateInitialStateMethodName = "Create")]
			public record PrivateFactoryState(int Count)
			{
				private static PrivateFactoryState Create() => new(2);
			}

			// 4 required members
			[FeatureState]
			public class RequiredMemberState
			{
				public required int Count { get; init; }
			}

			// 5 Feature<T> class
			public record FeatureClassState(string Name);
			public class FeatureClassFeature : Feature<FeatureClassState>
			{
				public override string GetName() => "FeatureClass";
				protected override FeatureClassState GetInitialState() => new("x");
			}

			// 6 static reducer (state, action), 7 instance reducer (state) with action type, 18 two reducers same action
			public class Reducers
			{
				[ReducerMethod]
				public static PublicCtorState ReduceA(PublicCtorState state, ActionA action) => state with { Count = state.Count + 1 };

				[ReducerMethod(typeof(ActionB))]
				public PublicCtorState ReduceB(PublicCtorState state) => state with { Count = 0 };

				[ReducerMethod]
				public static PublicCtorState ReduceASecond(PublicCtorState state, ActionA action) => state with { Count = state.Count * 2 };
			}

			// 8 IReducer<T> classes
			public class ClassReducer : IReducer<PrivateCtorState>
			{
				public PrivateCtorState Reduce(PrivateCtorState state, object action) => state;
				public bool ShouldReduceStateForAction(object action) => action is ActionA;
			}

			public class GenericBaseReducer : Reducer<PrivateCtorState, ActionB>
			{
				public override PrivateCtorState Reduce(PrivateCtorState state, ActionB action) => state;
			}

			// 9 only inherits from a base class in another assembly
			public class InheritsOnly : Referenced.ReducerAndEffectBase { }

			// 10 override of an attributed virtual method
			public class Overrides : Referenced.ReducerAndEffectBase
			{
				[ReducerMethod(typeof(Referenced.BaseAction))]
				public override Referenced.BaseState ReduceVirtual(Referenced.BaseState state) => state;
			}

			// 11 new-hiding
			public class HidesBase
			{
				[ReducerMethod]
				public static PublicCtorState Reduce(PublicCtorState state, ActionA action) => state;
			}

			public class HidesDerived : HidesBase
			{
				[ReducerMethod]
				public static new PublicCtorState Reduce(PublicCtorState state, ActionA action) => state;
			}

			// 12 partial across two files
			public partial class PartialHost
			{
				[ReducerMethod]
				public static PublicCtorState ReducePartialA(PublicCtorState state, ActionA action) => state;
			}

			// 13 nested public types
			public class Outer
			{
				public class Inner
				{
					[ReducerMethod]
					public static PublicCtorState ReduceNested(PublicCtorState state, ActionA action) => state;
				}

				[FeatureState]
				public record NestedState(int X)
				{
					public NestedState() : this(0) { }
				}
			}

			// 14 static effect (action, dispatcher), 15 instance effect (dispatcher) with action type, 17 base/interface actions
			public class Effects
			{
				[EffectMethod]
				public static Task HandleA(ActionA action, IDispatcher dispatcher) => Task.CompletedTask;

				[EffectMethod(typeof(ActionB))]
				public Task HandleB(IDispatcher dispatcher) => Task.CompletedTask;

				[EffectMethod]
				public Task HandleMarker(IMarkerAction action, IDispatcher dispatcher) => Task.CompletedTask;

				[EffectMethod]
				public static Task HandleObject(object action, IDispatcher dispatcher) => Task.CompletedTask;
			}

			// 16 IEffect classes
			public class ClassEffect : IEffect
			{
				public Task HandleAsync(object action, IDispatcher dispatcher) => Task.CompletedTask;
				public bool ShouldReactToAction(object action) => true;
			}

			public class GenericBaseEffect : Effect<ActionA>
			{
				public override Task HandleAsync(ActionA action, IDispatcher dispatcher) => Task.CompletedTask;
			}

			// 20 internal host
			internal class InternalHost
			{
				[ReducerMethod]
				public static PublicCtorState ReduceInternal(PublicCtorState state, ActionA action) => state;
			}

			// generic type definitions are skipped
			public class GenericHost<T>
			{
				[ReducerMethod]
				public static PublicCtorState Ignored(PublicCtorState state, ActionA action) => state;
			}

			// abstract host with static methods
			public abstract class AbstractHost
			{
				[EffectMethod]
				public static Task HandleAbstract(ActionA action, IDispatcher dispatcher) => Task.CompletedTask;
			}

			public static class StaticHost
			{
				[ReducerMethod]
				public static PublicCtorState @event(PublicCtorState state, ActionA action) => state;
			}
		}

		// 19 middleware in a namespace that also contains a reducer
		namespace Forms.WithMiddleware
		{
			public class SomeMiddleware : Middleware { }

			public class ReducersNextToMiddleware
			{
				[ReducerMethod]
				public static PublicCtorState Reduce(PublicCtorState state, ActionA action) => state;
			}
		}
		""";

	private const string AllFormsSourceB = """
		using Fluxor;

		namespace Forms;

		public partial class PartialHost
		{
			[ReducerMethod]
			public static PublicCtorState ReducePartialB(PublicCtorState state, ActionB action) => state;
		}
		""";

	[Theory]
	[InlineData(null)]
	[InlineData("false")]
	[InlineData("")]
	[InlineData("yes")]
	public void WhenPropertyIsNotTrue_ThenNothingIsGenerated(string optInValue)
	{
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) =
			GeneratorTestHost.Run(new[] { AllFormsSourceB, AllFormsSourceA }, optInValue, new[] { CreateReferencedBase() });

		Assert.Empty(diagnostics);
		Assert.DoesNotContain(output.SyntaxTrees, IsGeneratedFile);
	}

	[Theory]
	[InlineData("true")]
	[InlineData("True ")]
	[InlineData(" TRUE")]
	public void WhenPropertyIsTrueInAnyCaseOrSpacing_ThenRegistrationIsGenerated(string optInValue)
	{
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) =
			GeneratorTestHost.Run(new[] { AllFormsSourceB, AllFormsSourceA }, optInValue, new[] { CreateReferencedBase() });

		Assert.Empty(diagnostics);
		Assert.Contains(output.SyntaxTrees, IsGeneratedFile);
	}

	[Fact]
	public void WhenOptedIn_ThenGeneratedCodeCompilesWithoutDiagnostics()
	{
		Compilation referencedBase = CreateReferencedBaseCompilation();
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { AllFormsSourceA, AllFormsSourceB },
			"true",
			new[] { GeneratorTestHost.CreateReference(referencedBase) });

		Assert.Empty(diagnostics);
		Assert.Empty(output.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));
		string generated = GetGeneratedSource(output);
		Assert.Contains(
			"[assembly: global::Fluxor.DependencyInjection.Generated.FluxorGeneratedRegistration("
				+ "typeof(global::Fluxor.SourceGenerator.TestAssembly.FluxorGenerated.FluxorGeneratedRegistration), 1)]",
			generated);

		Assembly assembly = GeneratorTestHost.LoadAssembly(output, referencedBase);
		Dictionary<string, GeneratedTypeEntry> entries = GetEntries(assembly);

		Assert.Equal(
			new[]
			{
				"Forms.AbstractHost", "Forms.ClassEffect", "Forms.ClassReducer", "Forms.Effects", "Forms.FeatureClassFeature",
				"Forms.GenericBaseEffect", "Forms.GenericBaseReducer", "Forms.HidesBase", "Forms.HidesDerived",
				"Forms.InheritsOnly", "Forms.InternalHost", "Forms.Outer+Inner", "Forms.Outer+NestedState",
				"Forms.Overrides", "Forms.PartialHost", "Forms.PrivateCtorState", "Forms.PrivateFactoryState",
				"Forms.PublicCtorState", "Forms.PublicFactoryState", "Forms.Reducers", "Forms.RequiredMemberState",
				"Forms.StaticHost", "Forms.WithMiddleware.ReducersNextToMiddleware", "Forms.WithMiddleware.SomeMiddleware"
			},
			entries.Keys.OrderBy(x => x, StringComparer.Ordinal));

		// Inherited methods are listed on the type that inherits them
		GeneratedTypeEntry inheritsOnly = entries["Forms.InheritsOnly"];
		Assert.Equal(new[] { "ReduceBase", "ReduceVirtual" }, GetReducerMethodNames(inheritsOnly));
		Assert.Equal(new[] { "HandleBase" }, GetEffectMethodNames(inheritsOnly));

		// Declared methods first, then the base chain; an override replaces the base method
		GeneratedTypeEntry overrides = entries["Forms.Overrides"];
		Assert.Equal(new[] { "ReduceVirtual", "ReduceBase" }, GetReducerMethodNames(overrides));

		// new-hiding: both methods, as GetMethods returns both
		Assert.Equal(2, entries["Forms.HidesDerived"].ReducerMethods.Count);
		Assert.Equal(new[] { "ReducePartialA", "ReducePartialB" }, GetReducerMethodNames(entries["Forms.PartialHost"]));
		Assert.Equal(new[] { "ReduceA", "ReduceB", "ReduceASecond" }, GetReducerMethodNames(entries["Forms.Reducers"]));

		Assert.True(entries["Forms.WithMiddleware.SomeMiddleware"].IsMiddleware);
		Assert.True(entries["Forms.ClassEffect"].IsEffectClass);
		Assert.True(entries["Forms.GenericBaseEffect"].IsEffectClass);
		Assert.Equal("PrivateCtorState", entries["Forms.ClassReducer"].ReducerClassStateType.Name);
		Assert.Equal("FeatureClassState", entries["Forms.FeatureClassFeature"].FeatureClass.StateType.Name);
		// The action type is known to the effect the descriptor creates
		GeneratedEffectMethod effect = entries["Forms.Effects"].EffectMethods[1];
		object effectHost = effect.IsStatic ? null : Activator.CreateInstance(entries["Forms.Effects"].Type);
		Assert.Equal("ActionB", effect.CreateEffect(effectHost, effect.Index).GetType().GetGenericArguments()[0].Name);

		// Every state can be created, whichever way its initial state is made
		Assert.Equal("PublicCtor", CreateState(entries["Forms.PublicCtorState"], out object publicCtorState));
		Assert.Equal("PublicCtorState { Count = 0 }", publicCtorState.ToString());
		CreateState(entries["Forms.PrivateCtorState"], out object privateCtorState);
		Assert.Equal("PrivateCtorState { Count = 0 }", privateCtorState.ToString());
		CreateState(entries["Forms.PublicFactoryState"], out object publicFactoryState);
		Assert.Equal("PublicFactoryState { Count = 1 }", publicFactoryState.ToString());
		CreateState(entries["Forms.PrivateFactoryState"], out object privateFactoryState);
		Assert.Equal("PrivateFactoryState { Count = 2 }", privateFactoryState.ToString());
		CreateState(entries["Forms.RequiredMemberState"], out object requiredMemberState);
		Assert.NotNull(requiredMemberState);
		Assert.Equal("Forms.Outer+NestedState", CreateState(entries["Forms.Outer+NestedState"], out _));
	}

	[Fact]
	public void WhenPrivateNestedHost_ThenFLXG001AndNoAttribute()
	{
		const string source = """
			using Fluxor;
			namespace Ns;
			public record S;
			public record A;
			public class Outer
			{
				private class Inner
				{
					[ReducerMethod]
					public static S Reduce(S state, A action) => state;
				}
			}
			""";

		AssertFallback(source, "Outer.Inner.Reduce");
	}

	[Fact]
	public void WhenGenericMethod_ThenFLXG001AndNoAttribute()
	{
		const string source = """
			using Fluxor;
			namespace Ns;
			public record S;
			public class Host
			{
				[ReducerMethod]
				public static S Reduce<T>(S state, T action) => state;
			}
			""";

		AssertFallback(source, "Host.Reduce");
	}

	[Theory]
	[InlineData("[ReducerMethod] public static S Reduce(S state) => state;")]
	[InlineData("[ReducerMethod(typeof(A))] public static S Reduce(S state, A action) => state;")]
	[InlineData("[ReducerMethod] public static object Reduce(S state, A action) => state;")]
	[InlineData("[ReducerMethod] public static S Reduce(ref S state, A action) => state;")]
	[InlineData("[EffectMethod] public static Task Reduce(A action) => Task.CompletedTask;")]
	[InlineData("[EffectMethod] public static Task Reduce(A action, object dispatcher) => Task.CompletedTask;")]
	[InlineData("[EffectMethod] public static ValueTask Reduce(A action, IDispatcher dispatcher) => default;")]
	[InlineData("[EffectMethod(typeof(A))] public static Task Reduce(A action, IDispatcher dispatcher) => Task.CompletedTask;")]
	public void WhenWrongSignature_ThenFLXG001AndNoAttribute(string member)
	{
		string source = """
			using Fluxor;
			using System.Threading.Tasks;
			namespace Ns;
			public record S;
			public record A;
			public class Host
			{
				MEMBER
			}
			""".Replace("MEMBER", member);

		AssertFallback(source, "Host.Reduce");
	}

	[Theory]
	[InlineData("[FeatureState] public record Missing(int X);")]
	[InlineData("[FeatureState] public class Missing { public Missing(int x = 0) { } }")]
	[InlineData("[FeatureState] public static class Missing { }")]
	[InlineData("[FeatureState(CreateInitialStateMethodName = \"Nope\")] public record Missing(int X);")]
	[InlineData("[FeatureState(CreateInitialStateMethodName = \"Create\")] public record Missing(int X) { public static Missing Create(int x) => new(x); }")]
	[InlineData("[FeatureState(CreateInitialStateMethodName = \"Create\")] public record Missing(int X) { public static object Create() => new Missing(1); }")]
	[InlineData("[FeatureState(CreateInitialStateMethodName = \"Create\")] public record Missing(int X) { public static Missing Create() => new(1); private static Missing Create(int x) => new(x); }")]
	public void WhenMissingStateConstructor_ThenFLXG001AndNoAttribute(string member)
	{
		AssertFallback("using Fluxor;\nnamespace Ns;\n" + member, "Missing");
	}

	[Fact]
	public void WhenStateHasPrivateConstructor_ThenGeneratedWithoutFLXG001()
	{
		const string source = """
			using Fluxor;
			namespace Ns;
			[FeatureState]
			public record S(int X)
			{
				private S() : this(42) { }
			}
			""";

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source }, optIn: true);

		Assert.Empty(diagnostics);
		Assert.Empty(output.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning));
		Assert.Contains("global::System.Activator.CreateInstance(typeof(global::Ns.S), nonPublic: true)", GetGeneratedSource(output));
		Dictionary<string, GeneratedTypeEntry> entries = GetEntries(GeneratorTestHost.LoadAssembly(output));
		CreateState(entries["Ns.S"], out object state);
		Assert.Equal("S { X = 42 }", state.ToString());
	}

	[Theory]
	[InlineData("public class Host { [ReducerMethod] public static S Reduce(S state, A action) => state; private record A; }", "Host.Reduce")]
	[InlineData("public abstract class Host { [ReducerMethod] public S Reduce(S state, A action) => state; }", "Host.Reduce")]
	[InlineData("public struct Host { [ReducerMethod] public S Reduce(S state, A action) => state; }", "Host.Reduce")]
	[InlineData("public interface IHost { [ReducerMethod] S Reduce(S state, A action); }", "IHost.Reduce")]
	[InlineData("public class Host { [ReducerMethod] public static S operator +(S state, Host action) => state; }", "Host.op_Addition")]
	[InlineData("public class Host { [System.Obsolete(\"x\", true)] [ReducerMethod] public static S Reduce(S state, A action) => state; }", "Host.Reduce")]
	[InlineData("public class GenericMiddleware<T> : Middleware { }", "GenericMiddleware")]
	[InlineData("public class Outer<T> { public class NestedMiddleware : Middleware { } }", "NestedMiddleware")]
	[InlineData("public class Outer { private class HiddenEffect : IEffect { public Task HandleAsync(object a, IDispatcher d) => Task.CompletedTask; public bool ShouldReactToAction(object a) => true; } }", "Outer.HiddenEffect")]
	[InlineData("public class Outer { private class HiddenMiddleware : Middleware { } }", "Outer.HiddenMiddleware")]
	[InlineData("public class TwoFeatures : Feature<S>, IFeature<A> { public override string GetName() => \"x\"; protected override S GetInitialState() => new S(); }", "TwoFeatures")]
	[InlineData("public class MyReducerAttribute : ReducerMethodAttribute { } public class Host { [MyReducer] public static S Reduce(S state, A action) => state; }", "Host.Reduce")]
	public void WhenFormCannotBeGenerated_ThenFLXG001AndNoAttribute(string member, string expectedName)
	{
		string source = """
			using Fluxor;
			using System.Threading.Tasks;
			namespace Ns;
			public record S;
			public record A;
			""" + "\n" + member;

		AssertFallback(source, expectedName);
	}

	private const string RazorSupportSource = """
		using Fluxor;
		using System.Threading.Tasks;
		namespace Ns;
		public record S;
		public record A;
		public class ComponentWithReducers
		{
			[ReducerMethod]
			public static S Reduce(S state, A action) => state;
		}
		public class ComponentInheritingReducers : ComponentWithReducers { }
		public interface IMyEffect : IEffect { }
		public class HarmlessComponentBase { }
		""";

	[Theory]
	[InlineData("Pages/Counter.razor", "@code { [EffectMethod] public static Task Handle(A action, IDispatcher dispatcher) => Task.CompletedTask; }", "EffectMethod")]
	[InlineData("Pages/Counter.razor", "@inherits ComponentWithReducers\n<p>Hi</p>", "ComponentWithReducers")]
	[InlineData("Pages/Counter.razor", "@inherits Ns.ComponentInheritingReducers", "ComponentInheritingReducers")]
	[InlineData("Pages/Counter.razor", "@implements IMyEffect\n@code { public Task HandleAsync(object a, IDispatcher d) => Task.CompletedTask; public bool ShouldReactToAction(object a) => true; }", "IMyEffect")]
	[InlineData("Pages/_Imports.razor", "@using Ns\n@inherits ComponentWithReducers", "ComponentWithReducers")]
	[InlineData("Pages/Counter.razor", "@code { public class R : Reducer <S, A> { public override S Reduce(S s, A a) => s; } }", "Reducer")]
	[InlineData("Pages/Counter.cshtml", "@code { [FeatureState] public record NestedState; }", "FeatureState")]
	public void WhenRazorFileMentionsFluxor_ThenFLXG001AndNoAttribute(string path, string razorText, string expectedName)
	{
		var razor = new GeneratorTestHost.InMemoryAdditionalText(path, razorText);

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { RazorSupportSource },
			"true",
			additionalTexts: new[] { razor });

		Diagnostic diagnostic = Assert.Single(diagnostics);
		Assert.Equal("FLXG001", diagnostic.Id);
		Assert.Contains(path, diagnostic.GetMessage());
		Assert.Contains("mentions " + expectedName + ";", diagnostic.GetMessage());
		Assert.DoesNotContain(output.SyntaxTrees, IsGeneratedFile);
	}

	[Theory]
	[InlineData("Pages/Counter.razor")]
	[InlineData("Pages/_Imports.razor")]
	public void WhenRazorFileCompletesACodeBehindPartialWithFluxorBase_ThenFLXG001NamesThePartialClass(string path)
	{
		var razor = new GeneratorTestHost.InMemoryAdditionalText(path, "@inherits ComponentWithReducers");
		const string codeBehind = "namespace Ns; public partial class Counter { }";

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { RazorSupportSource, codeBehind },
			"true",
			additionalTexts: new[] { razor });

		Assert.All(diagnostics, x => Assert.Equal("FLXG001", x.Id));
		Assert.Contains(diagnostics, x => x.GetMessage().Contains(path));
		Assert.Contains(diagnostics, x => x.GetMessage().Contains("Ns.Counter (a partial class"));
		Assert.DoesNotContain(output.SyntaxTrees, IsGeneratedFile);
	}

	[Theory]
	[InlineData("<p>@Count</p>")]
	[InlineData("@inherits HarmlessComponentBase\n@inject IState<S> State\n@inject IDispatcher Dispatcher")]
	[InlineData("@code { private List<Reducers> items; private string Effect = \"x\"; }")]
	public void WhenRazorFileDoesNotMentionFluxorRegistrationTypes_ThenRegistrationIsGenerated(string razorText)
	{
		var razor = new GeneratorTestHost.InMemoryAdditionalText("Pages/Counter.razor", razorText);
		const string codeBehind = "namespace Ns; public partial class Counter { }";

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(
			new[] { RazorSupportSource, codeBehind },
			"true",
			additionalTexts: new[] { razor });

		Assert.Empty(diagnostics);
		Assert.Contains(output.SyntaxTrees, IsGeneratedFile);
	}

	[Fact]
	public void WhenObsoleteMembersHaveCustomDiagnosticIds_ThenGeneratedCodeDisablesThem()
	{
		const string source = """
			using Fluxor;
			using System;
			using System.Threading.Tasks;
			namespace Ns;
			[Obsolete("s", DiagnosticId = "MYOBS1")]
			[FeatureState]
			public record S
			{
				[Obsolete("c", DiagnosticId = "MYOBS2")]
				public S() { }
			}
			[Obsolete("a", DiagnosticId = "MYOBS3")]
			public record A;
			public class Host
			{
				[Obsolete("m", DiagnosticId = "MYOBS4")]
				[ReducerMethod]
				public static S Reduce(S state, A action) => state;

				[Obsolete("e")]
				[EffectMethod]
				public Task Handle(A action, IDispatcher dispatcher) => Task.CompletedTask;
			}
			""";

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source }, optIn: true);

		Assert.Empty(diagnostics);
		string generated = GetGeneratedSource(output);
		Assert.Contains("#pragma warning disable CS0612, CS0618, MYOBS1, MYOBS2, MYOBS3, MYOBS4 ", generated);
		Assert.Empty(output.GetDiagnostics().Where(x =>
			x.Severity >= DiagnosticSeverity.Warning && x.Location.SourceTree?.FilePath.EndsWith(GeneratedFileName) == true));
	}

	[Fact]
	public void WhenObsoleteDiagnosticIdCannotBeWrittenInAPragma_ThenFLXG001()
	{
		AssertFallback("""
			using Fluxor;
			namespace Ns;
			public record S;
			public record A;
			public class Host
			{
				[System.Obsolete("m", DiagnosticId = "MY OBS")]
				[ReducerMethod]
				public static S Reduce(S state, A action) => state;
			}
			""", "Host.Reduce");
	}

	private static void AssertFallback(string source, string expectedName)
	{
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source }, optIn: true);

		Assert.NotEmpty(diagnostics);
		Assert.All(diagnostics, x =>
		{
			Assert.Equal("FLXG001", x.Id);
			Assert.Equal(DiagnosticSeverity.Warning, x.Severity);
		});
		Assert.Contains(diagnostics, x => x.GetMessage().Contains(expectedName));
		Assert.Contains(diagnostics, x => x.GetMessage().Contains("cannot be registered by generated code; this assembly uses the reflection scan"));
		Assert.DoesNotContain(output.SyntaxTrees, IsGeneratedFile);
	}

	private static bool IsGeneratedFile(SyntaxTree tree) => tree.FilePath.EndsWith(GeneratedFileName, StringComparison.Ordinal);

	private static string GetGeneratedSource(Compilation output) =>
		Assert.Single(output.SyntaxTrees, IsGeneratedFile).ToString();

	private static Compilation CreateReferencedBaseCompilation() =>
		GeneratorTestHost.CreateCompilation(new[] { ReferencedBaseSource }, assemblyName: "Fluxor.SourceGenerator.TestBase");

	private static MetadataReference CreateReferencedBase() =>
		GeneratorTestHost.CreateReference(CreateReferencedBaseCompilation());

	private static Dictionary<string, GeneratedTypeEntry> GetEntries(Assembly assembly)
	{
		FluxorGeneratedRegistrationAttribute attribute = assembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>();
		Assert.NotNull(attribute);
		Assert.Equal(FluxorGeneratedRegistrationFormat.Current, attribute.FormatVersion);
		var registration = (IFluxorGeneratedRegistration)Activator.CreateInstance(attribute.RegistrationType);
		Assert.Same(registration.Types, registration.Types);
		return registration.Types.ToDictionary(x => x.Type.FullName);
	}

	private static string[] GetReducerMethodNames(GeneratedTypeEntry entry) =>
		entry.ReducerMethods.Select(x => GetDelegateMethodName(entry, x.IsStatic, host => x.CreateReducer(host, x.Index))).ToArray();

	private static string[] GetEffectMethodNames(GeneratedTypeEntry entry) =>
		entry.EffectMethods.Select(x => GetDelegateMethodName(entry, x.IsStatic, host => x.CreateEffect(host, x.Index))).ToArray();

	// The wrappers keep the bound delegate in a non-public field; its Method is the real method
	private static string GetDelegateMethodName(GeneratedTypeEntry entry, bool isStatic, Func<object, object> create)
	{
		object host = isStatic ? null : Activator.CreateInstance(entry.Type);
		object wrapper = create(host);
		FieldInfo handlerField = wrapper.GetType().GetField("Handler", BindingFlags.Instance | BindingFlags.NonPublic);
		var handler = (Delegate)handlerField.GetValue(wrapper);
		return handler.Method.Name;
	}

	private static string CreateState(GeneratedTypeEntry entry, out object state)
	{
		IFeature feature = entry.FeatureState.CreateFeature(entry.FeatureState.StateIndex);
		state = feature.GetState();
		return feature.GetName();
	}
}
