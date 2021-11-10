using Moq;

namespace Fluxor.UnitTests.StateSelectionTests
{
	public class TestsBase
	{
		protected readonly IStateSelection<string, char> Subject;
		protected readonly Mock<IFeature<string>> MockFeature = new();
		protected string FeatureState = "ABC";

		public TestsBase()
		{
			MockFeature.SetupGet(x => x.State).Returns(() => FeatureState);
			Subject = new StateSelection<string, char>(MockFeature.Object);
		}
	}
}
