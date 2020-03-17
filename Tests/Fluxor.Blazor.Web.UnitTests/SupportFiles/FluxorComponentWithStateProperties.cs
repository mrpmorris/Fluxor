using Fluxor.Blazor.Web.Components;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles
{
	public class FluxorComponentWithStateProperties : FluxorComponent
	{
		public IState State1 { get; set; }
		public IState State2 { get; set; }

		public void ExecuteOnInitialized()
		{
			OnInitialized();
		}
	}
}
