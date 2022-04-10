using Fluxor.Blazor.Web.ReduxDevTools.Serialization;
using Fluxor.DependencyInjection;
using Fluxor.Extensions;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;

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
		/// <summary>
		/// When enabled, the stack trace that led to the dispatch of an action will
		/// be displayed in Redux Dev Tools.
		/// </summary>
		public bool StackTraceEnabled { get; private set; }
		/// <summary>
		/// Specifies how many stack frames to show in Redux Dev Tools for each action.
		/// Less than or equal to zero means show all.
		/// </summary>
		public int StackTraceLimit { get; private set; }
		internal Regex StackTraceFilterRegex { get; private set; }


		public ReduxDevToolsMiddlewareOptions(FluxorOptions fluxorOptions)
		{
			FluxorOptions = fluxorOptions;
		}


		/// <summary>
		/// Enables stack trace in Redux Dev Tools
		/// <see cref="StackTraceEnabled"/>
		/// </summary>
		/// <param name="limit"><see cref="StackTraceLimit"/></param>
		/// <param name="stackTraceFilterExpression">
		///		A regex expression to specify which stack frames should be included. The
		///		default value will exclude any stack frames from
		///		System, Microsoft, ExecuteMiddlewareAfterDispatch, or ReduxDevTools.
		///		You can include all frames by passing an empty string to this parameter.
		/// </param>
#if !NET5_0_OR_GREATER
		[Obsolete("StackTrace does not work in Blazor on .net 3.1")]
#endif
		public ReduxDevToolsMiddlewareOptions EnableStackTrace(
			int limit = 0,
			string stackTraceFilterExpression =
				@"^(?:(?!\b" +
				@"System" +
				@"|Microsoft" +
				@"|ExecuteMiddlewareAfterDispatch" +
				@"|ReduxDevTools" +
				@"\b).)*$")
		{
			StackTraceEnabled = true;
			StackTraceLimit = Math.Max(0, limit);
			if (!string.IsNullOrWhiteSpace(stackTraceFilterExpression))
				StackTraceFilterRegex = new Regex(stackTraceFilterExpression, RegexOptions.Compiled);
			return this;
		}

		/// <summary>
		/// Uses Newtonsoft JSON as the JSON serializer for Redux Dev Tools
		/// </summary>
		/// <param name="getSettings">Optional function to create JSON serialization settings</param>
		/// <returns></returns>
		public ReduxDevToolsMiddlewareOptions UseNewtonsoftJson(
			Func<IServiceProvider, JsonSerializerSettings> getSettings = null)
		{
			var implementationFactory = new Func<IServiceProvider, NewtonsoftJsonAdapter>(sp =>
			{
				JsonSerializerSettings settings = getSettings?.Invoke(sp);
				return new NewtonsoftJsonAdapter(settings);
			});
			FluxorOptions.Services.Add<IJsonSerialization, NewtonsoftJsonAdapter>(implementationFactory, FluxorOptions);
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
			var implementationFactory = new Func<IServiceProvider, SystemTextJsonAdapter>(sp =>
			{
				JsonSerializerOptions jsonOptions = getOptions?.Invoke(sp);
				return new SystemTextJsonAdapter(jsonOptions);
			});
			FluxorOptions.Services.Add<IJsonSerialization, SystemTextJsonAdapter>(implementationFactory, FluxorOptions);
			return this;
		}
	}
}
