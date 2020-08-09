using Fluxor;

namespace BasicConcepts.ActionSubscriber.Store.EditCustomerUseCase
{
	public static class Reducers
	{
		[ReducerMethod]
		public static EditCustomerState Reduce(EditCustomerState state, GetCustomerForEditAction action) =>
			new EditCustomerState(isLoading: true);

		[ReducerMethod]
		public static EditCustomerState Reduce(EditCustomerState state, GetCustomerForEditResultAction action) =>
			new EditCustomerState(isLoading: false);
	}
}
