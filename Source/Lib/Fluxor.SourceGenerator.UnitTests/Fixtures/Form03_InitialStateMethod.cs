using Fluxor;

namespace Fixture;

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

public record FactoryAction;
public record FactoryOtherAction;

// Per state: two reducers for the same action, in an order that changes the result, and one for another action
public class FactoryReducers
{
	[ReducerMethod]
	public static PublicFactoryState AddTen(PublicFactoryState state, FactoryAction action) => state with { Count = state.Count + 10 };

	[ReducerMethod]
	public static PublicFactoryState TimesFour(PublicFactoryState state, FactoryAction action) => state with { Count = state.Count * 4 };

	[ReducerMethod(typeof(FactoryOtherAction))]
	public PublicFactoryState Decrement(PublicFactoryState state) => state with { Count = state.Count - 1 };

	[ReducerMethod(typeof(FactoryAction))]
	public PrivateFactoryState TimesSeven(PrivateFactoryState state) => state with { Count = state.Count * 7 };

	[ReducerMethod]
	public PrivateFactoryState SubtractOne(PrivateFactoryState state, FactoryAction action) => state with { Count = state.Count - 1 };

	[ReducerMethod]
	public static PrivateFactoryState Reset(PrivateFactoryState state, FactoryOtherAction action) => state with { Count = 100 };
}
