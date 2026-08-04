using Fluxor.UnsupportedClasses;
using MauiReactor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using System;

namespace Fluxor.Reactor.Maui.Components;

/// <summary>
/// A component that auto-subscribes to state changes on all <see cref="IStateChangedNotifier"/> properties
/// and ensures <see cref="ComponentBase.StateHasChanged"/> is called
/// </summary>
public abstract class FluxorComponent : Component
{
	private readonly IActionSubscriber ActionSubscriber;

	private IDisposable StateSubscription;
	private readonly ThrottledInvoker StateHasChangedThrottler;

	public static StoreInitializer StoreInitializer(VisualNode content)
	{
		var storeInitializer = new StoreInitializer
		{
			content
		};

		return storeInitializer;
	}

	/// <summary>
	/// Creates a new instance
	/// </summary>
	public FluxorComponent()
	{
		ActionSubscriber = Services.GetRequiredService<IActionSubscriber>();

		StateHasChangedThrottler = new ThrottledInvoker(() =>
		{
			if (_isMounted)
				MainThread.InvokeOnMainThreadAsync(Invalidate);
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
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				if (_isMounted)
					callback(action);
				Invalidate();
			});
		});
	}

	/// <summary>
	/// Subscribes to state properties
	/// </summary>
	protected override void OnMounted()
	{
		base.OnMounted();
		StateSubscription = StateSubscriber.Subscribe(this, _ =>
		{
			StateHasChangedThrottler.Invoke(MaximumStateChangedNotificationsPerSecond);
		});
	}

	/// <summary>
	/// Unsubscribes from state properties
	/// </summary>
	/// <exception cref="NullReferenceException"></exception>
	/// <exception cref="NullReferenceException">
	///		Thrown when a descendant overrides DisposeAsyncCore and does call not base.
	/// </exception>
	protected override void OnWillUnmount()
	{
		base.OnWillUnmount();
		if (StateSubscription is null)
			throw new NullReferenceException(ErrorMessages.ForgottenToCallBaseOnMounted);

		StateSubscription.Dispose();
		ActionSubscriber?.UnsubscribeFromAllActions(this);
	}
}
