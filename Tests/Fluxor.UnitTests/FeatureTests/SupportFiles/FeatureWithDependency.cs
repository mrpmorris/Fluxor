using System;

namespace Fluxor.UnitTests.FeatureTests.SupportFiles
{
    public class State
    {
        public DateTime Now { get; }

        public State(DateTime now)
        {
            Now = now;
        }

    }

    public class FeatureWithDependency : Feature<State>
    {
        readonly Func<DateTime> _NowGetter;

        public override string GetName() => nameof(FeatureWithDependency);

        public FeatureWithDependency(Func<DateTime> nowGetter)
        {
            _NowGetter = nowGetter;
        }

        protected override State GetInitialState()
        {
            return new State(_NowGetter());
        }
    }
}
