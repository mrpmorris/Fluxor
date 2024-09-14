using Fluxor.Blazor.Web.Components;

namespace Fluxor.Blazor.Web.UnitTests.SupportFiles;

public class FluxorComponentThatOptionallyCallsBaseOnInitialized : FluxorComponent
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
