using Microsoft.AspNetCore.Components;
using Fluxor;
using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store;
using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.SearchUseCases;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Pages.Customers
{
	public partial class Search
	{
		[Inject] private IDispatcher Dispatcher { get; set; } = null!;
		[Inject] private IState<SearchCustomersState> State { get; set; } = null!;

		protected override void OnInitialized()
		{
			base.OnInitialized();
			if (!State.Value.Customers.Any())
				Refresh();
		}

		protected void Refresh()
		{
			Dispatcher.Dispatch(new SearchCustomersAction(""));
		}
	}
}