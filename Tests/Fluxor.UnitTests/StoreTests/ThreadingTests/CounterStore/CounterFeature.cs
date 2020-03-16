namespace Fluxor.UnitTests.StoreTests.ThreadingTests.CounterStore
{
	class CounterFeature : Feature<CounterState>
	{
		public override string GetName() => "Counter";
		protected override CounterState GetInitialState() => new CounterState(0);
	}
}
