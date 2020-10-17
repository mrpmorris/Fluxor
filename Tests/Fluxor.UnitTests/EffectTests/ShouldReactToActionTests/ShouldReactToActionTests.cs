using Fluxor.UnitTests.EffectTests.ShouldReactToActionTests.SupportFiles;
using Xunit;

namespace Fluxor.UnitTests.EffectTests.ShouldReactToActionTests
{
	public class ShouldReactToActionTests
	{
		[Fact]
		public void WhenTriggerActionIsSameAsActionType_ThenReturnsTrue()
		{
			var action = new ActionForEffect();
			var subject = new GenericEffectThatDoesNothing<ActionForEffect>();
			Assert.True(subject.ShouldReactToAction(action));
		}

		[Fact]
		public void WhenActionTypeIsDescendedFromTriggerAction_ThenReturnsTrue()
		{
			var action = new ActionThatQualifiesViaInheritance();
			var subject = new GenericEffectThatDoesNothing<ActionForEffect>();
			Assert.True(subject.ShouldReactToAction(action));
		}

		[Fact]
		public void WhenActionTypeImplementsTriggerActionInterface_ThenReturnsTrue()
		{
			var action = new ActionThatQualifiesViaInterface();
			var subject = new GenericEffectThatDoesNothing<IInterfaceForEffect>();
			Assert.True(subject.ShouldReactToAction(action));
		}

		[Fact]
		public void WhenTriggerActionIsNotAssignableFromActionType_ThenReturnsFalse()
		{
			var action = new ActionNotForThisEffect();
			var subject = new GenericEffectThatDoesNothing<ActionForEffect>();
			Assert.False(subject.ShouldReactToAction(action));
		}
	}
}
