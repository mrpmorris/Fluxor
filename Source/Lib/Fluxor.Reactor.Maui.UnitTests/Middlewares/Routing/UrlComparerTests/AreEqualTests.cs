using Fluxor.Blazor.Web.Middlewares.Routing;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Middlewares.Routing.UrlComparerTests;

public class AreEqualTests
{
	[Fact]
	public void WhenUrlsAreIdentical_ThenReturnsTrue()
	{
		bool areEqual = UrlComparer.AreEqual("https://ibm.com", "https://ibm.com");
		Assert.True(areEqual);
	}

	[Fact]
	public void WhenProtocolChanges_ThenReturnsFalse()
	{
		bool areEqual = UrlComparer.AreEqual("https://ibm.com", "http://ibm.com");
		Assert.False(areEqual);
	}

	[Fact]
	public void WhenPortChanges_ThenReturnsFalse()
	{
		bool areEqual = UrlComparer.AreEqual("https://ibm.com:1234", "https://ibm.com:12345");
		Assert.False(areEqual);
	}

	[Fact]
	public void WhenDomainChanges_ThenReturnsFalse()
	{
		bool areEqual = UrlComparer.AreEqual("https://ibm.com:1234", "https://microsoft.com:1234");
		Assert.False(areEqual);
	}

	[Fact]
	public void WhenQueryStringChanges_ThenReturnsFalse()
	{
		bool areEqual = UrlComparer.AreEqual("https://ibm.com?id=31", "https://ibm.com?id=32");
		Assert.False(areEqual);
	}

	[Fact]
	public void WhenOnlyTheAnchorChanges_ThenReturnsTrue()
	{
		bool areEqual = UrlComparer.AreEqual("https://ibm.com#old-anchor", "https://ibm.com#new-anchor");
		Assert.True(areEqual);
	}

}
