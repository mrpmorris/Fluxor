using Microsoft.AspNetCore.Components;

namespace Fluxor.Blazor.Web.Components
{
	public abstract class FluxorDispatchComponent<T> : FluxorComponent
	{
		[Inject]
		protected IState<T> State { get; set; }

		[Inject]
		protected IDispatcher Dispatcher { get; set; }
	}
}
