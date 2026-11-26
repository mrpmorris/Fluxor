using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.InvalidSetupTests;

public abstract class InvalidSetupTestsBase
{
	/// <summary>AddFluxor runs the reflection scan, so an invalid type throws from here.</summary>
	protected static TException ScanTypeAndExpect<TException>(Type typeToScan)
		where TException : Exception =>
		Assert.Throws<TException>(() =>
			new ServiceCollection().AddFluxor(x => x.ScanTypes(typeToScan)));

	protected static string MethodDescription(Type hostClass, string methodName) =>
		$"Method \"{methodName}\" on class \"{hostClass.FullName}\"";
}
