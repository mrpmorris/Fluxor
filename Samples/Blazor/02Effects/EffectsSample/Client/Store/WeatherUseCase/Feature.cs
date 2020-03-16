using Fluxor;

namespace EffectsSample.Client.Store.WeatherUseCase
{
	public class Feature : Feature<State>
	{
		public override string GetName() => "Weather";
		protected override State GetInitialState() =>
			new State(
				isLoading: false,
				forecasts: null);
	}
}
