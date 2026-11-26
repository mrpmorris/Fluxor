using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace Fluxor.UnitTests;

public class PackagingTests
{
	[ReleasePackageFact]
	public void WhenPacked_ThenFluxorPackageContainsGeneratorAndProps()
	{
		string package = ReleasePackageFactAttribute.FindReleasePackage();
		Assert.NotNull(package);

		using ZipArchive zip = ZipFile.OpenRead(package);
		string[] entries = zip.Entries.Select(x => x.FullName).ToArray();

		Assert.Contains("analyzers/dotnet/cs/Fluxor.SourceGenerator.dll", entries);
		Assert.Contains("build/Fluxor.props", entries);
		Assert.Contains("buildTransitive/Fluxor.props", entries);
		foreach (string props in new[] { "build/Fluxor.props", "buildTransitive/Fluxor.props" })
		{
			using var reader = new StreamReader(zip.GetEntry(props)!.Open());
			Assert.Contains("<CompilerVisibleProperty Include=\"FluxorGenerateRegistration\" />", reader.ReadToEnd());
		}
	}
}

/// <summary>A fact that is skipped, with a reason, when the Release package has not been built.</summary>
public sealed class ReleasePackageFactAttribute : FactAttribute
{
	public ReleasePackageFactAttribute()
	{
		if (FindReleasePackage() is null)
			Skip = "The Release Fluxor .nupkg was not built (Release builds generate it). Run: dotnet build Source -c Release";
	}

	public static string FindReleasePackage()
	{
		string directory = Path.GetDirectoryName(typeof(PackagingTests).Assembly.Location);
		while (directory is not null && !File.Exists(Path.Combine(directory, "Fluxor", "Fluxor.csproj")))
			directory = Path.GetDirectoryName(directory);
		if (directory is null)
			return null;

		string releaseFolder = Path.Combine(directory, "Fluxor", "bin", "Release");
		if (!Directory.Exists(releaseFolder))
			return null;

		return Directory.GetFiles(releaseFolder, "Fluxor.*.nupkg")
			.Where(x => !x.EndsWith(".symbols.nupkg") && !x.Contains("Blazor"))
			.OrderByDescending(File.GetLastWriteTimeUtc)
			.FirstOrDefault();
	}
}
