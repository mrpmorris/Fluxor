using Fluxor;

namespace BlazorLazyLoading.AdminModule.Store
{
	public class AdminFeature : Feature<AdminState>
	{
		public override string GetName() => "Admin";
		protected override AdminState GetInitialState() => new AdminState(null);
	}
}
