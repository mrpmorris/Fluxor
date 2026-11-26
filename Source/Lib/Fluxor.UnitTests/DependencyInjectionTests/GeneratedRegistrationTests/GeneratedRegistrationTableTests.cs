using Fluxor.DependencyInjection.Generated;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Table = Fluxor.DependencyInjection.Generated.GeneratedRegistrationTable;

namespace Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;

public class GeneratedRegistrationTableTests
{
	private class Described { }
	private class OtherDescribed { }
	private class StateA { }
	private class StateB { }

	private static readonly Type[] Types =
	{
		typeof(Described), typeof(OtherDescribed), typeof(StateA), typeof(IFeature<StateA>), typeof(StateB), typeof(IFeature<StateB>)
	};

	private static readonly Func<object, int, object>[] ReducerFactories = { (_, _) => "reducer 0", (_, _) => "reducer 1" };
	private static readonly Func<object, int, IEffect>[] EffectFactories = { (_, _) => null, (_, _) => null };
	private static readonly Func<int, IFeature> CreateFeature = _ => null;
	private static readonly Action<IFeature, object, int> AddReducer = (_, _, _) => { };

	private static GeneratedTypeEntry[] Create(params int[] table) =>
		Table.CreateEntries(Types, ReducerFactories, EffectFactories, CreateFeature, AddReducer, table);

	[Fact]
	public void WhenTableIsNull_ThenThrowsArgumentNullException()
	{
		var exception = Assert.Throws<ArgumentNullException>(() =>
			Table.CreateEntries(Types, ReducerFactories, EffectFactories, CreateFeature, AddReducer, null));

		Assert.Equal("table", exception.ParamName);
	}

	[Fact]
	public void WhenTableIsEmpty_ThenThereAreNoEntries()
	{
		Assert.Empty(Create());
	}

	[Fact]
	public void WhenEntryHasNoFlagsAndNoMethods_ThenItOnlyDescribesItsType()
	{
		GeneratedTypeEntry entry = Assert.Single(Create(1, 0, 0, 0));

		Assert.Same(typeof(OtherDescribed), entry.Type);
		Assert.False(entry.IsMiddleware);
		Assert.False(entry.IsEffectClass);
		Assert.Null(entry.FeatureClass);
		Assert.Null(entry.FeatureState);
		Assert.Null(entry.ReducerClassStateType);
		Assert.Empty(entry.ReducerMethods);
		Assert.Empty(entry.EffectMethods);
	}

	[Theory]
	[InlineData(Table.IsMiddleware)]
	[InlineData(Table.IsEffectClass)]
	[InlineData(Table.IsMiddleware | Table.IsEffectClass)]
	public void WhenEntryHasBooleanFlags_ThenOnlyThoseAreSet(int flags)
	{
		GeneratedTypeEntry entry = Assert.Single(Create(0, flags, 0, 0));

		Assert.Equal((flags & Table.IsMiddleware) != 0, entry.IsMiddleware);
		Assert.Equal((flags & Table.IsEffectClass) != 0, entry.IsEffectClass);
		Assert.Null(entry.FeatureClass);
		Assert.Null(entry.FeatureState);
		Assert.Null(entry.ReducerClassStateType);
	}

	[Fact]
	public void WhenFlagsHaveExpectedValues_ThenTheyAreDistinctBits()
	{
		Assert.Equal(
			new[] { 1, 2, 4, 8, 16 },
			new[] { Table.IsMiddleware, Table.IsEffectClass, Table.HasFeatureClass, Table.HasFeatureState, Table.HasReducerClass });
	}

