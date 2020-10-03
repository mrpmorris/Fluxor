using System.Threading.Tasks;

namespace Fluxor.UnitTests.EffectTests.ShouldReactToActionTests.SupportFiles
{
	public class GenericEffectThatDoesNothing<TTriggerAction> : Effect<TTriggerAction>
	{
		protected override Task HandleAsync(TTriggerAction action, IDispatcher dispatcher) => Task.CompletedTask;
	}
}
