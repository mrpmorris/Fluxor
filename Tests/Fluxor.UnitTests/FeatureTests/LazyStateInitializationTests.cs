using System;
using Fluxor.UnitTests.FeatureTests.SupportFiles;
using Xunit;

namespace Fluxor.UnitTests.FeatureTests
{
    public class LazyStateInitializationTests
    {
        [Fact]
        public void WhenAFeatureIsConstructed_ThenStateTheInitialStateIsNotSet()
        {
            var count = 0;
            var feature = new FeatureWithDependency(() => DateTime.MaxValue.Subtract(TimeSpan.FromDays(count++)));

            Assert.NotNull(feature);
            Assert.Equal(0, count);
        }

        [Fact]
        public void WhenStateIsAccessedMultipleTimes_ThenTheStateIsOnlyInitialisedOnce()
        {
            var count = 0;
            var feature = new FeatureWithDependency(() => DateTime.MaxValue.Subtract(TimeSpan.FromDays(count++)));

            for (var i = 0; i < 10; i++)
                Assert.Equal(DateTime.MaxValue, feature.State.Now);

            Assert.Equal(1, count);
        }
    }
}
