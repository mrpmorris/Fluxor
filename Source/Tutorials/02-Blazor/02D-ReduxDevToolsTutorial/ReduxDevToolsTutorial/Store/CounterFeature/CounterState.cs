using Fluxor;

namespace FluxorBlazorWeb.ReduxDevToolsTutorial.Store.CounterFeature;

[FeatureState]
public record CounterState(int ClickCount)
{
	// Parameterless constructor required for creating initial state
	public CounterState() : this(0) { }
}
