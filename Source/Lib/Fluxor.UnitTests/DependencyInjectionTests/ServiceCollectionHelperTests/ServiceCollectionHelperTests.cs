using Fluxor.DependencyInjection;
using Fluxor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.ServiceCollectionHelperTests;

/// <summary>
/// The lifetime-aware Add helpers are public so that middleware libraries can register services
/// with the same lifetime as the store.
/// </summary>
public class ServiceCollectionHelperTests
{
	public interface IService { }
	public class Service : IService { }

	private static (ServiceCollection Services, FluxorOptions Options) Create(StoreLifetime lifetime)
	{
		var services = new ServiceCollection();
		return (services, new FluxorOptions(services).WithLifetime(lifetime));
	}

	[Theory]
	[InlineData(StoreLifetime.Scoped, ServiceLifetime.Scoped)]
	[InlineData(StoreLifetime.Singleton, ServiceLifetime.Singleton)]
	public void WhenAddingATypeWithTheStoreLifetime_ThenItIsRegisteredWithThatLifetime(
		StoreLifetime storeLifetime,
		ServiceLifetime expectedLifetime)
	{
		var (services, options) = Create(storeLifetime);

		services.Add<Service>(options);

		ServiceDescriptor descriptor = Assert.Single(services, x => x.ServiceType == typeof(Service));
		Assert.Equal(expectedLifetime, descriptor.Lifetime);
		Assert.Equal(typeof(Service), descriptor.ImplementationType);
	}

	[Theory]
	[InlineData(StoreLifetime.Scoped, ServiceLifetime.Scoped)]
	[InlineData(StoreLifetime.Singleton, ServiceLifetime.Singleton)]
	public void WhenAddingAFactoryWithTheStoreLifetime_ThenItIsRegisteredWithThatLifetime(
		StoreLifetime storeLifetime,
		ServiceLifetime expectedLifetime)
	{
		var (services, options) = Create(storeLifetime);
		var instance = new Service();

		services.Add<IService, Service>(_ => instance, options);

		ServiceDescriptor descriptor = Assert.Single(services, x => x.ServiceType == typeof(IService));
		Assert.Equal(expectedLifetime, descriptor.Lifetime);
		Assert.Same(instance, services.BuildServiceProvider().GetRequiredService<IService>());
	}
}
