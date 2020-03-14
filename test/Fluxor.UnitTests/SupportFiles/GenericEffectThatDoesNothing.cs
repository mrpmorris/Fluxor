using System.Threading.Tasks;

namespace Fluxor.UnitTests.SupportFiles
{
	public class GenericEffectThatDoesNothing<TTriggerAction> : Effect<TTriggerAction>
	{
		protected override Task HandleAsync(TTriggerAction action, IDispatcher dispatcher) => Task.CompletedTask;
	}
}
