using Fluxor.UnsupportedClasses;
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
		private ThrottledInvoker StateHasChangedThrottler;

		/// <summary>
		/// Creates a new instance
		/// </summary>
		public FluxorComponent()
		{
			StateHasChangedThrottler = new ThrottledInvoker(() =>
			{
				if (!Disposed)
					InvokeAsync(StateHasChanged);
			});
		}

		/// <summary>
		/// If greater than 0, the feature will not execute state changes
		/// more often than this many times per second. Additional notifications
		/// will be surpressed, and observers will be notified of the latest
		/// state when the time window has elapsed to allow another notification.
		/// </summary>
		protected byte MaximumStateChangedNotificationsPerSecond { get; set; }

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
				StateHasChangedThrottler.Invoke(MaximumStateChangedNotificationsPerSecond);
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
