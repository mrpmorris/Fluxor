using Fluxor.DependencyInjection.Generated;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;

public class GeneratedWrappersTests
{
	private class A { }
	private class DerivedA : A { }
	private class TestState { }

	[Fact]
	public void WhenReducerCreated_ThenShouldReduceOnlyForActionTypeAndDerived()
	{
		var expected = new TestState();
		IReducer<TestState> reducer = GeneratedWrappers.Reducer<TestState, A>((s, a) => expected);

		Assert.True(reducer.ShouldReduceStateForAction(new A()));
		Assert.True(reducer.ShouldReduceStateForAction(new DerivedA()));
		Assert.False(reducer.ShouldReduceStateForAction(new object()));
		Assert.Same(expected, reducer.Reduce(new TestState(), new A()));
	}

	[Fact]
	public void WhenReducerWithoutActionCreated_ThenActionIsIgnoredAndStateReduced()
	{
		var expected = new TestState();
		IReducer<TestState> reducer = GeneratedWrappers.ReducerWithoutAction<TestState, A>(s => expected);

		Assert.True(reducer.ShouldReduceStateForAction(new A()));
		Assert.False(reducer.ShouldReduceStateForAction(new object()));
		Assert.Same(expected, reducer.Reduce(new TestState(), new A()));
	}

	[Fact]
	public async Task WhenEffectCreated_ThenReactsOnlyToActionTypeAndPassesActionAndDispatcher()
	{
		A receivedAction = null;
		IDispatcher receivedDispatcher = null;
		IEffect effect = GeneratedWrappers.Effect<A>((a, d) =>
		{
			receivedAction = a;
			receivedDispatcher = d;
			return Task.CompletedTask;
		});
		var action = new DerivedA();
		var dispatcher = new Mock<IDispatcher>().Object;

		Assert.True(effect.ShouldReactToAction(new A()));
		Assert.True(effect.ShouldReactToAction(action));
		Assert.False(effect.ShouldReactToAction(new object()));
		await effect.HandleAsync(action, dispatcher);

		Assert.Same(action, receivedAction);
		Assert.Same(dispatcher, receivedDispatcher);
	}

	[Fact]
	public async Task WhenEffectWithoutActionCreated_ThenReactsOnlyToActionTypeAndPassesDispatcher()
	{
		IDispatcher receivedDispatcher = null;
		IEffect effect = GeneratedWrappers.EffectWithoutAction<A>(d =>
		{
			receivedDispatcher = d;
			return Task.CompletedTask;
		});
		var dispatcher = new Mock<IDispatcher>().Object;

		Assert.True(effect.ShouldReactToAction(new DerivedA()));
		Assert.False(effect.ShouldReactToAction(new object()));
		await effect.HandleAsync(new A(), dispatcher);

		Assert.Same(dispatcher, receivedDispatcher);
	}

	[Fact]
	public void WhenFeatureStateCreatedWithNullName_ThenNameIsStateFullName()
	{
		IFeature<TestState> feature = GeneratedWrappers.FeatureState<TestState>(null, 0, true, () => new TestState());

		Assert.Equal(typeof(TestState).FullName, feature.GetName());
	}

	[Fact]
	public void WhenFeatureStateCreatedWithName_ThenNameIsUsedAndSettingsCopied()
	{
		var initial = new TestState();
		int calls = 0;
		IFeature<TestState> feature = GeneratedWrappers.FeatureState<TestState>("X", 5, false, () =>
		{
			calls++;
			return initial;
		});

		Assert.Equal("X", feature.GetName());
		Assert.Equal(5, feature.MaximumStateChangedNotificationsPerSecond);
		Assert.False(feature.DebuggerBrowsable);
		Assert.Same(initial, feature.State);
		Assert.Equal(1, calls);
	}

	[Fact]
	public void WhenFeatureStateCreatedWithIndexedFactory_ThenFactoryIsCalledWithTheIndexOnFirstRead()
	{
		var initial = new TestState();
		var indexes = new System.Collections.Generic.List<int>();
		IFeature<TestState> feature = GeneratedWrappers.FeatureState<TestState>("Y", 7, true, index =>
		{
			indexes.Add(index);
			return initial;
		}, 42);

		Assert.Empty(indexes);
		Assert.Equal("Y", feature.GetName());
		Assert.Equal(7, feature.MaximumStateChangedNotificationsPerSecond);
		Assert.True(feature.DebuggerBrowsable);
		Assert.Same(initial, feature.State);
		Assert.Same(initial, feature.State);
		Assert.Equal(new[] { 42 }, indexes);
	}

	[Fact]
	public void WhenFeatureStateCreatedWithIndexedFactoryAndNullName_ThenNameIsStateFullName()
	{
		IFeature<TestState> feature = GeneratedWrappers.FeatureState<TestState>(null, 0, false, _ => new TestState(), 0);

		Assert.Equal(typeof(TestState).FullName, feature.GetName());
		Assert.False(feature.DebuggerBrowsable);
	}

	[Fact]
	public void WhenFeatureStateCreatedWithNullIndexedFactory_ThenThrowsArgumentNullException()
	{
		var exception = Assert.Throws<ArgumentNullException>(() =>
			GeneratedWrappers.FeatureState<TestState>(null, 0, true, (Func<int, object>)null, 0));

		Assert.Equal("createInitialState", exception.ParamName);
	}

	[Fact]
	public void WhenAttributeConstructed_ThenExposesTypeAndVersion()
	{
		var attribute = new FluxorGeneratedRegistrationAttribute(typeof(GeneratedWrappersTests), 7);

		Assert.Equal(typeof(GeneratedWrappersTests), attribute.RegistrationType);
		Assert.Equal(7, attribute.FormatVersion);
		Assert.Equal(1, FluxorGeneratedRegistrationFormat.Current);
	}
}
