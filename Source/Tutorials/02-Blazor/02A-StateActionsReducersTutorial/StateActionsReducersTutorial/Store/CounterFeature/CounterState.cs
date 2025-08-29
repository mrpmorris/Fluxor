using Fluxor;

namespace FluxorBlazorWeb.StateActionsReducersTutorial.Store.CounterFeature;

[FeatureState]
public record CounterState(int ClickCount)
{
	// Required for creating initial state
	public CounterState() : this(0) { }
}
