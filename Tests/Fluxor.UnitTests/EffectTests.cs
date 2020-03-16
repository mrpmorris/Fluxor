using Fluxor.UnitTests.SupportFiles;
using Xunit;

namespace Fluxor.UnitTests
{
	public class EffectTests
	{
		public class ShouldReactToAction
		{
			[Fact]
			public void ReturnsTrue_WhenTTriggerActionIsSameAsActionType()
			{
				var action = new ActionForEffect();
				var subject = new GenericEffectThatDoesNothing<ActionForEffect>();
				Assert.True(subject.ShouldReactToAction(action));
			}

			[Fact]
			public void ReturnsTrue_WhenActionTypeIsDescendedFromTTriggerAction()
			{
				var action = new ActionThatQualifiesViaInheritance();
				var subject = new GenericEffectThatDoesNothing<ActionForEffect>();
				Assert.True(subject.ShouldReactToAction(action));
			}

			[Fact]
			public void ReturnsTrue_WhenActionTypeImplementsTTriggerActionInterface()
			{
				var action = new ActionThatQualifiesViaInterface();
				var subject = new GenericEffectThatDoesNothing<IInterfaceForEffect>();
				Assert.True(subject.ShouldReactToAction(action));
			}

			[Fact]
			public void ReturnsFalse_WhenTTriggerActionIsNotAssignableFromActionType()
			{
				var action = new ActionNotForThisEffect();
				var subject = new GenericEffectThatDoesNothing<ActionForEffect>();
				Assert.False(subject.ShouldReactToAction(action));
			}
		}

		public class ActionForEffect  { }
		public class ActionThatQualifiesViaInheritance : ActionForEffect { }
		public class ActionNotForThisEffect  { }
		public interface IInterfaceForEffect  { }
		public class ActionThatQualifiesViaInterface : IInterfaceForEffect { }

	}
}
