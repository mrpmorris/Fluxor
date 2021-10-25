using System;

namespace Fluxor.DependencyInjection.Wrappers
{
	internal class FeatureStateWrapper<TState> : Feature<TState>
	{
		private readonly string Name;
		private readonly Func<object> CreateInitialStateFunc;

		public FeatureStateWrapper(
			FeatureStateInfo info)
		{
			Name = info.FeatureStateAttribute.Name ?? typeof(TState).FullName;
			MaximumStateChangedNotificationsPerSecond = info.FeatureStateAttribute.MaximumStateChangedNotificationsPerSecond;
			CreateInitialStateFunc = info.CreateInitialStateFunc;
		}

		public override string GetName() => Name;

		protected override TState GetInitialState() => (TState)CreateInitialStateFunc();
	}
}
