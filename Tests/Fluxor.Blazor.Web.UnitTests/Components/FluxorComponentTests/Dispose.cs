using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using Moq;
using System;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorComponentTests
{
	public partial class FluxorComponentTests
	{
		public class Dispose
		{
			private readonly FluxorComponentWithStateProperties Subject;
			private readonly Mock<IState> MockState1;
			private readonly Mock<IState> MockState2;

			[Fact]
			public void UnsubscribesFromStateProperties()
			{
				Subject.ExecuteOnInitialized();

				MockState1.SetupRemove(x => x.StateChanged -= It.IsAny<EventHandler>()).Verifiable();
				MockState2.SetupRemove(x => x.StateChanged -= It.IsAny<EventHandler>()).Verifiable();

				Subject.Dispose();
				MockState1.VerifyAll();
				MockState2.VerifyAll();
			}

			public Dispose()
			{
				MockState1 = new Mock<IState>();
				MockState2 = new Mock<IState>();
				Subject = new FluxorComponentWithStateProperties {
					State1 = MockState1.Object,
					State2 = MockState2.Object
				};
			}
		}
	}
}
