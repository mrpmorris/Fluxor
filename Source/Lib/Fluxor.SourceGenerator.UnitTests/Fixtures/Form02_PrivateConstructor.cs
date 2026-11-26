using Fluxor;

namespace Fixture;

[FeatureState(Name = "PrivateCtor")]
public record PrivateCtorState(int Count)
{
	private PrivateCtorState() : this(42) { }
}

[FeatureState]
public class ProtectedCtorState
{
	protected ProtectedCtorState() : this("initial") { }
	public ProtectedCtorState(string text) { Text = text; }
	public string Text { get; }
	public override string ToString() => "ProtectedCtorState " + Text;
}

public record PrivateCtorAction;
public record PrivateCtorOtherAction;

// Per state: two reducers for the same action, in an order that changes the result, and one for another action
public class PrivateCtorReducers
{
	[ReducerMethod(typeof(PrivateCtorAction))]
	public PrivateCtorState Halve(PrivateCtorState state) => state with { Count = state.Count / 2 };

	[ReducerMethod]
	public static PrivateCtorState AddThree(PrivateCtorState state, PrivateCtorAction action) => state with { Count = state.Count + 3 };

	[ReducerMethod]
	public PrivateCtorState Square(PrivateCtorState state, PrivateCtorOtherAction action) => state with { Count = state.Count * state.Count };

	[ReducerMethod]
	public static ProtectedCtorState AppendX(ProtectedCtorState state, PrivateCtorAction action) => new(state.Text + "+x");

	[ReducerMethod]
	public static ProtectedCtorState WrapInBrackets(ProtectedCtorState state, PrivateCtorAction action) => new("[" + state.Text + "]");

	[ReducerMethod(typeof(PrivateCtorOtherAction))]
	public static ProtectedCtorState AppendOther(ProtectedCtorState state) => new(state.Text + "+other");
}
