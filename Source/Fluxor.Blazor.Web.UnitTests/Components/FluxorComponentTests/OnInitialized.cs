using Fluxor.Blazor.Web.UnitTests.SupportFiles;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fluxor.Blazor.Web.UnitTests.Components.FluxorComponentTests
{
	public partial class FluxorComponentTests
	{
		public class OnInitialized
		{
			private readonly FluxorComponentWithStateProperties Subject;
			private readonly Mock<IState> MockState1;
			private readonly Mock<IState> MockState2;

			[Fact]
			public void SubscribesToStateProperties()
			{
				MockState1.SetupAdd(x => x.StateChanged += It.IsAny<EventHandler>()).Verifiable();
				MockState2.SetupAdd(x => x.StateChanged += It.IsAny<EventHandler>()).Verifiable();

				Subject.ExecuteOnInitialized();

				MockState1.VerifyAll();
				MockState2.VerifyAll();
			}

			public OnInitialized()
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
