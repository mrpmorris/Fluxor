using Fluxor.DependencyInjection;
using Fluxor.DependencyInjection.Generated;
using Fluxor.UnitTests.HandWrittenRegistration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;

[CollectionDefinition(nameof(GeneratedRegistrationSwitchCollection), DisableParallelization = true)]
public class GeneratedRegistrationSwitchCollection { }

[Collection(nameof(GeneratedRegistrationSwitchCollection))]
public class GeneratedRegistrationRuntimeTests
{
	private const string DisableSwitchName = "Fluxor.DisableGeneratedRegistration";
	private static readonly Assembly FixtureAssembly = typeof(HandWrittenFluxorRegistration).Assembly;

	[Theory]
	[InlineData(StoreLifetime.Scoped)]
	[InlineData(StoreLifetime.Singleton)]
	public void WhenAssemblyHasGeneratedRegistration_ThenStoreEqualsReflectionScan(StoreLifetime lifetime)
	{
		StoreShape reflection = WithGeneratedRegistrationDisabled(
			disabled: true,
			() => StoreShape.Capture(AddFluxor(lifetime)));
		StoreShape generated = WithGeneratedRegistrationDisabled(
			disabled: false,
			() => StoreShape.Capture(AddFluxor(lifetime)));

		Assert.Contains(reflection.Features, x => x.StartsWith("Title: "));
		Assert.Contains(reflection.Features, x => x.Contains("CounterState { Count = 0 }"));
		Assert.Contains(reflection.Reducers, x => x.Contains("CounterReducers.ReduceIncrement("));
		Assert.Contains(reflection.Reducers, x => x.Contains("CounterReducers.ReduceReset("));
		Assert.Contains(reflection.Reducers, x => x.EndsWith(typeof(DoubleCounterReducer).FullName));
		Assert.Contains(reflection.Effects, x => x.Contains("CounterEffects.HandleRequestIncrement("));
		Assert.Contains(reflection.Effects, x => x.StartsWith(typeof(ResetEffect).FullName));
		Assert.Contains(reflection.Reducers, x => x.Contains("CounterAuditReducers.ReduceResetCompleted("));
		Assert.Contains(reflection.Effects, x => x.Contains("CounterAuditEffects.HandleIncrement("));
		StoreShape.AssertEqual(reflection, generated);
	}

	[Fact]
	public void WhenAssemblyHasGeneratedRegistration_ThenRegistrationIsUsed()
	{
		int readsWhenEnabled = WithGeneratedRegistrationDisabled(
			disabled: false,
			() => CountTypesReads(() => AddFluxor(StoreLifetime.Scoped)));
		int readsWhenDisabled = WithGeneratedRegistrationDisabled(
			disabled: true,
			() => CountTypesReads(() => AddFluxor(StoreLifetime.Scoped)));

		Assert.Equal(1, readsWhenEnabled);
		Assert.Equal(0, readsWhenDisabled);
	}

	[Theory]
	[InlineData(StoreLifetime.Scoped)]
	[InlineData(StoreLifetime.Singleton)]
	public void WhenAssemblyHasGeneratedRegistration_ThenEveryReducerAndEffectIsBuiltFromItsEntry(StoreLifetime lifetime)
	{
		// Switch off: generated registration in use
		(StoreShape generated, FixtureCounts generatedCounts) = WithGeneratedRegistrationDisabled(
			disabled: false,
			() => CaptureCounted(lifetime));
		// Switch on: reflection scan
		(StoreShape reflection, FixtureCounts reflectionCounts) = WithGeneratedRegistrationDisabled(
			disabled: true,
			() => CaptureCounted(lifetime));

		// 3 reducer methods + 2 effect methods in the fixture
		Assert.Equal(5, generated.Wrappers.Count);
		Assert.All(generated.Wrappers, x => Assert.True(x.HasGeneratedHandler, x.Description));
		Assert.Equal(5, reflection.Wrappers.Count);
		Assert.All(reflection.Wrappers, x => Assert.False(x.HasGeneratedHandler, x.Description));

		Assert.True(generatedCounts.FeaturesCreated > 0, nameof(FixtureCounts.FeaturesCreated));
		Assert.True(generatedCounts.ReducersAdded > 0, nameof(FixtureCounts.ReducersAdded));
		Assert.True(generatedCounts.ReducersCreated > 0, nameof(FixtureCounts.ReducersCreated));
		Assert.True(generatedCounts.EffectsCreated > 0, nameof(FixtureCounts.EffectsCreated));
		Assert.Equal(new FixtureCounts(0, 0, 0, 0), reflectionCounts);
	}

