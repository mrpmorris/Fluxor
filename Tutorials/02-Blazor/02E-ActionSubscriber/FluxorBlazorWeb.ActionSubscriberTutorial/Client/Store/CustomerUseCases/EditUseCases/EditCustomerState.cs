using Fluxor;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.EditUseCases;

[FeatureState]
public record EditCustomerState(bool IsLoading, bool IsSaving)
{
	public static readonly EditCustomerState Empty = new();
	private EditCustomerState() :
		this(
			IsLoading: false,
			IsSaving: false) { }
}
