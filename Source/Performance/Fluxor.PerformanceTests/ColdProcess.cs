using System.Diagnostics;
using System.Text.Json;

namespace Fluxor.PerformanceTests;

/// <summary>
/// Starts Fluxor.Performance.Runner as a new process (a cold start: nothing of Fluxor or the sample is loaded or
/// compiled yet) and reads the one JSON line it prints.
/// </summary>
internal static class ColdProcess
{
	public static readonly string[] PhaseNames = ["addFluxor", "buildProvider", "resolveStore", "initialize", "firstDispatch"];

	/// <summary>Runs the runner once for <paramref name="variant"/>, with the runner's extra options if any.</summary>
	public static RunResult Run(string variant, params string[] options)
	{
		var startInfo = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false
		};
		startInfo.ArgumentList.Add(TestPaths.RunnerPath);
		startInfo.ArgumentList.Add(variant);
		foreach (string option in options)
			startInfo.ArgumentList.Add(option);

		using Process process = Process.Start(startInfo)!;
		Task<string> standardError = process.StandardError.ReadToEndAsync();
		string standardOutput = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		Assert.True(
			process.ExitCode == 0,
			$"Runner {variant} exited with {process.ExitCode}: {standardError.Result}");

		string json = standardOutput
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Last();
		return RunResult.Parse(json);
	}
}

/// <summary>What one run of Fluxor.Performance.Runner measured and the store it built.</summary>
/// <param name="Phases">Milliseconds per phase</param>
/// <param name="JitMilliseconds">Milliseconds the JIT spent in each phase</param>
/// <param name="JitMethods">Methods the JIT compiled in each phase</param>
/// <param name="Features">Features in the store</param>
/// <param name="Reducers">Reducers in the store</param>
/// <param name="Effects">Effects in the store</param>
/// <param name="HasGeneratedRegistration">True when the sample assembly has a generated registration</param>
/// <param name="GeneratedMethods">Methods (and constructors) of the generated registration</param>
/// <param name="GeneratedReducers">Reducers created by generated code</param>
/// <param name="GeneratedEffects">Effects created by generated code</param>
/// <param name="Types">Types in the sample assembly (T), counted after measuring</param>
/// <param name="Methods">Public methods of those types, inherited ones included (M), counted after measuring</param>
/// <param name="Dispatch">The warm dispatch timing, when the runner was asked for it</param>
internal sealed record RunResult(
	IReadOnlyDictionary<string, double> Phases,
	IReadOnlyDictionary<string, double> JitMilliseconds,
	IReadOnlyDictionary<string, double> JitMethods,
	int Features,
	int Reducers,
	int Effects,
	bool HasGeneratedRegistration,
	int GeneratedMethods,
	int GeneratedReducers,
	int GeneratedEffects,
	int Types,
	int Methods,
	DispatchResult? Dispatch)
{
	public static RunResult Parse(string json)
	{
		using JsonDocument document = JsonDocument.Parse(json);
		JsonElement root = document.RootElement;
		JsonElement phases = root.GetProperty("phases");
		JsonElement jit = root.GetProperty("jit");
		DispatchResult? dispatch = root.TryGetProperty("dispatch", out JsonElement dispatchElement)
			? new DispatchResult(
				dispatchElement.GetProperty("batchMicroseconds").EnumerateArray().Select(x => x.GetDouble()).ToArray(),
				dispatchElement.GetProperty("dispatchesPerBatch").GetInt32(),
				dispatchElement.GetProperty("reducers").GetInt32(),
				dispatchElement.GetProperty("effects").GetInt32())
			: null;
		return new RunResult(
			ColdProcess.PhaseNames.ToDictionary(x => x, x => phases.GetProperty(x).GetDouble()),
			ColdProcess.PhaseNames.ToDictionary(x => x, x => jit.GetProperty(x).GetProperty("ms").GetDouble()),
			ColdProcess.PhaseNames.ToDictionary(x => x, x => jit.GetProperty(x).GetProperty("methods").GetDouble()),
			root.GetProperty("features").GetInt32(),
			root.GetProperty("reducers").GetInt32(),
			root.GetProperty("effects").GetInt32(),
			root.GetProperty("hasGeneratedRegistration").GetBoolean(),
			root.GetProperty("generatedMethods").GetInt32(),
			root.GetProperty("generatedReducers").GetInt32(),
			root.GetProperty("generatedEffects").GetInt32(),
			root.GetProperty("types").GetInt32(),
			root.GetProperty("methods").GetInt32(),
			dispatch);
	}
}

/// <summary>Warm dispatches timed in the runner after it built the store.</summary>
/// <param name="BatchMicroseconds">Microseconds per dispatch, one value per batch</param>
/// <param name="DispatchesPerBatch">Dispatches in each batch</param>
/// <param name="Reducers">Reducers in the store that handle the dispatched action</param>
/// <param name="Effects">Effects in the store that react to the dispatched action</param>
internal sealed record DispatchResult(IReadOnlyList<double> BatchMicroseconds, int DispatchesPerBatch, int Reducers, int Effects)
{
	/// <summary>Microseconds per dispatch: the median of the batches.</summary>
	public double MedianMicroseconds => Statistics.Median(BatchMicroseconds);
}

internal static class Statistics
{
	public static double Median(IEnumerable<double> values)
	{
		double[] sorted = values.Order().ToArray();
		if (sorted.Length == 0)
			throw new ArgumentException("No values", nameof(values));
		int middle = sorted.Length / 2;
		return sorted.Length % 2 == 1
			? sorted[middle]
			: (sorted[middle - 1] + sorted[middle]) / 2;
	}

	public static double Median<T>(IEnumerable<T> items, Func<T, double> selector) => Median(items.Select(selector));

	/// <summary>The least squares slope of y against x.</summary>
	public static double Slope(IReadOnlyList<double> x, IReadOnlyList<double> y)
	{
		if (x.Count != y.Count || x.Count < 2)
			throw new ArgumentException("Needs two or more points, as many x as y");
		double meanX = x.Average();
		double meanY = y.Average();
		double covariance = 0;
		double variance = 0;
		for (int index = 0; index < x.Count; index++)
		{
			covariance += (x[index] - meanX) * (y[index] - meanY);
			variance += (x[index] - meanX) * (x[index] - meanX);
		}
		return covariance / variance;
	}

	/// <summary>The least squares slope of log(y) against log(x): the exponent b of y = a * x^b.</summary>
	public static double LogLogSlope(IReadOnlyList<double> x, IReadOnlyList<double> y) =>
		Slope(x.Select(value => Math.Log(value)).ToArray(), y.Select(value => Math.Log(value)).ToArray());
}
