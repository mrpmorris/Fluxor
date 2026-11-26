using Fluxor.Performance.Samples;

// dotnet run --project Source/Performance/Fluxor.Performance.SampleWriter -- [options] --out DIR
//   --size small|medium|complex   (the browser benchmark's samples; later options change the size's options)
//   --scaling NAME   (a sample of the scaling proofs, see ScalingSample)
//   --profile simple|standard|simplewithclasses|complex   --states N   --reducers-per-state R   --effects-per-state E
//   --total-reducers R   --total-effects E   --private-constructor-states N   --feature-class-states N
//   --reducer-classes N   --effect-classes N   --reducer-hosts N   --effect-hosts N
//   --plain-classes P   --methods-per-plain-class M   --namespace NS   --out DIR   --base-out DIR
// With neither --size nor --scaling it writes the standard sample (SampleOptions.Default). No sample is checked in:
// the sample projects (Samples/WrittenSample.targets) run the writer at build time and compile what it writes into
// their obj folder. The Complex profile also writes the code of its separate base assembly, into --base-out.
try
{
	(SampleOptions options, string outputDirectory, string? baseOutputDirectory, _, _) = CommandLine.Parse(args);

	IReadOnlyList<SampleFile> files = SampleWriter.Write(options, outputDirectory, baseOutputDirectory);
	Console.WriteLine($"Wrote {files.Count(x => !x.IsBase)} files to {Path.GetFullPath(outputDirectory)}");
	if (files.Any(x => x.IsBase))
		Console.WriteLine($"Wrote {files.Count(x => x.IsBase)} files to {Path.GetFullPath(baseOutputDirectory!)}");
	return 0;
}
catch (ArgumentException exception)
{
	Console.Error.WriteLine(exception.Message);
	return 1;
}
