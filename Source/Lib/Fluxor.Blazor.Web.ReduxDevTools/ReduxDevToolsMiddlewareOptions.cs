using Fluxor.DependencyInjection;
using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Fluxor.Blazor.Web.ReduxDevTools;

/// <summary>
/// Used to determine if an action should be logged to Redux Dev Tools
/// or filtered out and ignored.
/// </summary>
/// <param name="action"></param>
/// <returns></returns>
public delegate bool ActionFilter(object action);

/// <summary>
/// Options class for Redux Dev Tools integration.
/// </summary>
public class ReduxDevToolsMiddlewareOptions
{
	private readonly FluxorOptions FluxorOptions;

	/// <summary>
	/// The name to display in the Redux Dev Tools window.
	/// </summary>
	public string Name { get; set; } = "Fluxor";

	/// <summary>
	/// JSON Serializer options for sending state to the 
	/// ReduxDevTools plugin.
	/// </summary>
	public JsonSerializerOptions JsonSerializerOptions { get; set; }

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

	/// <summary>
	/// Allows the developer to decide whether an action is logged via Redux Dev Tools
	/// or filtered out and ignored.
	/// </summary>
	public ImmutableArray<ActionFilter> ActionFilters { get; private set; } = ImmutableArray.Create<ActionFilter>();

	internal Regex StackTraceFilterRegex { get; private set; }


	public ReduxDevToolsMiddlewareOptions(FluxorOptions fluxorOptions)
	{
		FluxorOptions = fluxorOptions;
		JsonSerializerOptions = new JsonSerializerOptions {
			NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
		};
	}

	/// <summary>
	/// Enables action <see cref="ActionFilter"/>
	/// </summary>
	/// <param name="filter">Accepts the action object, should return `true` to log the action or `false` to ignore it.</param>
	/// <returns></returns>
	public ReduxDevToolsMiddlewareOptions AddActionFilter(ActionFilter filter)
	{
		ArgumentNullException.ThrowIfNull(filter);
		ActionFilters = ActionFilters.Add(filter);
		return this;
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
}
