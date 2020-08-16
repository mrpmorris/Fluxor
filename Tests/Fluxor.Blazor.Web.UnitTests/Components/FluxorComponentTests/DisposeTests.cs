using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorComponentTests
{
	public class DisposeTests
	{
		private readonly FluxorComponentWithStateProperties Subject;
		private readonly MockState<int> MockState1;
		private readonly MockState<int> MockState2;

		[Fact]
		public void UnsubscribesFromStateProperties()
		{
			Subject.ExecuteOnInitialized();
			Subject.Dispose();

			Assert.Equal(1, MockState1.UnsubscribeCount);
			Assert.Equal(1, MockState2.UnsubscribeCount);
		}

		public DisposeTests()
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
}
