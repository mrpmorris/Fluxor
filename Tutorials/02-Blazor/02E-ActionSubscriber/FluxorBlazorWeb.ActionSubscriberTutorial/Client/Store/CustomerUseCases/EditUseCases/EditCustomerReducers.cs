using Fluxor;
using Fluxor.Blazor.Web.Middlewares.Routing;

namespace FluxorBlazorWeb.ActionSubscriberTutorial.Client.Store.CustomerUseCases.EditUseCases;

public static class EditCustomerReducers
{
	/// <summary>
	/// Any navigation should cause the state to empty
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	[ReducerMethod(typeof(GoAction))]
	public static EditCustomerState ReduceGoAction(EditCustomerState state) => EditCustomerState.Empty;

	/// <summary>
	/// Set IsLoading to `true` when asking the server for a customer object to edit
	/// </summary>
	/// <param name="_"></param>
	/// <returns></returns>
	[ReducerMethod(typeof(GetCustomerForEditAction))]
	public static EditCustomerState ReduceGetCustomerForEditAction(EditCustomerState _) =>
		EditCustomerState.Empty with { IsLoading = true };

	/// <summary>
	/// Set IsLoading to `false` when the server responds with a customer object to edit
	/// </summary>
	/// <param name="_"></param>
	/// <returns></returns>
	[ReducerMethod(typeof(GetCustomerForEditActionResult))]
	public static EditCustomerState ReduceGetCustomerForEditActionResult(EditCustomerState _) =>
		EditCustomerState.Empty with { IsLoading = false };

	/// <summary>
	/// Set IsSaving to `true` when telling the server to update a customer object
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	[ReducerMethod(typeof(UpdateCustomerAction))]
	public static EditCustomerState ReduceUpdateCustomerAction(EditCustomerState state) =>
		state with { IsSaving = true };

	/// <summary>
	/// Set IsSaving to `false` when the serve has saved the customer object
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	[ReducerMethod(typeof(UpdateCustomerActionResult))]
	public static EditCustomerState ReduceUpdateCustomerActionResult(EditCustomerState state) =>
		state with { IsSaving = false };

}
