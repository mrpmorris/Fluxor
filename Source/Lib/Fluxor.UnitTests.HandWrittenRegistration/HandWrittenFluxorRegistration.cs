using Fluxor.DependencyInjection.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

[assembly: FluxorGeneratedRegistration(
	typeof(Fluxor.UnitTests.HandWrittenRegistration.HandWrittenFluxorRegistration),
	1)]

namespace Fluxor.UnitTests.HandWrittenRegistration;

/// <summary>
/// Written by hand in the (compact) shape the source generator emits, describing exactly the Fluxor types of this
/// assembly: one factory per host class selecting the method by index, and one switch per assembly for feature
/// creation, initial state and AddReducer.
/// Entries are deliberately NOT in metadata order, so the runtime's sort by metadata token is exercised.
/// The <see cref="Counted{T}"/> calls, the counters and <see cref="AdditionalEntries"/> are test hooks, not part
/// of the generated shape.
/// </summary>
public sealed class HandWrittenFluxorRegistration : IFluxorGeneratedRegistration
{
	private static int TypesReadCountValue;
	private static int FeaturesCreatedValue;
	private static int ReducersAddedValue;
	private static int ReducersCreatedValue;
	private static int EffectsCreatedValue;

	/// <summary>Number of times <see cref="Types"/> has been read.</summary>
	public static int TypesReadCount => Volatile.Read(ref TypesReadCountValue);
	/// <summary>Number of features created by the createFeature delegates.</summary>
	public static int FeaturesCreated => Volatile.Read(ref FeaturesCreatedValue);
	/// <summary>Number of reducers added by the addReducer delegates.</summary>
	public static int ReducersAdded => Volatile.Read(ref ReducersAddedValue);
	/// <summary>Number of reducers created by the createReducer delegates.</summary>
	public static int ReducersCreated => Volatile.Read(ref ReducersCreatedValue);
	/// <summary>Number of effects created by the createEffect delegates.</summary>
	public static int EffectsCreated => Volatile.Read(ref EffectsCreatedValue);

	/// <summary>
	/// Entries returned after the ones describing this assembly (null for none), for tests of entries the
	/// runtime must ignore.
	/// </summary>
	public static IReadOnlyList<GeneratedTypeEntry> AdditionalEntries { get; set; }

	// Private, so the runtime must create the registration with nonPublic: true
	private HandWrittenFluxorRegistration() { }

	private static readonly GeneratedTypeEntry[] Entries = new GeneratedTypeEntry[]
	{
		// Declared last in the assembly, listed first here
		new GeneratedTypeEntry(
			type: typeof(CounterAuditEffects),
			effectMethods: new GeneratedEffectMethod[]
			{
				new GeneratedEffectMethod(
					isStatic: true,
					createEffect: CreateCounterAuditEffects,
					index: 0),
			}),
		new GeneratedTypeEntry(
			type: typeof(TitleFeature),
			featureClass: new GeneratedFeatureClass(
				stateType: typeof(TitleState),
				featureInterfaceType: typeof(IFeature<TitleState>),
				addReducer: AddReducer,
				stateIndex: TitleStateIndex)),
		new GeneratedTypeEntry(
			type: typeof(CounterState),
			featureState: new GeneratedFeatureState(
				stateType: typeof(CounterState),
				featureInterfaceType: typeof(IFeature<CounterState>),
				createFeature: CreateFeature,
				addReducer: AddReducer,
				stateIndex: CounterStateIndex)),
		new GeneratedTypeEntry(
			type: typeof(CounterAuditReducers),
			reducerMethods: new GeneratedReducerMethod[]
			{
				new GeneratedReducerMethod(
					stateType: typeof(CounterState),
					isStatic: true,
					createReducer: CreateCounterAuditReducers,
					index: 0),
			}),
		new GeneratedTypeEntry(
			type: typeof(CounterReducers),
			reducerMethods: new GeneratedReducerMethod[]
			{
				new GeneratedReducerMethod(
					stateType: typeof(CounterState),
					isStatic: true,
					createReducer: CreateCounterReducers,
					index: 0),
				new GeneratedReducerMethod(
					stateType: typeof(CounterState),
					isStatic: false,
					createReducer: CreateCounterReducers,
					index: 1),
			}),
		new GeneratedTypeEntry(
			type: typeof(ResetEffect),
			isEffectClass: true),
		new GeneratedTypeEntry(
			type: typeof(DoubleCounterReducer),
			reducerClassStateType: typeof(CounterState)),
		new GeneratedTypeEntry(
			type: typeof(CounterEffects),
			effectMethods: new GeneratedEffectMethod[]
			{
				new GeneratedEffectMethod(
					isStatic: false,
					createEffect: CreateCounterEffects,
					index: 0),
			}),
	};

