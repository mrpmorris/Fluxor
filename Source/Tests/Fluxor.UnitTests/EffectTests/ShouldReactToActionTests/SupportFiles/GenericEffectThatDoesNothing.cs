using System.Threading.Tasks;

namespace Fluxor.UnitTests.EffectTests.ShouldReactToActionTests.SupportFiles
{
	public class GenericEffectThatDoesNothing<TTriggerAction> : Effect<TTriggerAction>
	{
		public override Task HandleAsync(TTriggerAction action, IDispatcher dispatcher) => Task.CompletedTask;
	}
}
