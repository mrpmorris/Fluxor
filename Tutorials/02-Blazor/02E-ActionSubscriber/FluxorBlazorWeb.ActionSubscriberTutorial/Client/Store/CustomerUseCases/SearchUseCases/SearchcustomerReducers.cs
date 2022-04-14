using Fluxor;
using Fluxor.Blazor.Web.Middlewares.Routing;
using FluxorBlazorWeb.ActionSubscriberTutorial.Client.Extensions;
using System.Collections.Immutable;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.SearchUseCases;

public static class SearchcustomerReducers
{
	/// <summary>
	/// Set `IsLoading` to true and clear the `Customers` state when performing a search
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	[ReducerMethod(typeof(SearchCustomersAction))]
	public static SearchCustomersState ReduceSearchCustomersAction(SearchCustomersState state) =>
		state with {
			IsLoading = true,
			Customers = SearchCustomersState.Empty.Customers
		};

	/// <summary>
	/// Set `IsLoading` to false and set the `Customers` to the state returned from the server
	/// </summary>
	/// <param name="state"></param>
	/// <param name="action"></param>
	/// <returns></returns>
	[ReducerMethod]
	public static SearchCustomersState ReduceSearchCustomersActionResult(SearchCustomersState state, SearchCustomersActionResult action) =>
		state with
		{
			IsLoading = false,
			Customers = action.Customers.ToImmutableArray()
		};

	/// <summary>
	/// When another part of the app retrieves a customer from the server in the form of EditCustomerDto
	/// then we need to reduce the new state into the search state, in case another user has edited it.
	/// </summary>
	/// <param name="state"></param>
	/// <param name="action"></param>
	/// <returns></returns>
	[ReducerMethod]
	public static SearchCustomersState ReduceGetCustomerForEditActionResult(SearchCustomersState state, GetCustomerForEditActionResult action) =>
		!state.Customers.ReplaceOne(
			selector: x => x.Id == action.Dto.Id,
			replacement: x => x with { Name = action.Dto.Name },
			result: out var newCustomers)
		? state
		: state with { Customers = newCustomers };

	/// <summary>
	/// When another part of the app saves a customer on the server, we need to update our state
	/// </summary>
	/// <param name="state"></param>
	/// <param name="action"></param>
	/// <returns></returns>
	[ReducerMethod]
	public static SearchCustomersState ReduceUpdateCustomerActionResult(SearchCustomersState state, UpdateCustomerActionResult action) =>
		!state.Customers.ReplaceOne(
			selector: x => x.Id == action.Dto.Id,
			replacement: x => x with { Name = action.Dto.Name },
			result: out var newCustomers)
		? state
		: state with { Customers = newCustomers };

	/// <summary>
	/// We only need search state if we are searching or editing.
	/// If the app navigates outside both of these use cases then we can
	/// empty the state to preserve memory
	/// </summary>
	/// <param name="state"></param>
	/// <param name="action"></param>
	/// <returns></returns>
	[ReducerMethod]
	public static SearchCustomersState ReduceGoAction(SearchCustomersState state, GoAction action) =>
		action.NewUri.Contains("/customers/search", StringComparison.OrdinalIgnoreCase)
		|| action.NewUri.Contains("/customer/edit/", StringComparison.OrdinalIgnoreCase)
		? state
		: SearchCustomersState.Empty;
}		