	[Fact]
	public void WhenAttributeHasUnknownFormatVersion_ThenAssemblyIsScannedByReflection()
	{
		var unknownVersionAssembly = new FakeAssembly(formatVersion: FluxorGeneratedRegistrationFormat.Current + 1);
		var knownVersionAssembly = new FakeAssembly(formatVersion: FluxorGeneratedRegistrationFormat.Current);

		GeneratedRegistrationCatalog catalog = WithGeneratedRegistrationDisabled(
			disabled: false,
			() => GeneratedRegistrationCatalog.Create(new Assembly[] { unknownVersionAssembly, knownVersionAssembly }));

		Assert.False(catalog.TryGetTypes(unknownVersionAssembly, out _));
		Assert.True(catalog.TryGetTypes(knownVersionAssembly, out _));
	}

	[Theory]
	[InlineData(0)]
	[InlineData(2)]
	[InlineData(-1)]
	public void WhenFixtureAttributeHasOtherFormatVersion_ThenStoreIsBuiltByReflection(int formatVersion)
	{
		var assembly = new FixtureWithAttribute(new FluxorGeneratedRegistrationAttribute(typeof(HandWrittenFluxorRegistration), formatVersion));

		AssertReflectionPathUsed(assembly);
	}

	[Theory]
	[InlineData(typeof(CounterState))]
	[InlineData(null)]
	public void WhenRegistrationTypeDoesNotImplementTheInterface_ThenStoreIsBuiltByReflection(Type registrationType)
	{
		var assembly = new FixtureWithAttribute(new FluxorGeneratedRegistrationAttribute(registrationType, FluxorGeneratedRegistrationFormat.Current));

		AssertReflectionPathUsed(assembly);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void WhenRegistrationHasEntriesOfAnotherAssembly_ThenTheyAreIgnored(bool foreignTypeIsScanned)
	{
		// Reducers for the fixture's state, described as if they belonged to types of this test assembly
		var foreignEntries = new[]
		{
			null,
			new GeneratedTypeEntry(type: null),
			new GeneratedTypeEntry(
				type: typeof(ForeignHost),
				reducerMethods: new[]
				{
					new GeneratedReducerMethod(
						stateType: typeof(CounterState),
						isStatic: true,
						createReducer: (_, _) => GeneratedWrappers.Reducer<CounterState, Increment>(ForeignHost.MultiplyByTen),
						index: 0),
				}),
		};

		AssertGeneratedEqualsReflection(foreignEntries, foreignTypeIsScanned);
	}

	[Fact]
	public void WhenRegistrationDescribesATypeTwice_ThenTheFirstEntryIsUsed()
	{
		// Another description of CounterReducers, listing a reducer the type does not have
		var duplicate = new GeneratedTypeEntry(
			type: typeof(CounterReducers),
			reducerMethods: new[]
			{
				new GeneratedReducerMethod(
					stateType: typeof(CounterState),
					isStatic: true,
					createReducer: (_, _) => GeneratedWrappers.Reducer<CounterState, DoubleCount>(ForeignHost.MultiplyByTen),
					index: 0),
			});

		AssertGeneratedEqualsReflection(new[] { duplicate }, foreignTypeIsScanned: false);
	}

	/// <summary>The switch as the catalog reads it: not set, or set to a value.</summary>
	public static TheoryData<bool?> SwitchStates => new() { null, false, true };

	[Theory]
	[MemberData(nameof(SwitchStates))]
	public void WhenDisableSwitchIsRead_ThenOnlyTrueDisablesGeneratedRegistration(bool? switchValue)
	{
		string readName = null;
		GeneratedRegistrationCatalog.SwitchReader reader = (string name, out bool isEnabled) =>
		{
			readName = name;
			isEnabled = switchValue ?? false;
			return switchValue.HasValue;
		};

		int before = HandWrittenFluxorRegistration.TypesReadCount;
		GeneratedRegistrationCatalog catalog = GeneratedRegistrationCatalog.Create(new[] { FixtureAssembly }, reader);
		int typesReads = HandWrittenFluxorRegistration.TypesReadCount - before;

		bool expectDisabled = switchValue == true;
		Assert.Equal(DisableSwitchName, readName);
		Assert.Equal(!expectDisabled, catalog.TryGetTypes(FixtureAssembly, out _));
		Assert.Equal(!expectDisabled, catalog.TryGetEntry(typeof(CounterReducers), out _));
		Assert.Equal(expectDisabled ? 0 : 1, typesReads);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void WhenCatalogIsCreatedWithoutAReader_ThenItReadsTheAppContextSwitch(bool disabled)
	{
		GeneratedRegistrationCatalog catalog = WithGeneratedRegistrationDisabled(
			disabled,
			() => GeneratedRegistrationCatalog.Create(new[] { FixtureAssembly }));

		Assert.Equal(!disabled, catalog.TryGetTypes(FixtureAssembly, out _));
	}

	private static void AssertReflectionPathUsed(Assembly assembly)
	{
		StoreShape reflection = WithGeneratedRegistrationDisabled(
			disabled: true,
			() => StoreShape.Capture(AddFluxor(StoreLifetime.Scoped)));

		(StoreShape shape, int typesReads) = WithGeneratedRegistrationDisabled(disabled: false, () =>
		{
			int before = HandWrittenFluxorRegistration.TypesReadCount;
			var services = new ServiceCollection();
			services.AddFluxor(x => x.ScanAssemblies(assembly));
			StoreShape captured = StoreShape.Capture(services);
			return (captured, HandWrittenFluxorRegistration.TypesReadCount - before);
		});

		StoreShape.AssertEqual(reflection, shape);
		Assert.Equal(0, typesReads);
		Assert.NotEmpty(shape.Wrappers);
		Assert.All(shape.Wrappers, x => Assert.False(x.HasGeneratedHandler, x.Description));
	}

	private static void AssertGeneratedEqualsReflection(GeneratedTypeEntry[] additionalEntries, bool foreignTypeIsScanned)
	{
		IServiceCollection AddFluxorWithForeignType()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x =>
			{
				x.ScanAssemblies(FixtureAssembly);
				if (foreignTypeIsScanned)
					x.ScanTypes(typeof(ForeignHost));
			});
			return services;
		}

		StoreShape reflection = WithGeneratedRegistrationDisabled(
			disabled: true,
			() => StoreShape.Capture(AddFluxorWithForeignType()));

		HandWrittenFluxorRegistration.AdditionalEntries = additionalEntries;
		try
		{
			(StoreShape generated, int typesReads) = WithGeneratedRegistrationDisabled(disabled: false, () =>
			{
				int before = HandWrittenFluxorRegistration.TypesReadCount;
				StoreShape captured = StoreShape.Capture(AddFluxorWithForeignType());
				return (captured, HandWrittenFluxorRegistration.TypesReadCount - before);
			});

			StoreShape.AssertEqual(reflection, generated);
			Assert.Equal(1, typesReads);
			Assert.Equal(5, generated.Wrappers.Count);
			Assert.All(generated.Wrappers, x => Assert.True(x.HasGeneratedHandler, x.Description));
		}
		finally
		{
			HandWrittenFluxorRegistration.AdditionalEntries = null;
		}
	}

