using System.Globalization;

namespace Fluxor.Performance.BlazorWasm.Benchmark;

/// <summary>
/// A sample size, the store every build of it must create, and the speed bar for the generated registration
/// (Compact shape): its median AddFluxor + store initialized time divided by the scanned one.
/// </summary>
internal sealed record SampleSize(string Name, int Features, int Reducers, int Effects, double Bar, bool StrictlyBelow)
{
	public static readonly SampleSize Small = new("Small", Features: 20, Reducers: 100, Effects: 100, Bar: 1.0, StrictlyBelow: true);
	public static readonly SampleSize Medium = new("Medium", Features: 80, Reducers: 600, Effects: 600, Bar: 0.75, StrictlyBelow: false);
	public static readonly SampleSize Complex = new("Complex", Features: 200, Reducers: 2_000, Effects: 2_000, Bar: 0.5, StrictlyBelow: false);

	public static SampleSize Parse(string name) =>
		new[] { Small, Medium, Complex }.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))
		?? throw new ArgumentException($"Unknown sample size {name}", nameof(name));

	public string Key => Name.ToLowerInvariant();

	public bool Passes(double ratio) => StrictlyBelow ? ratio < Bar : ratio <= Bar;

	public string DescribeBar() =>
		(StrictlyBelow ? "< " : "<= ") + Bar.ToString("0.00", CultureInfo.InvariantCulture);
}
