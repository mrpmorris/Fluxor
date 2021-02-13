using Fluxor.Blazor.Web.ReduxDevTools.Serialization;
using Fluxor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace Fluxor.Blazor.Web.ReduxDevTools
{
	/// <summary>
	/// Options class for Redux Dev Tools integration
	/// </summary>
	public class ReduxDevToolsMiddlewareOptions
	{
		private readonly FluxorOptions FluxorOptions;

		/// <summary>
		/// The name to display in the Redux Dev Tools window
		/// </summary>
		public string Name { get; set; } = "Fluxor";
		/// <summary>
		/// How often the Redux Dev Tools actions are updated.
		/// </summary>
		public TimeSpan Latency { get; set; } = TimeSpan.FromMilliseconds(50);
		/// <summary>
		/// How many actions to keep in the Redux Dev Tools history (maxAge setting).
		/// Default is 50.
		/// </summary>
		public ushort MaximumHistoryLength { get; set; } = 50;

		public ReduxDevToolsMiddlewareOptions(FluxorOptions fluxorOptions)
		{
			FluxorOptions = fluxorOptions;
		}

		/// <summary>
		/// Uses Newtonsoft JSON as the JSON serializer for Redux Dev Tools
		/// </summary>
		/// <param name="getSettings">Optional function to create JSON serialization settings</param>
		/// <returns></returns>
		public ReduxDevToolsMiddlewareOptions UseNewtonsoftJson(
			Func<IServiceProvider, JsonSerializerSettings> getSettings = null)
		{
			FluxorOptions.Services.AddScoped<IJsonSerialization, NewtonsoftJsonAdapter>(
				(Func<IServiceProvider, NewtonsoftJsonAdapter>)(				sp =>
				{
					JsonSerializerSettings settings = getSettings?.Invoke(sp);

/* Unmerged change from project 'Fluxor.Blazor.Web.ReduxDevTools (netstandard2.1)'
Before:
					return new NewtonsoftSerialization(settings);
After:
					return new Serialization.NewtonsoftSerialization(settings);
*/

/* Unmerged change from project 'Fluxor.Blazor.Web.ReduxDevTools (net5.0)'
Before:
					return new NewtonsoftSerialization(settings);
After:
					return new Serialization.NewtonsoftSerialization(settings);
*/
					return (NewtonsoftJsonAdapter)new NewtonsoftJsonAdapter(settings);
				}));
			return this;
		}

		/// <summary>
		/// Uses Newtonsoft JSON as the JSON serializer for Redux Dev Tools
		/// </summary>
		/// <param name="getOptions">Optional function to create JSON serialization options</param>
		/// <returns></returns>
		public ReduxDevToolsMiddlewareOptions UseSystemTextJson(
			Func<IServiceProvider, JsonSerializerOptions> getOptions = null)
		{
			FluxorOptions.Services.AddScoped<IJsonSerialization, SystemTextJsonAdapter>(
				(Func<IServiceProvider, SystemTextJsonAdapter>)(				sp =>
				{
					JsonSerializerOptions jsonOptions = getOptions?.Invoke(sp);

/* Unmerged change from project 'Fluxor.Blazor.Web.ReduxDevTools (netstandard2.1)'
Before:
					return new SystemTextSerialization(jsonOptions);
After:
					return new Serialization.SystemTextSerialization(jsonOptions);
*/

/* Unmerged change from project 'Fluxor.Blazor.Web.ReduxDevTools (net5.0)'
Before:
					return new SystemTextSerialization(jsonOptions);
After:
					return new Serialization.SystemTextSerialization(jsonOptions);
*/
					return (SystemTextJsonAdapter)new SystemTextJsonAdapter(jsonOptions);
				}));
			return this;
		}
	}
}