	[Fact]
	public async Task WhenDispatching_ThenBothPathsProduceTheSameStateAndFollowUpActions()
	{
		List<string> reflection = await WithGeneratedRegistrationDisabledAsync(
			disabled: true,
			() => DispatchAndRecordAsync());
		List<string> generated = await WithGeneratedRegistrationDisabledAsync(
			disabled: false,
			() => DispatchAndRecordAsync());

		Assert.Contains("Increment { } -> 1", reflection);
		Assert.Contains("ResetCompleted { } -> 0", reflection);
		Assert.Equal(reflection, generated);
	}

	[Fact]
	public void WhenTypesInMetadataTokenOrder_ThenEqualsGetTypesOrder()
	{
		Type[] getTypesOrder = FixtureAssembly.GetTypes()
			.Where(x => !x.IsGenericTypeDefinition)
			.ToArray();
		Type[] metadataTokenOrder = getTypesOrder
			.OrderBy(x => x.MetadataToken)
			.ToArray();

		Assert.Equal(getTypesOrder, metadataTokenOrder);
	}

	private static IServiceCollection AddFluxor(StoreLifetime lifetime)
	{
		var services = new ServiceCollection();
		services.AddFluxor(x => x
			.WithLifetime(lifetime)
			.ScanAssemblies(FixtureAssembly));
		return services;
	}