	[Fact]
	public void WhenEntryHasEverything_ThenEveryValueIsReadInOrder()
	{
		int flags = Table.IsMiddleware | Table.IsEffectClass | Table.HasFeatureClass | Table.HasFeatureState | Table.HasReducerClass;
		GeneratedTypeEntry[] entries = Create(
			0, flags,
			2, 3, 7, // feature class: StateA, IFeature<StateA>, state index 7
			4, 5, 9, // feature state: StateB, IFeature<StateB>, state index 9
			4, // reducer class of StateB
			2, 1, 2 * 2 + 1, 4 * 2, // two reducers with factory 1: static of StateA, instance of StateB
			3, 1, 0, 1, 0, // three effects with factory 1: instance, static, instance
			1, 0, 0, 0); // a second, plain entry

		Assert.Equal(2, entries.Length);
		GeneratedTypeEntry entry = entries[0];
		Assert.Same(typeof(Described), entry.Type);
		Assert.True(entry.IsMiddleware);
		Assert.True(entry.IsEffectClass);

		Assert.Same(typeof(StateA), entry.FeatureClass.StateType);
		Assert.Same(typeof(IFeature<StateA>), entry.FeatureClass.FeatureInterfaceType);
		Assert.Equal(7, entry.FeatureClass.StateIndex);
		Assert.Same(AddReducer, entry.FeatureClass.AddReducer);

		Assert.Same(typeof(StateB), entry.FeatureState.StateType);
		Assert.Same(typeof(IFeature<StateB>), entry.FeatureState.FeatureInterfaceType);
		Assert.Equal(9, entry.FeatureState.StateIndex);
		Assert.Same(CreateFeature, entry.FeatureState.CreateFeature);
		Assert.Same(AddReducer, entry.FeatureState.AddReducer);

		Assert.Same(typeof(StateB), entry.ReducerClassStateType);

		Assert.Equal(
			new[] { (typeof(StateA), true, 0), (typeof(StateB), false, 1) },
			entry.ReducerMethods.Select(x => (x.StateType, x.IsStatic, x.Index)));
		Assert.All(entry.ReducerMethods, x => Assert.Same(ReducerFactories[1], x.CreateReducer));

		Assert.Equal(new[] { (false, 0), (true, 1), (false, 2) }, entry.EffectMethods.Select(x => (x.IsStatic, x.Index)));
		Assert.All(entry.EffectMethods, x => Assert.Same(EffectFactories[1], x.CreateEffect));

		Assert.Same(typeof(OtherDescribed), entries[1].Type);
		Assert.Empty(entries[1].ReducerMethods);
	}

	[Fact]
	public void WhenEntriesUseDifferentFactories_ThenEachGetsItsOwn()
	{
		GeneratedTypeEntry[] entries = Create(
			0, 0, 1, 0, 2 * 2, 1, 0, 1,
			1, 0, 1, 1, 2 * 2, 1, 1, 0);

		Assert.Same(ReducerFactories[0], entries[0].ReducerMethods[0].CreateReducer);
		Assert.Same(EffectFactories[0], entries[0].EffectMethods[0].CreateEffect);
		Assert.True(entries[0].EffectMethods[0].IsStatic);
		Assert.Same(ReducerFactories[1], entries[1].ReducerMethods[0].CreateReducer);
		Assert.Same(EffectFactories[1], entries[1].EffectMethods[0].CreateEffect);
		Assert.False(entries[1].EffectMethods[0].IsStatic);
	}

	[Fact]
	public void WhenTableDescribesAnEffect_ThenTheEffectIsCreatedByItsFactory()
	{
		var effect = GeneratedWrappers.EffectWithoutAction<StateA>(_ => Task.CompletedTask);
		object receivedHost = null;
		int receivedIndex = -1;
		GeneratedTypeEntry entry = Assert.Single(Table.CreateEntries(
			Types,
			ReducerFactories,
			new Func<object, int, IEffect>[] { (host, index) => { receivedHost = host; receivedIndex = index; return effect; } },
			CreateFeature,
			AddReducer,
			new[] { 0, 0, 0, 1, 0, 0 }));

		GeneratedEffectMethod method = Assert.Single(entry.EffectMethods);
		var host = new Described();
		Assert.Same(effect, method.CreateEffect(host, method.Index));
		Assert.Same(host, receivedHost);
		Assert.Equal(0, receivedIndex);
	}
}
