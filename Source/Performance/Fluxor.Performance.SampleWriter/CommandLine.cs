using System.Globalization;

namespace Fluxor.Performance.Samples;

/// <summary>Reads <see cref="SampleOptions"/> from command line arguments.</summary>
public static class CommandLine
{
	/// <summary>
	/// Returns the options, the <c>--out</c> directory, the <c>--base-out</c> directory (null when not given), the
	/// <c>--size</c> sample and the <c>--scaling</c> sample (null when not given; their options are the start, later
	/// options change them). Samples are written at build time only, into the project's obj folder, so <c>--out</c>
	/// is required.
	/// </summary>
	public static (SampleOptions Options, string OutputDirectory, string? BaseOutputDirectory, SizedSample? Size, ScalingSample? Scaling) Parse(
		IReadOnlyList<string> args)
	{
		SampleOptions options = SampleOptions.Default;
		string? outputDirectory = null;
		string? baseOutputDirectory = null;
		SizedSample? size = null;
		ScalingSample? scaling = null;
		for (int index = 0; index < args.Count; index++)
		{
			string name = args[index];
			if (index + 1 >= args.Count)
				throw new ArgumentException($"{name} needs a value");
			string value = args[++index];

			if (name == "--size")
			{
				size = SizedSample.Get(value);
				options = size.Options;
				continue;
			}
			if (name == "--scaling")
			{
				scaling = ScalingSample.Get(value);
				options = scaling.Options;
				continue;
			}

			options = name switch
			{
				"--profile" => options with { Profile = ParseProfile(value) },
				"--states" => options with { States = ParseInt(name, value) },
				"--reducers-per-state" => options with { ReducersPerState = ParseInt(name, value) },
				"--effects-per-state" => options with { EffectsPerState = ParseInt(name, value) },
				"--total-reducers" => options with { TotalReducers = ParseInt(name, value) },
				"--total-effects" => options with { TotalEffects = ParseInt(name, value) },
				"--feature-class-states" => options with { FeatureClassStates = ParseInt(name, value) },
				"--private-constructor-states" => options with { PrivateConstructorStates = ParseInt(name, value) },
				"--reducer-classes" => options with { ReducerClasses = ParseInt(name, value) },
				"--effect-classes" => options with { EffectClasses = ParseInt(name, value) },
				"--reducer-hosts" => options with { ReducerHostClasses = ParseInt(name, value) },
				"--effect-hosts" => options with { EffectHostClasses = ParseInt(name, value) },
				"--plain-classes" => options with { PlainClasses = ParseInt(name, value) },
				"--methods-per-plain-class" => options with { MethodsPerPlainClass = ParseInt(name, value) },
				"--namespace" => options with { Namespace = value },
				"--out" or "--base-out" => options,
				_ => throw new ArgumentException($"Unknown option {name}")
			};
			if (name == "--out")
				outputDirectory = value;
			else if (name == "--base-out")
				baseOutputDirectory = value;
		}

		if (outputDirectory is null)
			throw new ArgumentException("--out is required: samples are written at build time only, into the project's obj folder");
		options.Validate();
		return (options, outputDirectory, baseOutputDirectory, size, scaling);
	}

	private static SampleProfile ParseProfile(string value) =>
		Enum.TryParse(value, ignoreCase: true, out SampleProfile profile) && Enum.IsDefined(profile)
			? profile
			: throw new ArgumentException($"Unknown profile {value}; expected one of: {string.Join(", ", Enum.GetNames<SampleProfile>()).ToLowerInvariant()}");

	private static int ParseInt(string name, string value) =>
		int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
			? result
			: throw new ArgumentException($"{name} needs a whole number, not {value}");
}
