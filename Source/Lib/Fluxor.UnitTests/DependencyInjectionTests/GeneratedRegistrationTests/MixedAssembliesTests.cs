using Fluxor.DependencyInjection;
using Fluxor.DependencyInjection.Generated;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Generated = Fluxor.UnitTests.GeneratedSample;
using Scanned = Fluxor.UnitTests.ScannedSample;

namespace Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;

/// <summary>
/// Real sample projects: one opts in to generated registration through MSBuild, the other does not.
/// </summary>
[Collection(nameof(GeneratedRegistrationSwitchCollection))]
public class MixedAssembliesTests
{
	private const string DisableSwitchName = "Fluxor.DisableGeneratedRegistration";
	private static readonly Assembly GeneratedAssembly = typeof(Generated.Included.GeneratedCounterState).Assembly;
	private static readonly Assembly ScannedAssembly = typeof(Scanned.Included.ScannedCounterState).Assembly;

	[Fact]
	public void WhenSampleOptsIn_ThenAssemblyHasGeneratedRegistrationAttribute()
	{
		var attribute = GeneratedAssembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>();

		Assert.NotNull(attribute);
		Assert.Equal(FluxorGeneratedRegistrationFormat.Current, attribute.FormatVersion);
		Assert.True(typeof(IFluxorGeneratedRegistration).IsAssignableFrom(attribute.RegistrationType));
	}

	[Fact]
	public void WhenSampleDoesNotOptIn_ThenNoAttribute()
	{
		Assert.Null(ScannedAssembly.GetCustomAttribute<FluxorGeneratedRegistrationAttribute>());
	}

	[Fact]
	public void WhenReducerAndStateAreInDifferentAssemblies_ThenBothPathsAttachIt()
	{
		void Configure(FluxorOptions x) => x
			.ScanAssemblies(ScannedAssembly, GeneratedAssembly)
			// A reducer in this (reflection scanned) assembly for a state of the generated assembly
			.ScanTypes(typeof(ReducerForGeneratedState));

		(StoreShape reflection, StoreShape generated) = CaptureBothPaths(Configure);

		StoreShape.AssertEqual(reflection, generated);
		// Reducer in the generated assembly for a state in the scanned one
		Assert.Contains(reflection.Reducers, x => x.Contains("GeneratedReducersOfScannedState.Reduce("));
		// Reducer in the scanned (this) assembly for a state in the generated one
		Assert.Contains(reflection.Reducers, x => x.Contains("ReducerForGeneratedState.Reduce("));
		// Each side's own reducers
		Assert.Contains(reflection.Reducers, x => x.Contains("ScannedReducers.Reduce("));
		Assert.Contains(reflection.Reducers, x => x.Contains(".GeneratedReducers.Reduce("));
		AssertWrappersBuiltBy(generated, "GeneratedSample", generatedHandler: true);
		AssertWrappersBuiltBy(generated, "ReducerForGeneratedState", generatedHandler: false);
		AssertWrappersBuiltBy(generated, "ScannedSample", generatedHandler: false);
		AssertWrappersBuiltBy(reflection, "", generatedHandler: false);
	}

	[Fact]
	public void WhenScanAssembliesHasNamespaceFilter_ThenExcludedNamespaceIsNotRegistered()
	{
		void Configure(FluxorOptions x) => x.ScanAssemblies(ScannedAssembly, GeneratedAssembly);

		(StoreShape reflection, StoreShape generated) = CaptureBothPaths(Configure);

		StoreShape.AssertEqual(reflection, generated);
		Assert.DoesNotContain(reflection.Features, x => x.Contains("Excluded"));
		Assert.DoesNotContain(reflection.Reducers, x => x.Contains(".Excluded."));
		Assert.DoesNotContain(reflection.Effects, x => x.Contains(".Excluded."));
		Assert.Contains(reflection.Features, x => x.Contains("GeneratedCounterState"));
		Assert.Contains(reflection.Features, x => x.Contains("ScannedCounterState"));
		Assert.Contains(reflection.Effects, x => x.Contains("GeneratedEffects.OnIncrement("));
		Assert.Contains(reflection.Effects, x => x.Contains("ScannedEffects.OnIncrement("));
		Assert.Contains(reflection.Effects, x => x.StartsWith(typeof(Generated.Included.GeneratedEffect).FullName));
		AssertWrappersBuiltBy(generated, "GeneratedSample", generatedHandler: true);
		AssertWrappersBuiltBy(generated, "ScannedSample", generatedHandler: false);
	}

