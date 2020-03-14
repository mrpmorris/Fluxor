using Moq;

namespace Fluxor.UnitTests.MockFactories
{
	public static class MockFeatureFactory
	{
		public static Mock<IFeature> Create(string name = "X")
		{
			var mock = new Mock<IFeature>();
			mock
				.Setup(x => x.GetName())
				.Returns(name);
			return mock;
		}
	}
}
