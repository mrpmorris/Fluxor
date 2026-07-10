using Fluxor.Reactor.Maui.UnitTests.SupportFiles;
using MauiReactor.Internals;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Reactor.Maui.UnitTests.Components.FluxorComponentTests;

public class OnMountedTests : IDisposable
{
	private readonly ServiceContext ServiceContext;
	private readonly FluxorComponentWithStateProperties Subject;
	private readonly MockState<int> MockState1;
	private readonly MockState<int> MockState2;
	private bool disposedValue;

	[Fact]
	public void SubscribesToStateProperties()
	{
		Subject.Test_OnMounted();

		Assert.Equal(1, MockState1.SubscribeCount);
		Assert.Equal(1, MockState2.SubscribeCount);
	}

	public OnMountedTests()
	{
		ServiceContext = new ServiceContext(services =>
			services.AddSingleton<IActionSubscriber, MockActionSubscriber>());
		MockState1 = new MockState<int>();
		MockState2 = new MockState<int>();
		Subject = new FluxorComponentWithStateProperties
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
				Subject.Test_OnWillUnmount();
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
