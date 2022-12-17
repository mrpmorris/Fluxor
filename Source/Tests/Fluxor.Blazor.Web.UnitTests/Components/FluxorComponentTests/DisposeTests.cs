using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorComponentTests
{
	public class DisposeTests
	{
		private readonly FluxorComponentWithStateProperties StateSubject;
		private readonly MockState<int> MockState1;
		private readonly MockState<int> MockState2;

		[Fact]
		public async ValueTask UnsubscribesFromStateProperties()
		{
			StateSubject.ExecuteOnInitialized();
			await StateSubject.DisposeAsync();

			Assert.Equal(1, MockState1.UnsubscribeCount);
			Assert.Equal(1, MockState2.UnsubscribeCount);
		}

		[Fact]
		public async ValueTask WhenBaseOnInitializedWasNotCalled_ThenThrowsNullReferenceException()
		{
			string errorMessage = null;
			var component = new FluxorComponentThatOptionallyCallsBaseOnInitialized();
			try
			{
				component.Test_OnInitialized();
				await component.DisposeAsync();
			}
			catch (NullReferenceException e)
			{
				errorMessage = e.Message;
			}
			Assert.Equal("Have you forgotten to call base.OnInitialized() in your component?", errorMessage);
		}

		[Fact]
		public async ValueTask WhenBaseOnInitializedWasCalled_ThenDoesNotThrowAnException()
		{
			var component = new FluxorComponentThatOptionallyCallsBaseOnInitialized
			{
				CallBaseOnInitialized = true
			};
			component.Test_OnInitialized();
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
}
