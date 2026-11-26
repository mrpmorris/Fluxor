using Fluxor.Performance.Samples;

namespace Fluxor.PerformanceTests;

public class SampleWriterTests
{
	[Fact]
	public void WhenSampleWriterRunsTwice_ThenEverySampleHasTheSameText()
	{
		SampleOptions[] samples = SizedSample.All.Select(x => x.Options)
			.Prepend(SampleOptions.Default)
			.Concat(ScalingSample.All.Select(x => x.Options))
			.ToArray();
		foreach (SampleOptions options in samples)
		{
			// Each build writes its sample again; a sample that changed from one run to the next would rebuild every time
			IReadOnlyList<SampleFile> first = SampleWriter.Create(options);
			IReadOnlyList<SampleFile> second = SampleWriter.Create(options);
			Assert.Equal(first, second);
			Assert.NotEmpty(first);
		}
	}

	[Fact]
	public void WhenExistingFilesDifferOnlyInLineEndings_ThenSampleWriterLeavesThemAlone()
	{
		string tempDirectory = Path.Combine(Path.GetTempPath(), "fluxor-sample-" + Guid.NewGuid().ToString("N"));
		try
		{
			IReadOnlyList<SampleFile> files = SampleWriter.Write(SampleOptions.Default, tempDirectory);
			Assert.DoesNotContain(files, x => x.Content.Contains('\r'));

			// As a CRLF checkout leaves them: same text, other line endings, an older time stamp
			var stamp = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			SampleFile crlfFile = files[0];
			string crlfPath = Path.Combine(tempDirectory, crlfFile.FileName);
			string crlfText = crlfFile.Content.Replace("\n", "\r\n");
			File.WriteAllText(crlfPath, crlfText);
			File.SetLastWriteTimeUtc(crlfPath, stamp);
			// And one file whose text really changed
			SampleFile changedFile = files[1];
			string changedPath = Path.Combine(tempDirectory, changedFile.FileName);
			File.WriteAllText(changedPath, "// changed\n" + changedFile.Content);
			File.SetLastWriteTimeUtc(changedPath, stamp);

			SampleWriter.Write(SampleOptions.Default, tempDirectory);

			Assert.Equal(crlfText, File.ReadAllText(crlfPath));
			Assert.Equal(stamp, File.GetLastWriteTimeUtc(crlfPath));
			Assert.Equal(changedFile.Content, File.ReadAllText(changedPath));
			Assert.NotEqual(stamp, File.GetLastWriteTimeUtc(changedPath));
		}
		finally
		{
			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, recursive: true);
		}
	}

	[Fact]
	public void WhenProfileIsComplex_ThenSampleHasEveryForm()
	{
		IReadOnlyList<SampleFile> files = SampleWriter.Create(SizedSample.Get("complex").Options);
		string main = string.Concat(files.Where(x => !x.IsBase).Select(x => x.Content));
		string @base = string.Concat(files.Where(x => x.IsBase).Select(x => x.Content));
		string FileText(string name) => files.Single(x => x.FileName == name).Content;

		// States: private constructor, public and private factory method, required members, Feature<T> class, nested
		Assert.Contains("\tprivate State000() : this(0, \"\") { }", main);
		Assert.Contains("[FeatureState(CreateInitialStateMethodName = nameof(Create))]", main);
		Assert.Contains("[FeatureState(CreateInitialStateMethodName = \"Create\")]", main);
		Assert.Contains("\tprivate static State", main);
		Assert.Contains("\tpublic required int Count { get; init; }", main);
		Assert.Contains(" : Feature<State", main);
		Assert.Contains(" : Feature<Base.State", main);
		Assert.Contains("\t[FeatureState]\n\tpublic record State", main);
		// Reducers and effects: both attribute forms, static and instance, classes
		Assert.Contains("[ReducerMethod(typeof(", main);
		Assert.Contains("[EffectMethod(typeof(", main);
		Assert.Contains("\tpublic static State", main);
		Assert.Contains(" : IReducer<State", main);
		Assert.Contains(" : IEffect", main);
		Assert.Contains(" : Effect<State", main);
		// Inheritance: three levels, the first in the separate assembly, with override and new
		Assert.Contains("public abstract class State001Level1", @base);
		Assert.Contains("[ReducerMethod]", @base);
		Assert.Contains("[EffectMethod]", @base);
		Assert.Contains("public class State001Level2 : Base.State001Level1", main);
		Assert.Contains("public class State001Level3 : State001Level2", main);
		Assert.Contains("\tpublic override ", main);
		Assert.Contains("\tpublic new ", main);
		// A partial host across two files
		Assert.Contains("public partial class State006Host", FileText("PartialHostsA.cs"));
		Assert.Contains("public partial class State006Host", FileText("PartialHostsB.cs"));
		// Effects on a base-class action and an interface action; several reducers for one action
		Assert.Contains("(State007BaseAction action, IDispatcher dispatcher)", main);
		Assert.Contains("(IState007Marker action, IDispatcher dispatcher)", main);
		Assert.Contains("ReduceState008Action0Second", main);
		// A middleware namespace, whose Fluxor code the scan leaves out
		Assert.Contains("namespace Fluxor.Performance.Sample.WithMiddleware;", FileText("Middleware.cs"));
		Assert.Contains(" : Middleware", FileText("Middleware.cs"));
	}

	[Fact]
	public void WhenFluxorMembersDouble_ThenEveryScalingSampleIsTheOneXSampleTimesItsMultiple()
	{
		ScalingSample one = ScalingSample.FluxorMemberSeries[0];
		Assert.Equal(["K1", "K2", "K4", "K8"], ScalingSample.FluxorMemberSeries.Select(x => x.Name));
		foreach (ScalingSample sample in ScalingSample.FluxorMemberSeries)
		{
			int multiple = sample.Features / one.Features;
			Assert.Equal(int.Parse(sample.Name[1..], System.Globalization.CultureInfo.InvariantCulture), multiple);
			Assert.Equal(
				(one.Features * multiple, one.Reducers * multiple, one.Effects * multiple, one.Members * multiple),
				(sample.Features, sample.Reducers, sample.Effects, sample.Members));
			Assert.Equal(sample.Features + sample.Reducers + sample.Effects, sample.Members);
			// Every kind of Fluxor type grows with K too, so the described types (n) are proportional to K
			Assert.Equal(
				(one.Options.ReducerHostClasses * multiple, one.Options.EffectHostClasses * multiple, one.Options.ReducerClasses * multiple,
					one.Options.EffectClasses * multiple, one.Options.PrivateConstructorStates * multiple),
				(sample.Options.ReducerHostClasses, sample.Options.EffectHostClasses, sample.Options.ReducerClasses,
					sample.Options.EffectClasses, sample.Options.PrivateConstructorStates));
			Assert.Equal(0, sample.Options.PlainClasses);
		}
		// The 1x sample is the standard sample
		Assert.Equal(SampleOptions.Default, one.Options);
	}

	[Fact]
	public void WhenNonFluxorCodeGrows_ThenScalingSamplesAddOnlyPlainClassesToTheOneXSample()
	{
		ScalingSample one = ScalingSample.FluxorMemberSeries[0];
		Assert.Same(one, ScalingSample.NonFluxorCodeSeries[0]);
		Assert.Equal([0, 2_000, 4_000, 8_000], ScalingSample.NonFluxorCodeSeries.Select(x => x.Options.PlainClasses));
		foreach (ScalingSample sample in ScalingSample.NonFluxorCodeSeries.Skip(1))
		{
			Assert.Equal(one.Options with { PlainClasses = sample.Options.PlainClasses }, sample.Options);
			Assert.Equal(sample.Options.PlainClasses * 10, sample.AddedMethods);
			AssertOnlyPlainClassesAdded(SampleWriter.Create(one.Options), SampleWriter.Create(sample.Options), sample.Options.PlainClasses);
		}
	}

	[Fact]
	public void WhenComplexSampleGetsPlainClasses_ThenOnlyCodeWithoutFluxorIsAdded()
	{
		ScalingSample sample = ScalingSample.Get("ComplexPlain4000");
		Assert.Equal(SizedSample.Complex.Options with { PlainClasses = 4_000 }, sample.Options);
		AssertOnlyPlainClassesAdded(SampleWriter.Create(SizedSample.Complex.Options), SampleWriter.Create(sample.Options), 4_000);
	}

	[Fact]
	public void WhenScalingSampleIsGiven_ThenCommandLineWritesItsOptionsToTheGivenFolder()
	{
		(SampleOptions options, string? output, _, SizedSample? size, ScalingSample? scaling) =
			CommandLine.Parse(["--scaling", "k4", "--out", "obj/Sample"]);
		Assert.Same(ScalingSample.Get("K4"), scaling);
		Assert.Equal(ScalingSample.Get("K4").Options, options);
		Assert.Equal("obj/Sample", output);
		Assert.Null(size);

		Assert.Throws<ArgumentException>(() => CommandLine.Parse(["--scaling", "K3", "--out", "x"]));
	}

	[Theory]
	[InlineData]
	[InlineData("--size", "small")]
	[InlineData("--size", "complex", "--base-out", "obj/Sample/Base")]
	[InlineData("--scaling", "K4")]
	public void WhenNoOutputFolderIsGiven_ThenCommandLineThrows(params string[] args)
	{
		// Samples are written at build time only: there is no checked-in folder to default to
		ArgumentException exception = Assert.Throws<ArgumentException>(() => CommandLine.Parse(args));
		Assert.Contains("--out", exception.Message);
	}

	[Fact]
	public void WhenSizeIsGiven_ThenCommandLineWritesItsOptionsToTheGivenFolders()
	{
		(SampleOptions options, string output, string? baseOutput, SizedSample? size, ScalingSample? scaling) =
			CommandLine.Parse(["--size", "Complex", "--out", "obj/Sample", "--base-out", "obj/Sample/Base"]);
		Assert.Same(SizedSample.Complex, size);
		Assert.Equal(SizedSample.Complex.Options, options);
		Assert.Equal(("obj/Sample", "obj/Sample/Base"), (output, baseOutput));
		Assert.Null(scaling);

		(options, _, baseOutput, size, scaling) = CommandLine.Parse(["--out", "obj/Sample"]);
		Assert.Equal(SampleOptions.Default, options);
		Assert.Null(baseOutput);
		Assert.Null(size);
		Assert.Null(scaling);
	}

	/// <summary>
	/// The files are the same but for the header comment that describes the options, plus PlainClasses.cs, which has
	/// the plain classes and nothing of Fluxor.
	/// </summary>
	private static void AssertOnlyPlainClassesAdded(IReadOnlyList<SampleFile> without, IReadOnlyList<SampleFile> with, int plainClasses)
	{
		Assert.Equal(without.Select(x => x.FileName).Append("PlainClasses.cs"), with.Select(x => x.FileName));
		foreach (SampleFile file in without)
		{
			SampleFile other = with.Single(x => x.FileName == file.FileName && x.IsBase == file.IsBase);
			Assert.Equal(WithoutHeader(file.Content), WithoutHeader(other.Content));
		}

		string plain = with.Single(x => x.FileName == "PlainClasses.cs").Content;
		Assert.Equal(plainClasses, Count(plain, "\npublic class Plain"));
		Assert.Equal(plainClasses * 10, Count(plain, "\tpublic int Method"));
		Assert.DoesNotContain("[", plain);
		Assert.DoesNotContain(" : ", plain);

		static string WithoutHeader(string content) => string.Join('\n', content.Split('\n').Where((_, index) => index != 1));
		static int Count(string text, string value) => (text.Length - text.Replace(value, "").Length) / value.Length;
	}
}
