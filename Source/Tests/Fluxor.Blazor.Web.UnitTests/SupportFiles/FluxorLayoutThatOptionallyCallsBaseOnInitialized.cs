using Fluxor.Blazor.Web.Components;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles;

public class FluxorLayoutThatOptionallyCallsBaseOnInitialized : FluxorLayout
{
	public bool CallBaseOnInitialized;

	protected override void OnInitialized()
	{
		if (CallBaseOnInitialized)
			base.OnInitialized();
	}

	public void Test_OnInitialized() => OnInitialized();
}
