using Microsoft.AspNetCore.Components;
using System;

namespace Fluxor.Blazor.Web.Components
{
	/// <summary>
	/// A component that auto-subscribes to state changes on all <see cref="IState"/> properties
	/// and ensures <see cref="ComponentBase.StateHasChanged"/> is called
	/// </summary>
	public class FluxorComponent : ComponentBase, IDisposable
	{
		[Inject]
		private IActionSubscriber ActionSubscriber { get; set; }

		private bool Disposed;
		private IDisposable StateSubscription;

		/// <see cref="IActionSubscriber.SubscribeToAction{TAction}(object, Action{TAction})"/>
		public void SubscribeToAction<TAction>(Action<TAction> callback)
		{
			ActionSubscriber.SubscribeToAction<TAction>(this, action =>
			{
				if (!Disposed)
					callback(action);
			});
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
			StateSubscription = StateSubscriber.Subscribe(this, _ =>
			{
				if (!Disposed)
					InvokeAsync(StateHasChanged);
			});
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				Disposed = true;
				if (disposing)
				{
					if (StateSubscription == null)
						throw new NullReferenceException(ErrorMessages.ForgottenToCallBaseOnInitialized);

					StateSubscription.Dispose();
					ActionSubscriber?.UnsubscribeFromAllActions(this);
				}
			}
		}
	}
}
