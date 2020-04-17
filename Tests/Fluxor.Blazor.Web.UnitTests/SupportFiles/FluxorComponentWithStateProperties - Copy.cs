using Fluxor.Blazor.Web.Components;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles
{
	public class FluxorLayoutWithStateProperties : FluxorLayout
	{
		public IState<int> State1 { get; set; }
		public IState<int> State2 { get; set; }

		public void ExecuteOnInitialized()
		{
			OnInitialized();
		}
	}
}
