using Fluxor.DependencyInjection;
using Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests;

/// <summary>
/// What the reflection scan registers and rejects. The generated registration must reproduce it exactly, so
/// these behaviours are pinned here.
/// </summary>
public class ReflectionScanTests
{
	private const string SupportNamespace = "Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests.SupportFiles";

	[Fact]
	public async Task WhenHostHasEveryMethodShape_ThenEachIsBoundToItsMethod()
	{
		string text = await DispatchScanActionAsync(typeof(ScanState), typeof(EffectLogReducer), typeof(AllReducerShapes), typeof(AllEffectShapes));

		Assert.Equal("sa;s;ia;i;x;[sa][s][ia][i]", text);
	}

	[Fact]
	public async Task WhenOverrideHasNoAttribute_ThenNeitherItNorTheAttributedBaseMethodIsRegistered()
	{
		string text = await DispatchScanActionAsync(typeof(ScanState), typeof(EffectLogReducer), typeof(OverridesWithoutAttributes));

		Assert.Equal("", text);
	}

	public static TheoryData<Type, Type, string> InvalidTypes => new()
	{
		{
			typeof(ReducerWithOneParameter), typeof(ArgumentException),
			"Method must have 2 parameters (state, action) when [ReducerMethodAttribute] has no ActionType specified. "
				+ MethodName(typeof(ReducerWithOneParameter), "Reduce") + " (Parameter 'MethodInfo')"
		},
		{
			typeof(ReducerWithActionTypeAndTwoParameters), typeof(ArgumentException),
			"Method must have 1 parameter (state) when [ReducerMethodAttribute] has an ActionType specified. "
				+ MethodName(typeof(ReducerWithActionTypeAndTwoParameters), "Reduce") + " (Parameter 'methodInfo')"
		},
		{
			typeof(ReducerReturningAnotherType), typeof(ArgumentException),
			"Expected reducer method to return type System.Object. "
				+ MethodName(typeof(ReducerReturningAnotherType), "Reduce") + " (Parameter 'methodInfo')"
		},
		{
			typeof(EffectWithOneParameter), typeof(ArgumentException),
			"Method must have 2 parameters (action, IDispatcher) when [EffectMethodAttribute] has no ActionType specified. "
				+ MethodName(typeof(EffectWithOneParameter), "Handle") + " (Parameter 'MethodInfo')"
		},
		{
			typeof(EffectWithActionTypeAndTwoParameters), typeof(ArgumentException),
			"Method must have 1 parameter (IDispatcher) when [EffectMethodAttribute] has an ActionType specified. "
				+ MethodName(typeof(EffectWithActionTypeAndTwoParameters), "Handle") + " (Parameter 'methodInfo')"
		},
		{
			typeof(EffectWithoutDispatcher), typeof(ArgumentException),
			"The last parameter of a method should be an IDispatcher when decorated with an [EffectMethodAttribute]. "
				+ MethodName(typeof(EffectWithoutDispatcher), "Handle") + " (Parameter 'methodInfo')"
		},
		{
			typeof(EffectNotReturningTask), typeof(ArgumentException),
			"Effect methods must have a return type of Task. "
				+ MethodName(typeof(EffectNotReturningTask), "Handle") + " (Parameter 'methodInfo')"
		},
		{
			typeof(StateWithoutConstructor), typeof(ArgumentException),
			typeof(StateWithoutConstructor).FullName + " doesn't have a public or non-public parameterless constructor."
				+ " Either create one, or specify a static factory method name using the CreateInitialStateMethodName"
				+ " property on the  FeatureStateAttribute"
		},
		{
			typeof(StateWithFactoryWithParameter), typeof(InvalidOperationException),
			FactoryMessage(typeof(StateWithFactoryWithParameter), "Create")
		},
		{
			typeof(StateWithFactoryReturningObject), typeof(InvalidOperationException),
			FactoryMessage(typeof(StateWithFactoryReturningObject), "Create")
		},
		{
			typeof(StateWithMissingFactory), typeof(InvalidOperationException),
			FactoryMessage(typeof(StateWithMissingFactory), "Missing")
		},
	};

	[Theory]
	[MemberData(nameof(InvalidTypes))]
	public void WhenTypeIsInvalid_ThenAddFluxorThrowsWithAMessageNamingIt(Type invalidType, Type exceptionType, string message)
	{
		var services = new ServiceCollection();

		Exception exception = Record.Exception(() => services.AddFluxor(x => x.ScanTypes(typeof(ScanState), invalidType)));

		Assert.NotNull(exception);
		Assert.Equal(exceptionType, exception.GetType());
		Assert.Equal(message, exception.Message);
	}