	private record FixtureCounts(int FeaturesCreated, int ReducersAdded, int ReducersCreated, int EffectsCreated);

	private static FixtureCounts ReadFixtureCounts() => new(
		HandWrittenFluxorRegistration.FeaturesCreated,
		HandWrittenFluxorRegistration.ReducersAdded,
		HandWrittenFluxorRegistration.ReducersCreated,
		HandWrittenFluxorRegistration.EffectsCreated);

	private static (StoreShape, FixtureCounts) CaptureCounted(StoreLifetime lifetime)
	{
		FixtureCounts before = ReadFixtureCounts();
		StoreShape shape = StoreShape.Capture(AddFluxor(lifetime));
		FixtureCounts after = ReadFixtureCounts();
		return (shape, new FixtureCounts(
			after.FeaturesCreated - before.FeaturesCreated,
			after.ReducersAdded - before.ReducersAdded,
			after.ReducersCreated - before.ReducersCreated,
			after.EffectsCreated - before.EffectsCreated));
	}

	private static int CountTypesReads(Action action)
	{
		int before = HandWrittenFluxorRegistration.TypesReadCount;
		action();
		return HandWrittenFluxorRegistration.TypesReadCount - before;
	}

	private static async Task<List<string>> DispatchAndRecordAsync()
	{
		IServiceCollection services = AddFluxor(StoreLifetime.Scoped);
		using ServiceProvider serviceProvider = services.BuildServiceProvider();
		using IServiceScope scope = serviceProvider.CreateScope();
		IServiceProvider scopedProvider = scope.ServiceProvider;

		var store = scopedProvider.GetRequiredService<IStore>();
		await store.InitializeAsync();
		var dispatcher = scopedProvider.GetRequiredService<IDispatcher>();
		var actionSubscriber = scopedProvider.GetRequiredService<IActionSubscriber>();
		var counter = scopedProvider.GetRequiredService<IState<CounterState>>();
		var title = scopedProvider.GetRequiredService<IState<TitleState>>();

		var log = new List<string>();
		var subscriber = new object();
		actionSubscriber.SubscribeToAction<object>(
			subscriber,
			action => log.Add($"{action} -> {counter.Value.Count}"));

		dispatcher.Dispatch(new Increment());
		dispatcher.Dispatch(new Increment());
		dispatcher.Dispatch(new DoubleCount());
		dispatcher.Dispatch(new RequestIncrement());
		dispatcher.Dispatch(new Reset());

		actionSubscriber.UnsubscribeFromAllActions(subscriber);
		log.Add($"final: {counter.Value} {title.Value}");
		return log;
	}

