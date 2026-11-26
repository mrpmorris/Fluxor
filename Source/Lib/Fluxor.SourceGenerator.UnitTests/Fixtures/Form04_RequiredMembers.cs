using Fluxor;

namespace Fixture;

[FeatureState]
public class RequiredMemberState
{
	public required int Count { get; init; }
	public required string Name { get; init; }
	public override string ToString() => "RequiredMemberState " + Count + " " + (Name ?? "null");
}

[FeatureState]
public record RequiredMemberRecord
{
	public required int Value { get; init; }
}

public record RequiredAction;
public record RequiredOtherAction;

// Per state: two reducers for the same action, in an order that changes the result, and one for another action
public class RequiredMemberReducers
{
	[ReducerMethod]
	public static RequiredMemberState AppendName(RequiredMemberState state, RequiredAction action) =>
		new() { Count = state.Count, Name = (state.Name ?? "") + "n" + state.Count };

	[ReducerMethod(typeof(RequiredAction))]
	public RequiredMemberState AddTwo(RequiredMemberState state) =>
		new() { Count = state.Count + 2, Name = state.Name };

	[ReducerMethod]
	public static RequiredMemberState Rename(RequiredMemberState state, RequiredOtherAction action) =>
		new() { Count = state.Count, Name = "other" };

	[ReducerMethod]
	public static RequiredMemberRecord AddSix(RequiredMemberRecord state, RequiredAction action) => state with { Value = state.Value + 6 };

	[ReducerMethod]
	public static RequiredMemberRecord TimesNine(RequiredMemberRecord state, RequiredAction action) => state with { Value = state.Value * 9 };

	[ReducerMethod(typeof(RequiredOtherAction))]
	public static RequiredMemberRecord Negate(RequiredMemberRecord state) => state with { Value = -state.Value };
}
