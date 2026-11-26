using Fluxor.DependencyInjection;
using Fluxor.DependencyInjection.Generated;
using Fluxor.UnitTests.DependencyInjectionTests.GeneratedRegistrationTests;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.SourceGenerator.UnitTests;

[CollectionDefinition(nameof(GeneratedRegistrationSwitchCollection), DisableParallelization = true)]
public class GeneratedRegistrationSwitchCollection { }

/// <summary>
/// The store built by <c>AddFluxor</c> for an assembly compiled by a test, with the state after dispatching
/// every action of the assembly.
/// </summary>
public sealed record RunResult(StoreShape Shape, List<string> DispatchLog, bool HasGeneratedRegistration);

/// <summary>
/// Loads a compiled fixture into a new collectible <see cref="AssemblyLoadContext"/>, runs <c>AddFluxor</c> on it
/// with the generated registration switched on or off, and dispatches every action it declares.
/// </summary>
public static class FixtureRunner
{
	public const string DisableSwitchName = "Fluxor.DisableGeneratedRegistration";

	public static string ReadFixture(string fileName)
	{
		using Stream stream = typeof(FixtureRunner).Assembly.GetManifestResourceStream("Fixtures." + fileName)
			?? throw new InvalidOperationException("Fixture not found: " + fileName);
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	/// <param name="output">The compiled fixture</param>
	/// <param name="referenced">A compilation the fixture references, loaded alongside it, or null</param>
	/// <param name="middlewareToAdd">The full name of a fixture type to add with <c>AddMiddleware</c>, or null</param>
	/// <param name="disableGenerated">The value of the <c>Fluxor.DisableGeneratedRegistration</c> switch</param>
	public static Task<RunResult> RunAsync(
		Compilation output,
		Compilation referenced,
		string middlewareToAdd,
		bool disableGenerated) =>
		RunAsync(output, referenced is null ? Array.Empty<Compilation>() : new[] { referenced }, middlewareToAdd, disableGenerated);

	/// <param name="output">The compiled fixture</param>
	/// <param name="referenced">Compilations the fixture references, loaded alongside it in this order</param>
	/// <param name="middlewareToAdd">The full name of a fixture type to add with <c>AddMiddleware</c>, or null</param>
	/// <param name="disableGenerated">The value of the <c>Fluxor.DisableGeneratedRegistration</c> switch</param>
	public static async Task<RunResult> RunAsync(
		Compilation output,
		Compilation[] referenced,
		string middlewareToAdd,
		bool disableGenerated)
	{
		Assembly assembly = GeneratorTestHost.LoadAssembly(output, referenced);
		bool hadValue = AppContext.TryGetSwitch(DisableSwitchName, out bool previous);
		AppContext.SetSwitch(DisableSwitchName, disableGenerated);
		try
		{
			var services = new ServiceCollection();
			services.AddFluxor(options =>
			{
				options.ScanAssemblies(assembly);
				if (middlewareToAdd is not null)
					AddMiddleware(options, assembly.GetType(middlewareToAdd, throwOnError: true));
			});

			StoreShape shape = StoreShape.Capture(services);
			List<string> dispatchLog = await DispatchEveryActionAsync(services, assembly);
			return new RunResult(shape, dispatchLog, assembly.IsDefined(typeof(FluxorGeneratedRegistrationAttribute)));
		}
		finally
		{
			AppContext.SetSwitch(DisableSwitchName, hadValue && previous);
			AssemblyLoadContext.GetLoadContext(assembly).Unload();
		}
	}

	private static void AddMiddleware(FluxorOptions options, Type middlewareType) =>
		typeof(FluxorOptions)
			.GetMethod(nameof(FluxorOptions.AddMiddleware))
			.MakeGenericMethod(middlewareType)
			.Invoke(options, null);

	/// <summary>
	/// Dispatches one instance of every public type named *Action with a parameterless constructor, in name
	/// order, recording every action the store processes (follow-up actions from effects too) with the state
	/// of every feature after it.
	/// </summary>
	private static async Task<List<string>> DispatchEveryActionAsync(IServiceCollection services, Assembly assembly)
	{
		using ServiceProvider rootProvider = services.BuildServiceProvider();
		using IServiceScope scope = rootProvider.CreateScope();
		IServiceProvider serviceProvider = scope.ServiceProvider;

		var store = serviceProvider.GetRequiredService<IStore>();
		var exceptions = new List<Exception>();
		store.UnhandledException += (_, e) => exceptions.Add(e.Exception);
		await store.InitializeAsync();

		var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
		var actionSubscriber = serviceProvider.GetRequiredService<IActionSubscriber>();
		var log = new List<string>();
		var subscriber = new object();
		actionSubscriber.SubscribeToAction<object>(subscriber, action => log.Add($"{action} -> {DescribeStates(store)}"));

		Type[] actionTypes = assembly.GetTypes()
			.Where(x => x.IsPublic || x.IsNestedPublic)
			.Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition && x.Name.EndsWith("Action"))
			.Where(x => x.GetConstructor(Type.EmptyTypes) is not null)
			.OrderBy(x => x.FullName, StringComparer.Ordinal)
			.ToArray();
		foreach (Type actionType in actionTypes)
			dispatcher.Dispatch(Activator.CreateInstance(actionType));

		actionSubscriber.UnsubscribeFromAllActions(subscriber);
		log.Add("final: " + DescribeStates(store));
		Assert.Empty(exceptions);
		return log;
	}

	private static string DescribeStates(IStore store) =>
		string.Join(", ", store.Features.Values.Select(x => $"{x.GetName()} = {x.GetState()}"));
}
