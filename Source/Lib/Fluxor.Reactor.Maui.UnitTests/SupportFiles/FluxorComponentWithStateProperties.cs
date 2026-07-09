using Fluxor.Reactor.Maui.Components;
using MauiReactor;

namespace Fluxor.Reactor.Maui.UnitTests.SupportFiles;

public class FluxorComponentWithStateProperties : FluxorComponent
{
	public IState<int> State1 { get; set; }
	public IState<int> State2 { get; set; }

	public override VisualNode Render()
	{
		throw new System.NotImplementedException();
	}

	public void Test_OnMounted()
	{
		OnMounted();
	}
}
