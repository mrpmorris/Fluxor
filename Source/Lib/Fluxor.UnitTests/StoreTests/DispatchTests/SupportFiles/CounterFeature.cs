namespace Fluxor.UnitTests.StoreTests.DispatchTests.SupportFiles;

public class CounterFeature : Feature<int>
{
	public override string GetName() => "Counter";
	protected override int GetInitialState() => 0;
}
