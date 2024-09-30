namespace Fluxor.UnitTests.ActionSubscriberTests.SubscribeToActionTests.SupportFiles;

[FeatureState]
public record State(int DispatchCount)
{
	private State() : this(0) { }
}
