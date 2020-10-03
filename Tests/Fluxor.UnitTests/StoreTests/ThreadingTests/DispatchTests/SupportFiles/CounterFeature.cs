namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchTests.SupportFiles
{
	class CounterFeature : Feature<CounterState>
	{
		public override string GetName() => "Counter";
		protected override CounterState GetInitialState() => new CounterState(0);
	}
}
