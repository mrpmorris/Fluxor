using Fluxor.Reactor.Maui.Components;
using Fluxor.Exceptions;
using MauiReactor;
using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Fluxor.Reactor.Maui;

/// <summary>
/// Initializes the store for the current user. This should be placed in the root app component.
/// </summary>
public partial class StoreInitializer : FluxorComponent
{
	[Inject]
	private IStore Store;

	private Exception ExceptionToThrow;

	public override VisualNode Render()
	{
		return new ContentView();
	}

	/// <summary>
	/// Disposes via IAsyncDisposable
	/// </summary>
	/// <param name="disposing">true if called manually, otherwise false</param>
	protected override ValueTask DisposeAsyncCore(bool disposing)
	{
		if (disposing)
			Store.UnhandledException -= OnUnhandledException;
		return base.DisposeAsyncCore(disposing);
	}

	/// <summary>
	/// Retrieves supporting JavaScript for any Middleware
	/// </summary>
	protected override void OnMounted()
	{
		Store.UnhandledException += OnUnhandledException;

		base.OnMounted();
	}

	protected override void OnPropsChanged()
	{
		base.OnPropsChanged();
		OnAfterRender();
	}

	protected override void OnWillUnmount()
	{
		base.OnWillUnmount();
		OnAfterRender();
	}

	private void OnAfterRender()
	{
		if (ExceptionToThrow is not null)
		{
			Exception exception = ExceptionToThrow;
			ExceptionToThrow = null;
			ExceptionDispatchInfo.Capture(exception).Throw();
		}
	}

	private void OnUnhandledException(object sender, Exceptions.UnhandledExceptionEventArgs e)
	{
		MainThread.InvokeOnMainThreadAsync(async () =>
		{
			Exception exceptionThrownInHandler = e.Exception;

			if (exceptionThrownInHandler is not null || !e.WasHandled)
			{
				ExceptionToThrow = exceptionThrownInHandler ?? e.Exception;
				Invalidate();
			}
		});
	}
}