	[Fact]
	public void WhenMiddlewareNamespaceExcluded_ThenItsReducersAreNotRegisteredUnlessMiddlewareAdded()
	{
		void WithoutMiddleware(FluxorOptions x) => x.ScanAssemblies(GeneratedAssembly, ScannedAssembly);
		void WithMiddleware(FluxorOptions x) => x
			.ScanAssemblies(GeneratedAssembly, ScannedAssembly)
			.AddMiddleware<Generated.Excluded.GeneratedMiddleware>()
			.AddMiddleware<Scanned.Excluded.ScannedMiddleware>();

		(StoreShape reflectionWithout, StoreShape generatedWithout) = CaptureBothPaths(WithoutMiddleware);
		(StoreShape reflectionWith, StoreShape generatedWith) = CaptureBothPaths(WithMiddleware);

		StoreShape.AssertEqual(reflectionWithout, generatedWithout);
		StoreShape.AssertEqual(reflectionWith, generatedWith);

		Assert.DoesNotContain(reflectionWithout.Reducers, x => x.Contains("GeneratedExcludedReducers"));
		Assert.DoesNotContain(reflectionWithout.Reducers, x => x.Contains("ScannedExcludedReducers"));
		Assert.Contains(reflectionWith.Reducers, x => x.Contains("GeneratedExcludedReducers.Reduce("));
		Assert.Contains(reflectionWith.Reducers, x => x.Contains("ScannedExcludedReducers.Reduce("));
		Assert.Contains(reflectionWith.Features, x => x.Contains("GeneratedExcludedState"));
		Assert.Contains(reflectionWith.Features, x => x.Contains("ScannedExcludedState"));
		Assert.Contains(generatedWith.Wrappers, x => CalledMethod(x.Description).Contains("GeneratedExcludedReducers") && x.HasGeneratedHandler);
		Assert.Contains(generatedWith.Wrappers, x => CalledMethod(x.Description).Contains("ScannedExcludedReducers") && !x.HasGeneratedHandler);
	}

	[Fact]
	public void WhenScanTypesNamesATypeFromAGeneratedAssembly_ThenOnlyThatTypeIsRegistered()
	{
		void Configure(FluxorOptions x) => x.ScanTypes(typeof(Generated.Included.GeneratedReducers));

		(StoreShape reflection, StoreShape generated) = CaptureBothPaths(Configure);

		StoreShape.AssertEqual(reflection, generated);
		Assert.Empty(reflection.Reducers);
		Assert.Empty(reflection.Effects);
		Assert.Empty(reflection.Features);
		Assert.DoesNotContain(generated.Wrappers, x => !x.HasGeneratedHandler);
	}

	private static (StoreShape reflection, StoreShape generated) CaptureBothPaths(Action<FluxorOptions> configure)
	{
		StoreShape reflection = Capture(disabled: true, configure);
		StoreShape generated = Capture(disabled: false, configure);
		return (reflection, generated);
	}

	private static StoreShape Capture(bool disabled, Action<FluxorOptions> configure)
	{
		bool hadValue = AppContext.TryGetSwitch(DisableSwitchName, out bool previous);
		AppContext.SetSwitch(DisableSwitchName, disabled);
		try
		{
			var services = new ServiceCollection();
			services.AddFluxor(configure);
			return StoreShape.Capture(services);
		}
		finally
		{
			AppContext.SetSwitch(DisableSwitchName, hadValue && previous);
		}
	}

	/// <summary>Every wrapper whose description contains the text has (or lacks) a generated handler, and there is at least one.</summary>
	private static void AssertWrappersBuiltBy(StoreShape shape, string text, bool generatedHandler)
	{
		var matching = shape.Wrappers.Where(x => CalledMethod(x.Description).Contains(text)).ToArray();
		Assert.NotEmpty(matching);
		Assert.All(matching, x => Assert.Equal(generatedHandler, x.HasGeneratedHandler));
	}

	// "Wrapper<State, Action> calls Declaring.Method(Parameters)": only the called method says where the code is declared
	private static string CalledMethod(string description) =>
		description.Substring(
			description.IndexOf(" calls ", StringComparison.Ordinal),
			description.IndexOf('(', description.IndexOf(" calls ", StringComparison.Ordinal)) - description.IndexOf(" calls ", StringComparison.Ordinal));

	public record ScannedSideAction;

	public class ReducerForGeneratedState
	{
		[ReducerMethod]
		public static Generated.Included.GeneratedCounterState Reduce(
			Generated.Included.GeneratedCounterState state,
			ScannedSideAction action) =>
			state with { Count = state.Count + 100 };
	}
}
