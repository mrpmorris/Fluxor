using Fluxor.Reactor.Maui.UnitTests.SupportFiles;
using MauiReactor.Internals;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Reactor.Maui.UnitTests.Components.FluxorComponentTests;

public class DisposeTests : IDisposable
{
	private readonly ServiceContext ServiceContext;
	private readonly FluxorComponentWithStateProperties StateSubject;
	private readonly MockState<int> MockState1;
	private readonly MockState<int> MockState2;
	private bool disposedValue;

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
		using var serviceContext = new ServiceContext(services =>
			services.AddSingleton<IActionSubscriber, MockActionSubscriber>());
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
		using var serviceContext = new ServiceContext(services =>
			services.AddSingleton<IActionSubscriber, MockActionSubscriber>());
		var component = new FluxorComponentThatOptionallyCallsBaseOnMounted();
		component.Test_OnMounted(callBase: true);
		await component.DisposeAsync();
	}

	public DisposeTests()
	{
		ServiceContext = new ServiceContext(services =>
			services.AddSingleton<IActionSubscriber, MockActionSubscriber>());
		MockState1 = new MockState<int>();
		MockState2 = new MockState<int>();
		StateSubject = new FluxorComponentWithStateProperties
		{
			State1 = MockState1,
			State2 = MockState2
		};
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				ServiceContext.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