	/// <inheritdoc />
	public IReadOnlyList<GeneratedTypeEntry> Types
	{
		get
		{
			Interlocked.Increment(ref TypesReadCountValue);
			return AdditionalEntries is null ? Entries : Entries.Concat(AdditionalEntries).ToArray();
		}
	}

	private const int CounterStateIndex = 0;
	private const int TitleStateIndex = 1;

	private static object CreateCounterReducers(object host, int index)
	{
		switch (index)
		{
			case 0: return Counted(ref ReducersCreatedValue, GeneratedWrappers.Reducer<CounterState, Increment>(CounterReducers.ReduceIncrement));
			case 1: return Counted(ref ReducersCreatedValue, GeneratedWrappers.ReducerWithoutAction<CounterState, Reset>(((CounterReducers)host).ReduceReset));
			default: throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	private static object CreateCounterAuditReducers(object host, int index)
	{
		switch (index)
		{
			case 0: return Counted(ref ReducersCreatedValue, GeneratedWrappers.Reducer<CounterState, ResetCompleted>(CounterAuditReducers.ReduceResetCompleted));
			default: throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	private static IEffect CreateCounterEffects(object host, int index)
	{
		switch (index)
		{
			case 0: return Counted(ref EffectsCreatedValue, GeneratedWrappers.Effect<RequestIncrement>(((CounterEffects)host).HandleRequestIncrement));
			default: throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	private static IEffect CreateCounterAuditEffects(object host, int index)
	{
		switch (index)
		{
			case 0: return Counted(ref EffectsCreatedValue, GeneratedWrappers.EffectWithoutAction<Increment>(CounterAuditEffects.HandleIncrement));
			default: throw new ArgumentOutOfRangeException(nameof(index));
		}
	}

	private static IFeature CreateFeature(int stateIndex)
	{
		switch (stateIndex)
		{
			case CounterStateIndex:
				return Counted(ref FeaturesCreatedValue, GeneratedWrappers.FeatureState<CounterState>(
					name: null,
					maximumStateChangedNotificationsPerSecond: 0,
					debuggerBrowsable: true,
					createInitialState: CreateInitialState,
					stateIndex: CounterStateIndex));
			default: throw new ArgumentOutOfRangeException(nameof(stateIndex));
		}
	}

	private static object CreateInitialState(int stateIndex)
	{
		switch (stateIndex)
		{
			// CounterState only has a private parameterless constructor, so the generated code uses the
			// one allowed targeted reflection call to create its initial state.
			case CounterStateIndex: return Activator.CreateInstance(typeof(CounterState), nonPublic: true);
			default: throw new ArgumentOutOfRangeException(nameof(stateIndex));
		}
	}

	private static void AddReducer(IFeature feature, object reducer, int stateIndex)
	{
		Interlocked.Increment(ref ReducersAddedValue);
		switch (stateIndex)
		{
			case CounterStateIndex: ((IFeature<CounterState>)feature).AddReducer((IReducer<CounterState>)reducer); return;
			case TitleStateIndex: ((IFeature<TitleState>)feature).AddReducer((IReducer<TitleState>)reducer); return;
			default: throw new ArgumentOutOfRangeException(nameof(stateIndex));
		}
	}

	private static T Counted<T>(ref int counter, T value)
	{
		Interlocked.Increment(ref counter);
		return value;
	}
}
