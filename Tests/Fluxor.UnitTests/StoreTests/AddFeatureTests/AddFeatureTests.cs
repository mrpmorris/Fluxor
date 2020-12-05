using Moq;
using System;
using Xunit;

namespace Fluxor.UnitTests.StoreTests.AddFeatureTests
{
	public class AddFeatureTests
	{
		[Fact]
		public void WhenFeatureNameIsUnique_ThenAddsFeatureToFeaturesDictionary()
		{
			const string featureName = "123";
			var mockFeature = new Mock<IFeature>();
			mockFeature
				.Setup(x => x.GetName())
				.Returns(featureName);

			var subject = new Store(new Dispatcher());
			subject.AddFeature(mockFeature.Object);

			Assert.Same(mockFeature.Object, subject.Features[featureName]);
		}

		[Fact]
		public void WhenFeatureWithSameNameAlreadyExists_ThenThrowsArgumentException()
		{
			const string featureName = "1234";
			var mockFeature = new Mock<IFeature>();
			mockFeature
				.Setup(x => x.GetName())
				.Returns(featureName);

			var subject = new Store(new Dispatcher());
			subject.AddFeature(mockFeature.Object);

			Assert.Throws<ArgumentException>(() =>
			{
				subject.AddFeature(mockFeature.Object);
			});
		}
	}
}
