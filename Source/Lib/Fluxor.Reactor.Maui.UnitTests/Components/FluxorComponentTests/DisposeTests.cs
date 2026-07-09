using Fluxor.Reactor.Maui.UnitTests.SupportFiles;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Reactor.Maui.UnitTests.Components.FluxorComponentTests;

public class DisposeTests
{
	private readonly FluxorComponentWithStateProperties StateSubject;
	private readonly MockState<int> MockState1;
	private readonly MockState<int> MockState2;

	[Fact]
	public async Task UnsubscribesFromStateProperties()
	{
		StateSubject.Test_OnMounted();
		await StateSubject.DisposeAsync();

		Assert.Equal(1, MockState1.UnsubscribeCount);
		Assert.Equal(1, MockState2.UnsubscribeCount);
	}

	[Fact]
	public async Task WhenBaseOnMountedWasNotCalled_ThenThrowsNullReferenceException()
	{
		var component = new FluxorComponentThatOptionallyCallsBaseOnMounted();
		component.Test_OnMounted(callBase: false);

		var exception = await Assert.ThrowsAsync<NullReferenceException>(
			async () =>
			{
				await component.DisposeAsync();
			}
		);
		
		Assert.Equal("Have you forgotten to call base.OnMounted() in your component?", exception.Message);
	}

	[Fact]
	public async Task WhenBaseOnMountedWasCalled_ThenDoesNotThrowAnException()
	{
		var component = new FluxorComponentThatOptionallyCallsBaseOnMounted();
		component.Test_OnMounted(callBase: true);
		await component.DisposeAsync();
	}

	public DisposeTests()
	{
		MockState1 = new MockState<int>();
		MockState2 = new MockState<int>();
		StateSubject = new FluxorComponentWithStateProperties
		{
			State1 = MockState1,
			State2 = MockState2
		};
	}
}
