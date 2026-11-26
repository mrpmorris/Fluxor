using Fluxor;

namespace Fixture;

public record FeatureClassState(string Name);

public class FeatureClassFeature : Feature<FeatureClassState>
{
	public override string GetName() => "FeatureClass";
	protected override FeatureClassState GetInitialState() => new("initial");
}

public record SetNameAction;

public class FeatureClassReducers
{
	[ReducerMethod]
	public static FeatureClassState Reduce(FeatureClassState state, SetNameAction action) => state with { Name = "set" };
}
