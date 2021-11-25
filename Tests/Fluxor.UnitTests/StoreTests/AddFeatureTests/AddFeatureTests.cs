using Moq;
using System;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.AddFeatureTests
{
	public class AddFeatureTests
	{
		private readonly Dispatcher Dispatcher;
		private readonly Store Subject;

		[Fact]
		public void WhenFeatureNameIsUnique_ThenAddsFeatureToFeaturesDictionary()
		{
			const string featureName = "123";
			var mockFeature = new Mock<IFeature>();
			mockFeature
				.Setup(x => x.GetName())
				.Returns(featureName);

			Subject.AddFeature(mockFeature.Object);

			Assert.Same(mockFeature.Object, Subject.Features[featureName]);
		}

		[Fact]
		public void WhenFeatureWithSameNameAlreadyExists_ThenThrowsArgumentException()
		{
			const string featureName = "1234";
			var mockFeature = new Mock<IFeature>();
			mockFeature
				.Setup(x => x.GetName())
				.Returns(featureName);

			Subject.AddFeature(mockFeature.Object);

			Assert.Throws<ArgumentException>(() =>
			{
				Subject.AddFeature(mockFeature.Object);
			});
		}

		public AddFeatureTests()
		{
			Dispatcher = new Dispatcher();
			Subject = new Store(Dispatcher);
		}
	}
}
