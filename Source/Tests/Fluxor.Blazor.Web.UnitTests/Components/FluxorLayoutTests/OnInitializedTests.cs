using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorLayoutTests;

public class OnInitializedTests: IAsyncLifetime
{
	private readonly FluxorLayoutWithStateProperties Subject;
	private readonly MockState<int> MockState1;
	private readonly MockState<int> MockState2;

	[Fact]
	public void SubscribesToStateProperties()
	{
		Subject.Test_OnInitialized();

		Assert.Equal(1, MockState1.SubscribeCount);
		Assert.Equal(1, MockState2.SubscribeCount);
	}

	Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;
	async Task IAsyncLifetime.DisposeAsync() => await Subject.DisposeAsync();

	public OnInitializedTests()
	{
		MockState1 = new MockState<int>();
		MockState2 = new MockState<int>();
		Subject = new FluxorLayoutWithStateProperties
		{
			State1 = MockState1,
			State2 = MockState2
		};
	}
}
