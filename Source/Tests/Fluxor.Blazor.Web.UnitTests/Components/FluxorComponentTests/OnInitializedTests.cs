using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorComponentTests;

public class OnInitializedTests
{
	private readonly FluxorComponentWithStateProperties Subject;
	private readonly MockState<int> MockState1;
	private readonly MockState<int> MockState2;

	[Fact]
	public void SubscribesToStateProperties()
	{
		Subject.ExecuteOnInitialized();

		Assert.Equal(1, MockState1.SubscribeCount);
		Assert.Equal(1, MockState2.SubscribeCount);
	}

	public OnInitializedTests()
	{
		MockState1 = new MockState<int>();
		MockState2 = new MockState<int>();
		Subject = new FluxorComponentWithStateProperties
		{
			State1 = MockState1,
			State2 = MockState2
		};
	}
}
