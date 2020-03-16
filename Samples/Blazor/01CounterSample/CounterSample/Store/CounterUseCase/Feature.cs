using Fluxor;

namespace CounterSample.Store.CounterUseCase
{
	public class Feature : Feature<State>
	{
		public override string GetName() => "Counter";
		protected override State GetInitialState() => new State(clickCount: 0);
	}
}
