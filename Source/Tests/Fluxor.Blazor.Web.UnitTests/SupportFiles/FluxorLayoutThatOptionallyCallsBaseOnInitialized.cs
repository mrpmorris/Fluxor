using Fluxor.Blazor.Web.Components;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles;

public class FluxorLayoutThatOptionallyCallsBaseOnInitialized : FluxorLayout
{
	private bool CallBaseOnInitialized;

	protected override void OnInitialized()
	{
		if (CallBaseOnInitialized)
			base.OnInitialized();
	}

	public void Test_OnInitialized(bool callBase)
	{
		CallBaseOnInitialized = callBase;
		OnInitialized();
	}
}