	private static T WithGeneratedRegistrationDisabled<T>(bool disabled, Func<T> action)
	{
		bool hadValue = AppContext.TryGetSwitch(DisableSwitchName, out bool previous);
		AppContext.SetSwitch(DisableSwitchName, disabled);
		try
		{
			return action();
		}
		finally
		{
			AppContext.SetSwitch(DisableSwitchName, hadValue && previous);
		}
	}

	private static async Task<T> WithGeneratedRegistrationDisabledAsync<T>(bool disabled, Func<Task<T>> action)
	{
		bool hadValue = AppContext.TryGetSwitch(DisableSwitchName, out bool previous);
		AppContext.SetSwitch(DisableSwitchName, disabled);
		try
		{
			return await action();
		}
		finally
		{
			AppContext.SetSwitch(DisableSwitchName, hadValue && previous);
		}
	}

	private sealed class FakeAssembly : Assembly
	{
		private readonly FluxorGeneratedRegistrationAttribute Attribute;

		public FakeAssembly(int formatVersion)
		{
			Attribute = new FluxorGeneratedRegistrationAttribute(typeof(EmptyRegistration), formatVersion);
		}

		public override object[] GetCustomAttributes(bool inherit) =>
			new object[] { Attribute };

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
			attributeType.IsAssignableFrom(typeof(FluxorGeneratedRegistrationAttribute))
				? new FluxorGeneratedRegistrationAttribute[] { Attribute }
				: Array.Empty<Attribute>();

		public override bool IsDefined(Type attributeType, bool inherit) =>
			attributeType.IsAssignableFrom(typeof(FluxorGeneratedRegistrationAttribute));

		public override string FullName => "FakeAssembly, Version=1.0.0.0";
	}

	/// <summary>
	/// The fixture assembly (same types) with another <see cref="FluxorGeneratedRegistrationAttribute"/>.
	/// </summary>
	private sealed class FixtureWithAttribute : Assembly
	{
		private readonly FluxorGeneratedRegistrationAttribute Attribute;

		public FixtureWithAttribute(FluxorGeneratedRegistrationAttribute attribute)
		{
			Attribute = attribute;
		}

		public override Type[] GetTypes() => FixtureAssembly.GetTypes();
		public override Type[] GetExportedTypes() => FixtureAssembly.GetExportedTypes();
		public override string FullName => FixtureAssembly.FullName;
		public override AssemblyName GetName() => FixtureAssembly.GetName();

		public override object[] GetCustomAttributes(bool inherit) =>
			FixtureAssembly.GetCustomAttributes(inherit)
				.Where(x => x is not FluxorGeneratedRegistrationAttribute)
				.Append(Attribute)
				.ToArray();

		public override object[] GetCustomAttributes(Type attributeType, bool inherit) =>
			attributeType.IsAssignableFrom(typeof(FluxorGeneratedRegistrationAttribute))
				? new FluxorGeneratedRegistrationAttribute[] { Attribute }
				: FixtureAssembly.GetCustomAttributes(attributeType, inherit);

		public override bool IsDefined(Type attributeType, bool inherit) =>
			attributeType.IsAssignableFrom(typeof(FluxorGeneratedRegistrationAttribute))
			|| FixtureAssembly.IsDefined(attributeType, inherit);
	}

	/// <summary>A type of this test assembly with no Fluxor attributes.</summary>
	private static class ForeignHost
	{
		public static CounterState MultiplyByTen(CounterState state, object action) => state with { Count = state.Count * 10 };
	}

	private sealed class EmptyRegistration : IFluxorGeneratedRegistration
	{
		public IReadOnlyList<GeneratedTypeEntry> Types => Array.Empty<GeneratedTypeEntry>();
	}
}
