namespace Fluxor.UnitTests.StoreTests.ThreadingTests.DispatchWhileInitializingTests.SupportFiles
{
	class CounterFeature : Feature<CounterState>
	{
		public override string GetName() => "Counter";
		protected override CounterState GetInitialState() => new(counter: 0);
	}
}
