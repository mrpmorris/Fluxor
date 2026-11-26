using Fluxor.DependencyInjection.Generated;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace Fluxor.SourceGenerator.UnitTests;

/// <summary>
/// Reads the generated file of a compilation: which methods and lambdas it emits.
/// </summary>
public static class GeneratedShape
{
	public const string GeneratedFileName = "FluxorGeneratedRegistration.g.cs";

	public static SyntaxTree GetTree(Compilation output) =>
		Assert.Single(output.SyntaxTrees, x => x.FilePath.EndsWith(GeneratedFileName, StringComparison.Ordinal));

	/// <summary>The names of the methods declared in the generated file.</summary>
	public static string[] GetMethodNames(Compilation output) =>
		GetTree(output).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.Text).ToArray();

	/// <summary>The lambdas in the generated file (each one is a method once compiled).</summary>
	public static int CountLambdas(Compilation output) =>
		GetTree(output).GetRoot().DescendantNodes().OfType<LambdaExpressionSyntax>().Count();
}

/// <summary>
/// The method count and chunking of the generated registration.
/// What the generated code registers is proven by <see cref="GeneratedEquivalenceTests"/>.
/// </summary>
public class GeneratorShapeTests
{
	/// <summary>
	/// The table, one method filling its types, one factory per host (reducer or effect), one switch each for feature
	/// creation, initial state and AddReducer, no lambdas.
	/// </summary>
	[Fact]
	public void WhenRegistrationIsGenerated_ThenOneMethodIsEmittedPerHostAndNoLambdas()
	{
		string source = """
			using Fluxor;
			using System.Threading.Tasks;
			namespace Ns;
			[FeatureState] public record S1(int Value) { public S1() : this(1) { } }
			[FeatureState] public record S2(int Value) { public S2() : this(2) { } }
			public record A1;
			public record A2;
			public class Feature3 : Feature<S1> { public override string GetName() => "F"; protected override S1 GetInitialState() => new(3); }
			public static class Reducers
			{
				[ReducerMethod] public static S1 R1(S1 s, A1 a) => s;
				[ReducerMethod] public static S1 R2(S1 s, A2 a) => s;
				[ReducerMethod(typeof(A1))] public static S2 R3(S2 s) => s;
			}
			public class Effects
			{
				[EffectMethod] public Task E1(A1 a, IDispatcher d) => Task.CompletedTask;
				[EffectMethod(typeof(A2))] public Task E2(IDispatcher d) => Task.CompletedTask;
			}
			""";

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source }, "true");
		Assert.Empty(diagnostics);

		Assert.Equal(
			new[] { "CreateTypes", "CreateTypeTable", "AddTypes0", "CreateReducers3", "CreateEffects4", "CreateFeature", "CreateInitialState", "AddReducer" },
			GeneratedShape.GetMethodNames(output));
		Assert.Equal(0, GeneratedShape.CountLambdas(output));
	}

	/// <summary>Each state is two types in the table: the state and its IFeature of TState.</summary>
	[Theory]
	[InlineData(1, 1)]
	[InlineData(128, 1)]
	[InlineData(129, 2)]
	[InlineData(256, 2)]
	[InlineData(257, 3)]
	public void WhenRegistrationHasManyTypes_ThenAtMost256TypesAreFilledInPerMethod(int stateCount, int expectedChunks)
	{
		var source = new StringBuilder("using Fluxor; namespace Ns;\n");
		for (int i = 0; i < stateCount; i++)
			source.Append("[FeatureState] public record S").Append(i).Append("(int Value) { public S").Append(i).Append("() : this(").Append(i).Append(") { } }\n");

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source.ToString() }, "true");
		Assert.Empty(diagnostics);

		Assert.Equal(expectedChunks, GeneratedShape.GetMethodNames(output).Count(x => x.StartsWith("AddTypes", StringComparison.Ordinal)));
		// Each type is filled in once
		int typeAssignments = GeneratedShape.GetTree(output).GetRoot().DescendantNodes()
			.OfType<MethodDeclarationSyntax>()
			.Where(x => x.Identifier.Text.StartsWith("AddTypes", StringComparison.Ordinal))
			.Sum(x => x.Body.Statements.Count);
		Assert.Equal(stateCount * 2, typeAssignments);
		IReadOnlyList<GeneratedTypeEntry> entries = GetEntries(output);
		Assert.Equal(stateCount, entries.Count);
		Assert.All(entries, x => Assert.NotNull(x));
		foreach (GeneratedTypeEntry entry in entries)
		{
			int index = int.Parse(entry.Type.Name.Substring(1));
			Assert.Equal(
				entry.Type.Name + " { Value = " + index + " }",
				entry.FeatureState.CreateFeature(entry.FeatureState.StateIndex).GetState().ToString());
		}
	}

	[Theory]
	[InlineData(1)]
	[InlineData(300)]
	public void WhenHostHasManyReducers_ThenOneFactoryCreatesEachByIndex(int reducerCount)
	{
		var source = new StringBuilder("using Fluxor; namespace Ns;\n[FeatureState] public record S(int Value) { public S() : this(0) { } }\npublic static class Host {\n");
		for (int i = 0; i < reducerCount; i++)
			source.Append("[ReducerMethod(typeof(int))] public static S R").Append(i).Append("(S s) => s with { Value = ").Append(i).Append(" };\n");
		source.Append("}\n");

		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source.ToString() }, "true");
		Assert.Empty(diagnostics);

		Assert.Single(GeneratedShape.GetMethodNames(output), x => x.StartsWith("CreateReducers", StringComparison.Ordinal));
		GeneratedTypeEntry host = Assert.Single(GetEntries(output), x => x.Type.Name == "Host");
		Assert.Equal(reducerCount, host.ReducerMethods.Count);
		for (int i = 0; i < reducerCount; i++)
		{
			GeneratedReducerMethod reducer = host.ReducerMethods[i];
			Assert.Equal(i, reducer.Index);
			Assert.True(reducer.IsStatic);
			Assert.Equal("S", reducer.StateType.Name);
			object created = reducer.CreateReducer(null, reducer.Index);
			Assert.Equal("R" + i, GetHandlerName(created));
		}
	}

	[Fact]
	public void WhenFactoriesGetAnUnknownIndex_ThenTheyThrowArgumentOutOfRange()
	{
		string source = """
			using Fluxor;
			using System.Threading.Tasks;
			namespace Ns;
			[FeatureState] public record S(int Value) { public S() : this(1) { } }
			public static class Host
			{
				[ReducerMethod(typeof(int))] public static S R(S s) => s;
				[EffectMethod(typeof(int))] public static Task E(IDispatcher d) => Task.CompletedTask;
			}
			""";
		(Compilation output, _) = GeneratorTestHost.Run(new[] { source }, "true");
		IReadOnlyList<GeneratedTypeEntry> entries = GetEntries(output);
		GeneratedTypeEntry host = Assert.Single(entries, x => x.Type.Name == "Host");
		GeneratedFeatureState state = Assert.Single(entries, x => x.FeatureState is not null).FeatureState;

		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => host.ReducerMethods[0].CreateReducer(null, 1)).ParamName);
		Assert.Equal("index", Assert.Throws<ArgumentOutOfRangeException>(() => host.EffectMethods[0].CreateEffect(null, 1)).ParamName);
		Assert.Equal("stateIndex", Assert.Throws<ArgumentOutOfRangeException>(() => state.CreateFeature(state.StateIndex + 1)).ParamName);
		Assert.Equal("stateIndex", Assert.Throws<ArgumentOutOfRangeException>(() => state.AddReducer(null, null, state.StateIndex + 1)).ParamName);

		// The initial state switch is reached through the feature
		Type registrationType = GetRegistrationType(output);
		MethodInfo createInitialState = registrationType.GetMethod("CreateInitialState", BindingFlags.Static | BindingFlags.NonPublic);
		var exception = Assert.Throws<TargetInvocationException>(() => createInitialState.Invoke(null, new object[] { state.StateIndex + 1 }));
		Assert.Equal("stateIndex", Assert.IsType<ArgumentOutOfRangeException>(exception.InnerException).ParamName);
	}

	[Fact]
	public void WhenFeatureClassAndFeatureStateShareAStateType_ThenTheyShareAStateIndex()
	{
		string source = """
			using Fluxor;
			namespace Ns;
			[FeatureState] public record S(int Value) { public S() : this(1) { } }
			[FeatureState] public record T(int Value) { public T() : this(2) { } }
			public class SFeature : Feature<S> { public override string GetName() => "SF"; protected override S GetInitialState() => new(3); }
			public class UFeature : Feature<string> { public override string GetName() => "UF"; protected override string GetInitialState() => "u"; }
			""";
		(Compilation output, _) = GeneratorTestHost.Run(new[] { source }, "true");
		Dictionary<string, GeneratedTypeEntry> entries = GetEntries(output).ToDictionary(x => x.Type.Name);

		int s = entries["S"].FeatureState.StateIndex;
		int t = entries["T"].FeatureState.StateIndex;
		Assert.Equal(s, entries["SFeature"].FeatureClass.StateIndex);
		int u = entries["UFeature"].FeatureClass.StateIndex;
		Assert.Equal(3, new[] { s, t, u }.Distinct().Count());
		Assert.Equal(new[] { 0, 1, 2 }, new[] { s, t, u }.OrderBy(x => x));
	}

	[Fact]
	public void WhenAssemblyHasNoFeatures_ThenNoFeatureMethodsAreEmittedAndMethodsKeepTheirStaticness()
	{
		string source = """
			using Fluxor;
			using System.Threading.Tasks;
			namespace Ns;
			public record A;
			public class Effects
			{
				[EffectMethod] public static Task Static(A a, IDispatcher d) => Task.CompletedTask;
				[EffectMethod] public Task Instance(A a, IDispatcher d) => Task.CompletedTask;
			}
			""";
		(Compilation output, ImmutableArray<Diagnostic> diagnostics) = GeneratorTestHost.Run(new[] { source }, "true");
		Assert.Empty(diagnostics);

		Assert.DoesNotContain(GeneratedShape.GetMethodNames(output), x => x.Contains("Feature") || x.Contains("AddReducer") || x.Contains("InitialState"));
		GeneratedTypeEntry effects = Assert.Single(GetEntries(output));
		Assert.Equal(new[] { true, false }, effects.EffectMethods.Select(x => x.IsStatic));
		Assert.Equal(
			new[] { "Static", "Instance" },
			effects.EffectMethods.Select(x => GetHandlerName(x.CreateEffect(x.IsStatic ? null : Activator.CreateInstance(effects.Type), x.Index))));
	}

	private static IReadOnlyList<GeneratedTypeEntry> GetEntries(Compilation output)
	{
		var registration = (IFluxorGeneratedRegistration)Activator.CreateInstance(GetRegistrationType(output));
		return registration.Types;
	}

	private static Type GetRegistrationType(Compilation output)
	{
		Assembly assembly = GeneratorTestHost.LoadAssembly(output);
		return assembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>().RegistrationType;
	}

	private static string GetHandlerName(object wrapper)
	{
		FieldInfo handlerField = wrapper.GetType().GetField("Handler", BindingFlags.Instance | BindingFlags.NonPublic);
		return ((Delegate)handlerField.GetValue(wrapper)).Method.Name;
	}
}
