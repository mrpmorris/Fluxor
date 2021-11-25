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
	public class StoreInitializer : ComponentBase, IDisposable
	{
		[Parameter]
		public EventCallback<Exceptions.UnhandledExceptionEventArgs> UnhandledException { get; set; }

		[Inject]
		private IStore Store { get; set; }

		[Inject]
		private IJSRuntime JSRuntime { get; set; }

		private string MiddlewareInitializationScripts;
		private bool Disposed;
		private Exception ExceptionToThrow;

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
				if (script != null)
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
			if (ExceptionToThrow != null)
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

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Store.UnhandledException -= OnUnhandledException;
		}

		private void OnUnhandledException(object sender, Exceptions.UnhandledExceptionEventArgs args)
		{
			InvokeAsync(async () =>
			{
				Exception exceptionThrownInHandler = null;
				try
				{
					await UnhandledException.InvokeAsync(args).ConfigureAwait(false);
				}
				catch (Exception e)
				{
					exceptionThrownInHandler = e;
				}

				if (exceptionThrownInHandler != null || !args.WasHandled)
				{
					ExceptionToThrow = exceptionThrownInHandler ?? args.Exception;
					StateHasChanged();
				}
			});
		}

		void IDisposable.Dispose()
		{
			if (!Disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
				Disposed = true;
			}
		}
	}
}
