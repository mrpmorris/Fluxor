using Fluxor.UnsupportedClasses;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web.Components
{
	/// <summary>
	/// A component that auto-subscribes to state changes on all <see cref="IStateChangedNotifier"/> properties
	/// and ensures <see cref="ComponentBase.StateHasChanged"/> is called
	/// </summary>
	public abstract class FluxorComponent : ComponentBase, IAsyncDisposable
	{
		[Inject]
		private IActionSubscriber ActionSubscriber { get; set; }

		private bool Disposed;
		private IDisposable StateSubscription;
		private readonly ThrottledInvoker StateHasChangedThrottler;

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
		/// will be supressed, and observers will be notified of the latest
		/// state when the time window has elapsed to allow another notification.
		/// </summary>
		protected byte MaximumStateChangedNotificationsPerSecond { get; set; }

		/// <see cref="IActionSubscriber.SubscribeToAction{TAction}(object, Action{TAction})"/>
		public void SubscribeToAction<TAction>(Action<TAction> callback)
		{
			ActionSubscriber.SubscribeToAction<TAction>(this, action =>
			{
				InvokeAsync(() =>
				{
					if (!Disposed)
						callback(action);
					StateHasChanged();
				});
			});
		}

		/// <summary>
		/// Disposes of the component and unsubscribes from any state
		/// </summary>
		public async ValueTask DisposeAsync()
		{
			if (Disposed)
				return;

			StateHasChangedThrottler.Dispose();
			await DisposeAsyncCore(true).ConfigureAwait(false);
			GC.SuppressFinalize(this);
			Disposed = true;
		}

		/// <summary>
		/// Disposes via IAsyncDisposable
		/// </summary>
		/// <param name="disposing">true if called manually, otherwise false</param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException">
		///		Thrown when a descendant overrides DisposeAsyncCore and does call base.
		/// </exception>
		protected virtual ValueTask DisposeAsyncCore(bool disposing)
		{
			if (disposing)
			{
				if (StateSubscription is null)
					throw new NullReferenceException(ErrorMessages.ForgottenToCallBaseOnInitialized);

				StateSubscription.Dispose();
				ActionSubscriber?.UnsubscribeFromAllActions(this);
			}
			return ValueTask.CompletedTask;
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
	}
}
