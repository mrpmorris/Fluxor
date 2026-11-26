using Fluxor;

namespace Fixture;

[FeatureState(Name = "PublicCtor", MaximumStateChangedNotificationsPerSecond = 5, DebuggerBrowsable = false)]
public record PublicCtorState(int Count)
{
	public PublicCtorState() : this(7) { }
}

[FeatureState]
public class PublicCtorClassState
{
	public int Count { get; set; } = 3;
	public override string ToString() => "PublicCtorClassState " + Count;
}

public record PublicCtorAction;
public record PublicCtorOtherAction;

// Per state: two reducers for the same action, in an order that changes the result, and one for another action
public class PublicCtorReducers
{
	[ReducerMethod]
	public static PublicCtorState Double(PublicCtorState state, PublicCtorAction action) => state with { Count = state.Count * 2 };

	[ReducerMethod(typeof(PublicCtorAction))]
	public PublicCtorState AddOne(PublicCtorState state) => state with { Count = state.Count + 1 };

	[ReducerMethod]
	public static PublicCtorState Subtract(PublicCtorState state, PublicCtorOtherAction action) => state with { Count = state.Count - 10 };

	[ReducerMethod]
	public static PublicCtorClassState AddFive(PublicCtorClassState state, PublicCtorAction action) => new() { Count = state.Count + 5 };

	[ReducerMethod(typeof(PublicCtorAction))]
	public static PublicCtorClassState Triple(PublicCtorClassState state) => new() { Count = state.Count * 3 };

	[ReducerMethod(typeof(PublicCtorOtherAction))]
	public PublicCtorClassState Negate(PublicCtorClassState state) => new() { Count = -state.Count };
}
