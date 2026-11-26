using Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests;

public class InvalidScanOptionsTests
{
	[Fact]
	public void WhenNothingIsScanned_ThenThrowsArgumentExceptionNamingTheScanMethods()
	{
		var exception = Assert.Throws<ArgumentException>(() => new ServiceCollection().AddFluxor());

		Assert.Equal(
			"Must call either FluxorOptions.ScanAssemblies or FluxorOptions.ScanTypes",
			exception.Message);
	}

	[Fact]
	public void WhenScanTypesIsGivenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(
			"typeToScan",
			() => new ServiceCollection().AddFluxor(x => x.ScanTypes(null)));
	}

	[Fact]
	public void WhenScanAssembliesIsGivenNull_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(
			"assemblyToScan",
			() => new ServiceCollection().AddFluxor(x => x.ScanAssemblies((Assembly)null)));
	}

	[Fact]
	public void WhenScanTypesIsGivenOpenGenericTypes_ThenThrowsInvalidOperationExceptionListingThem()
	{
		var exception = Assert.Throws<InvalidOperationException>(() =>
			new ServiceCollection().AddFluxor(x => x.ScanTypes(
				typeof(GenericType<>),
				typeof(TestState),
				typeof(GenericType<,>))));

		Assert.Equal(
			"The following types cannot be generic: GenericType`1,GenericType`2",
			exception.Message);
	}
}
