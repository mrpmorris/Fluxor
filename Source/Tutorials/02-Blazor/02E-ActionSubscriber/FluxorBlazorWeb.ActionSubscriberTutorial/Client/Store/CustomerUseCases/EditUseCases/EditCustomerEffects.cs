using Fluxor;
using Fluxor.Blazor.Web.Middlewares.Routing;
using System.Net.Http.Json;
using CustomerContracts = FluxorBlazorWeb.ActionSubscriberTutorial.Contracts.Customers;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.EditUseCases;

public class EditCustomerEffects
{
	private readonly HttpClient HttpClient;

	public EditCustomerEffects(HttpClient httpClient)
	{
		HttpClient = httpClient;
	}

	/// <summary>
	/// Pause for 0.5 seconds then ask the server for a customer object to edit
	/// </summary>
	/// <param name="action"></param>
	/// <param name="dispatcher"></param>
	/// <returns></returns>
	[EffectMethod]
	public async Task HandleGetCustomerForEditActionAsync(GetCustomerForEditAction action, IDispatcher dispatcher)
	{
		await Task.Delay(500);
		var dto = await HttpClient.GetFromJsonAsync<CustomerContracts.EditCustomerDto>($"/api/customer/{action.Id}");
		dispatcher.Dispatch(new GetCustomerForEditActionResult(dto!));
	}

	/// <summary>
	/// Pause for 0.5 seconds then send the updated customer object to the server
	/// </summary>
	/// <param name="action"></param>
	/// <param name="dispatcher"></param>
	/// <returns></returns>
	[EffectMethod]
	public async Task HandleUpdateCustomerActionAsync(UpdateCustomerAction action, IDispatcher dispatcher)
	{
		await Task.Delay(500);
		await HttpClient.PostAsJsonAsync($"/api/customer", action.Dto);
		dispatcher.Dispatch(new UpdateCustomerActionResult(action.Dto));
		dispatcher.Dispatch(new GoAction("/customers/search"));
	}
}