	[Fact]
	public void WhenNamespaceHasMiddleware_ThenItsTypesAreFilteredOutUnlessIncluded()
	{
		Type[] types = { typeof(ScanState), typeof(Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests.ReflectionScanTests) };
		var exclude = new[] { new AssemblyScanSettings(typeof(ScanState).Assembly, SupportNamespace) };

		Type[] withoutInclude = AssemblyScanSettings.FilterClasses(types, exclude, Array.Empty<AssemblyScanSettings>());
		Type[] withInclude = AssemblyScanSettings.FilterClasses(types, exclude, exclude);

		Assert.Equal(new[] { typeof(ReflectionScanTests) }, withoutInclude);
		Assert.Equal(types, withInclude);
	}

	[Theory]
	[InlineData(SupportNamespace, true)]
	[InlineData("FLUXOR.UNITTESTS.DependencyInjectionTests.ReflectionScanTests.SUPPORTFILES", true)]
	[InlineData("Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests", true)]
	[InlineData("Fluxor.UnitTests.DependencyInjectionTests.ReflectionScanTests.Support", false)]
	[InlineData(SupportNamespace + ".ScanState", false)]
	public void WhenSettingsHaveNamespace_ThenTheyMatchTypesInItAndItsChildNamespaces(string @namespace, bool expected)
	{
		var settings = new AssemblyScanSettings(typeof(ScanState).Assembly, @namespace);

		Assert.Equal(expected, settings.Matches(typeof(ScanState)));
	}

	[Fact]
	public void WhenSettingsHaveNoNamespace_ThenTheyMatchEveryTypeOfTheirAssemblyOnly()
	{
		var settings = new AssemblyScanSettings(typeof(ScanState).Assembly);

		Assert.True(settings.Matches(typeof(ScanState)));
		Assert.False(settings.Matches(typeof(IStore)));
	}

	[Fact]
	public void WhenSettingsHaveTheSameAssemblyAndNamespace_ThenTheyAreEqual()
	{
		var settings = new AssemblyScanSettings(typeof(ScanState).Assembly, SupportNamespace);
		var same = new AssemblyScanSettings(typeof(ScanState).Assembly, SupportNamespace);

		Assert.True(settings.Equals(same));
		Assert.Equal(settings.GetHashCode(), same.GetHashCode());
		Assert.Single(new[] { settings, same }.Distinct());
	}

	[Fact]
	public void WhenSettingsDiffer_ThenTheyAreNotEqual()
	{
		var settings = new AssemblyScanSettings(typeof(ScanState).Assembly, SupportNamespace);

		Assert.False(settings.Equals(null));
		Assert.False(settings.Equals("not settings"));
		Assert.False(settings.Equals(new AssemblyScanSettings(typeof(IStore).Assembly, SupportNamespace)));
		Assert.False(settings.Equals(new AssemblyScanSettings(typeof(ScanState).Assembly, "Other")));
		Assert.False(new AssemblyScanSettings(typeof(ScanState).Assembly, null).Equals(settings));
	}

	[Fact]
	public void WhenNothingIsGivenToScan_ThenAddFluxorThrows()
	{
		var services = new ServiceCollection();

		var exception = Assert.Throws<ArgumentException>(() => services.AddFluxor(x => { }));

		Assert.StartsWith("Must supply either ", exception.Message);
	}

	[Fact]
	public void WhenFeatureStateInfoIsGivenNull_ThenItThrows()
	{
		Assert.Equal(
			"generatedFeatureState",
			Assert.Throws<ArgumentNullException>(() => new FeatureStateInfo(generatedFeatureState: null)).ParamName);
		Assert.Equal(
			"featureStateAttribute",
			Assert.Throws<ArgumentNullException>(() => new FeatureStateInfo(null, typeof(ScanState))).ParamName);
		Assert.Equal(
			"stateType",
			Assert.Throws<ArgumentNullException>(() => new FeatureStateInfo(new FeatureStateAttribute(), null)).ParamName);
	}

	private static async Task<string> DispatchScanActionAsync(params Type[] typesToScan)
	{
		var services = new ServiceCollection();
		services.AddFluxor(x => x.ScanTypes(typesToScan[0], typesToScan.Skip(1).ToArray()));
		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		using IServiceScope scope = serviceProvider.CreateScope();
		var store = scope.ServiceProvider.GetRequiredService<IStore>();
		await store.InitializeAsync();

		scope.ServiceProvider.GetRequiredService<IDispatcher>().Dispatch(new ScanAction());
		return scope.ServiceProvider.GetRequiredService<IState<ScanState>>().Value.Text;
	}

	private static string MethodName(Type type, string methodName) =>
		$"Method \"{methodName}\" on class \"{type.FullName}\"";

	private static string FactoryMessage(Type stateType, string methodName) =>
		$"{stateType.FullName}.{methodName} must be a parameterless method, and return type {stateType.FullName}";
}
