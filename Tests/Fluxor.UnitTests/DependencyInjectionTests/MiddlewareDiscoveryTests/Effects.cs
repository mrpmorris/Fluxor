using System.Threading.Tasks;

namespace Fluxor.UnitTests.DependencyInjectionTests.MiddlewareDiscoveryTests
{
	public class Effects
	{
		public static bool WasExecuted;

		[EffectMethod]
		public Task DoSomething(string action, IDispatcher dispatcher)
		{
			WasExecuted = true;
			return Task.CompletedTask;
		}
	}
}
