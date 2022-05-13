using Fluxor;
using System.Collections.Immutable;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.SearchUseCases;

[FeatureState]
public record SearchCustomersState(bool IsLoading, ImmutableArray<Customer> Customers)
{
	public static readonly SearchCustomersState Empty = new();
	
	private SearchCustomersState() :
		this(
			IsLoading: false,
			Customers: ImmutableArray.Create<Customer>())
	{
	}
}