namespace Fluxor.UnitTests.FeatureTests.SupportFiles;

public record IncrementAction;

public class IncrementReducer : Reducer<TestState, IncrementAction>
{
	public override TestState Reduce(TestState state, IncrementAction action) => new(state.SomeValue + 1);
}

public class SameStateReducer : Reducer<TestState, IncrementAction>
{
	public override TestState Reduce(TestState state, IncrementAction action) => state;
}

public class CountingFeature : Feature<TestState>
{
	public int GetInitialStateCount { get; private set; }

	public override string GetName() => nameof(CountingFeature);

	protected override TestState GetInitialState()
	{
		GetInitialStateCount++;
		return new TestState(1);
	}
}
