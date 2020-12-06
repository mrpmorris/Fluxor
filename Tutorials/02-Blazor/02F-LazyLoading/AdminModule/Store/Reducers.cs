using Fluxor;

namespace BlazorLazyLoading.AdminModule.Store
{
	public static class Reducers
	{
		[ReducerMethod]
		public static AdminState ChangeNameAction(AdminState state, ChangeNameAction action) =>
			state with { Name = action.NewName };
	}
}
