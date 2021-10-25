using System;
using System.Collections.Generic;
using System.Text;

namespace Fluxor.DependencyInjection.Wrappers
{
	internal class FeatureAttributeStateWrapper<TState> : Feature<TState>
	{
		private readonly string Name;
		private readonly Func<object> CreateInitialStateFunc;

		public FeatureAttributeStateWrapper(
			FeatureAttributeClassInfo info)
		{
			Name = info.FeatureAttribute.Name ?? typeof(TState).FullName;
			MaximumStateChangedNotificationsPerSecond = info.FeatureAttribute.MaximumStateChangedNotificationsPerSecond;
			CreateInitialStateFunc = info.CreateInitialStateFunc;
		}

		public override string GetName() => Name;

		protected override TState GetInitialState() => (TState)CreateInitialStateFunc();
	}
}
