using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorLayoutTests;

public class DisposeTests
{
	private readonly FluxorLayoutWithStateProperties StateSubject;
	private readonly MockState<int> MockState1;
	private readonly MockState<int> MockState2;

	[Fact]
	public async Task UnsubscribesFromStateProperties()
	{
		StateSubject.Test_OnInitialized();
		await StateSubject.DisposeAsync();

		Assert.Equal(1, MockState1.UnsubscribeCount);
		Assert.Equal(1, MockState2.UnsubscribeCount);
	}

	[Fact]
	public async Task WhenBaseOnInitializedWasNotCalled_ThenThrowsNullReferenceException()
	{
		var layout = new FluxorLayoutThatOptionallyCallsBaseOnInitialized();
		var exception = await Assert.ThrowsAsync<NullReferenceException>(
			async () =>
			{
				layout.Test_OnInitialized(callBase: false);
				await layout.DisposeAsync();
			}
		);

		Assert.Equal("Have you forgotten to call base.OnInitialized() in your component?", exception.Message);
	}

	[Fact]
	public async Task WhenBaseOnInitializedWasCalled_ThenDoesNotThrowAnException()
	{
		var layout = new FluxorLayoutThatOptionallyCallsBaseOnInitialized();
		layout.Test_OnInitialized(callBase: true);
		await layout.DisposeAsync();
	}

	public DisposeTests()
	{
		MockState1 = new MockState<int>();
		MockState2 = new MockState<int>();
		StateSubject = new FluxorLayoutWithStateProperties
		{
			State1 = MockState1,
			State2 = MockState2
		};
	}
}
