using Fluxor.Blazor.Web.Components;
using Fluxor.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fluxor.Blazor.Web
{
	/// <summary>
	/// Initializes the store for the current user. This should be placed in the App.razor component.
	/// </summary>
	public class StoreInitializer : FluxorComponent
	{
		[Parameter]
		public EventCallback<Exceptions.UnhandledExceptionEventArgs> UnhandledException { get; set; }

		[Inject]
		private IStore Store { get; set; }

		[Inject]
		private IJSRuntime JSRuntime { get; set; }

		private string MiddlewareInitializationScripts;
		private Exception ExceptionToThrow;

		/// <summary>
		/// Disposes via IAsyncDisposable
		/// </summary>
		/// <param name="disposing">true if called manually, otherwise false</param>
		protected override ValueTask DisposeAsyncCore(bool disposing)
		{
			if (disposing)
				Store.UnhandledException -= OnUnhandledException;
			return ValueTask.CompletedTask;
		}

		/// <summary>
		/// Retrieves supporting JavaScript for any Middleware
		/// </summary>
		protected override void OnInitialized()
		{
			Store.UnhandledException += OnUnhandledException;

			var webMiddlewares = Store.GetMiddlewares().OfType<IWebMiddleware>();

			var scriptBuilder = new StringBuilder();
			foreach (IWebMiddleware middleware in webMiddlewares)
			{
				string script = middleware.GetClientScripts();
				if (script is not null)
				{
					scriptBuilder.AppendLine($"// Middleware scripts: {middleware.GetType().FullName}");
					scriptBuilder.AppendLine(script);
				}
			}
			MiddlewareInitializationScripts = scriptBuilder.ToString();
			base.OnInitialized();
		}

		protected override void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
			if (ExceptionToThrow is not null)
			{
				Exception exception = ExceptionToThrow;
				ExceptionToThrow = null;
				throw exception;
			}
		}

		/// <summary>
		/// Executes any supporting JavaScript required for Middleware
		/// </summary>
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(MiddlewareInitializationScripts))
						await JSRuntime.InvokeVoidAsync("eval", MiddlewareInitializationScripts);

					await Store.InitializeAsync();
				}
				catch (JSException err)
				{
					// An error in some JavaScript, cannot recover from this
					throw new StoreInitializationException("JavaScript error", err);
				}
				catch (TaskCanceledException)
				{
					// The browser has disconnected from a server-side-blazor app and can no longer be reached.
					// Swallow this exception as the store will be abandoned and garbage collected.
					return;
				}
				catch (Exception err)
				{
					throw new StoreInitializationException("Store initialization error", err);
				}
			}
		}

		private void OnUnhandledException(object sender, Exceptions.UnhandledExceptionEventArgs e)
		{
			InvokeAsync(async () =>
			{
				Exception exceptionThrownInHandler = null;
				try
				{
					await UnhandledException.InvokeAsync(e).ConfigureAwait(false);
				}
				catch (Exception exception)
				{
					exceptionThrownInHandler = exception;
				}

				if (exceptionThrownInHandler is not null || !e.WasHandled)
				{
					ExceptionToThrow = exceptionThrownInHandler ?? e.Exception;
					StateHasChanged();
				}
			});
		}
	}
}
