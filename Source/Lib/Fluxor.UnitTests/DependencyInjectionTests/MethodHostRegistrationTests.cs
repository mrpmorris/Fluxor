using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests;

public class MethodHostRegistrationTests
{
	[Fact]
	public void WhenStaticMethodsAreDeclaredOnRecord_ThenRecordIsNotRegisteredInDependencyInjection()
	{
		var services = new ServiceCollection();

		services.AddFluxor(x => x.ScanTypes(typeof(StaticMethodRecordAction)));

		Assert.DoesNotContain(services, x => x.ServiceType == typeof(StaticMethodRecordAction));
		using ServiceProvider _ = services.BuildServiceProvider(new ServiceProviderOptions
		{
			ValidateOnBuild = true,
			ValidateScopes = true
		});
	}

	[Fact]
	public void WhenTypeHostsBothReducerAndEffectMethods_ThenItIsOnlyRegisteredOnce()
	{
		var services = new ServiceCollection();

		services.AddFluxor(x => x.ScanTypes(typeof(CombinedMethodHost)));

		Assert.Equal(1, services.Count(x => x.ServiceType == typeof(CombinedMethodHost)));
	}

	public record TestState(int Count);

	public record StaticMethodRecordAction(string Value)
	{
		[ReducerMethod]
		public static TestState Reduce(TestState state, StaticMethodRecordAction action) =>
			new(state.Count + 1);

		[EffectMethod]
		public static Task Handle(StaticMethodRecordAction action, IDispatcher dispatcher) =>
			Task.CompletedTask;
	}

	public class CombinedMethodHost
	{
		[ReducerMethod]
		public TestState Reduce(TestState state, CombinedAction action) =>
			new(state.Count + 1);

		[EffectMethod]
		public Task Handle(CombinedAction action, IDispatcher dispatcher) =>
			Task.CompletedTask;
	}

	public record CombinedAction;
}
