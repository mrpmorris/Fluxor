using Microsoft.AspNetCore.Components;
using System;

namespace Fluxor.Blazor.Web.Components
{
	/// <summary>
	/// A layout that auto-subscribes to state changes on all <see cref="IState"/> properties
	/// and ensures <see cref="LayoutComponentBase.StateHasChanged"/> is called
	/// </summary>
	public class FluxorLayout : LayoutComponentBase, IDisposable
	{
		[Inject]
		private IActionSubscriber ActionSubscriber { get; set; }

		private bool Disposed;
		private IDisposable StateSubscription;

		/// <see cref="IActionSubscriber.SubscribeToAction{TAction}(object, Action{TAction})"/>
		public void SubscribeToAction<TAction>(Action<TAction> callback)
		{
			ActionSubscriber.SubscribeToAction<TAction>(this, action => callback(action));
		}

		/// <summary>
		/// Disposes of the component and unsubscribes from any state
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Subscribes to state properties
		/// </summary>
		protected override void OnInitialized()
		{
			base.OnInitialized();
			StateSubscription = StateSubscriber.Subscribe(this, _ => InvokeAsync(StateHasChanged));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					StateSubscription.Dispose();
					ActionSubscriber?.UnsubscribeFromAllActions(this);
				}
				Disposed = true;
			}
		}
	}
}
