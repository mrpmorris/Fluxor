using Xunit;

namespace Fluxor.Blazor.Web.ReduxDevTools.UnitTests.ActionInfoTests;

public record SimpleAction();

public static class ParentActions
{
	public record NestedAction();
}

public static class OuterActions
{
	public static class InnerActions
	{
		public record DeeplyNestedAction();
	}
}

public class GetTypeDisplayNameTests
{
	[Fact]
	public void WhenTypeIsSimpleNonNestedClass_ThenReturnsTypeName()
	{
		string result = ActionInfo.GetTypeDisplayName(typeof(SimpleAction));
		Assert.Equal("SimpleAction", result);
	}

	[Fact]
	public void WhenTypeIsNestedInsideStaticClass_ThenIncludesParentClassName()
	{
		string result = ActionInfo.GetTypeDisplayName(typeof(ParentActions.NestedAction));
		Assert.Equal("ParentActions+NestedAction", result);
	}

	[Fact]
	public void WhenTypeIsDeeplyNested_ThenIncludesFullNestedPath()
	{
		string result = ActionInfo.GetTypeDisplayName(typeof(OuterActions.InnerActions.DeeplyNestedAction));
		Assert.Equal("OuterActions+InnerActions+DeeplyNestedAction", result);
	}

	[Fact]
	public void WhenTypeIsGenericNonNested_ThenReturnsGenericTypeName()
	{
		string result = ActionInfo.GetTypeDisplayName(typeof(List<int>));
		Assert.Equal("List<Int32>", result);
	}

	[Fact]
	public void WhenTypeIsGenericWithNestedArgument_ThenIncludesNestedParentInArgument()
	{
		string result = ActionInfo.GetTypeDisplayName(typeof(List<ParentActions.NestedAction>));
		Assert.Equal("List<ParentActions+NestedAction>", result);
	}
}
