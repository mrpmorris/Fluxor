using Fluxor;
using CustomerContracts = FluxorBlazorWeb.ActionSubscriberTutorial.Contracts.Customers;
using System.Net.Http.Json;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.SearchUseCases;

public class SearchCustomerEffects
{
	private readonly HttpClient HttpClient;

	public SearchCustomerEffects(HttpClient httpClient)
	{
		HttpClient = httpClient;
	}

	/// <summary>
	/// Pause for 0.5 seconds then ask the server for a list of customers
	/// </summary>
	/// <param name="dispatcher"></param>
	/// <returns></returns>
	[EffectMethod(typeof(SearchCustomersAction))]
	public async Task HandleSearchCustomersAction(IDispatcher dispatcher)
	{
		await Task.Delay(500);
		var response = await HttpClient.GetFromJsonAsync<IEnumerable<CustomerContracts.CustomerSummaryDto>>("/api/customers/search");
		var customers = response?.Select(x => new Customer(Id: x.Id, Name: x.Name));
		dispatcher.Dispatch(new SearchCustomersActionResult(customers!));
	}
}
