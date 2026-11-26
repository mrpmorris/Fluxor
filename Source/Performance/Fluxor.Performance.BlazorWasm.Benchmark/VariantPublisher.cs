using System.Diagnostics;
using System.Text;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// Publishes one variant of the app with <c>dotnet publish -c Release</c> (no AOT: the default Mono interpreter,
/// all other settings as a new app has them). Each variant has its own artifacts folder, so the variants never
/// share build output.
/// </summary>
internal static class VariantPublisher
{
	private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(15);

	/// <summary>Publishes the variant and returns its <c>wwwroot</c> folder.</summary>
	public static string Publish(Variant variant)
	{
		string variantDirectory = Path.Combine(BenchmarkPaths.WorkDirectory, variant.Name);
		string publishDirectory = Path.Combine(variantDirectory, "publish");

		var startInfo = new ProcessStartInfo("dotnet")
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false
		};
		foreach (string argument in new[]
		{
			"publish", BenchmarkPaths.AppProjectPath,
			"-c", "Release",
			$"-p:FluxorSampleSize={variant.Size}",
			$"-p:FluxorSample={variant.Build}",
			// The library projects pack on a Release build; the packages are not needed here
			"-p:GeneratePackageOnBuild=false",
			"--artifacts-path", Path.Combine(variantDirectory, "artifacts"),
			"-o", publishDirectory,
			// No MSBuild nodes left running while the browser measures
			"-nodeReuse:false",
			"-nologo"
		})
		{
			startInfo.ArgumentList.Add(argument);
		}

		var output = new StringBuilder();
		using var process = new Process { StartInfo = startInfo };
		process.OutputDataReceived += (_, e) => Append(output, e.Data);
		process.ErrorDataReceived += (_, e) => Append(output, e.Data);
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		if (!process.WaitForExit(Timeout))
		{
			process.Kill(entireProcessTree: true);
			throw new TimeoutException($"Publishing {variant.Name} took more than {Timeout}:{Environment.NewLine}{output}");
		}
		process.WaitForExit();

		if (process.ExitCode != 0)
			throw new InvalidOperationException($"Publishing {variant.Name} failed ({process.ExitCode}):{Environment.NewLine}{output}");

		string wwwroot = Path.Combine(publishDirectory, "wwwroot");
		if (!File.Exists(Path.Combine(wwwroot, "index.html")))
			throw new InvalidOperationException($"Publishing {variant.Name} wrote no {wwwroot}{Path.DirectorySeparatorChar}index.html");
		return wwwroot;
	}

	private static void Append(StringBuilder output, string? line)
	{
		if (line is null)
			return;
		lock (output)
			output.AppendLine(line);
	}
}
