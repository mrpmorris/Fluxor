using Fluxor.UnitTests.DependencyInjectionTests.IsolationTests.SupportFiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Fluxor.UnitTests.DependencyInjectionTests.IsolationTests
{
   public class LifeCycleTests
   {
      [Fact]
      public void WhenStoreIsCreatedWithScopedLifecycle_ItIsUniqueToDependencyInjectionScope()
      {
         IServiceProvider serviceProvider = SetupServiceProvider(LifecycleEnum.Scoped);

         (IServiceScope scope1, IStore store1, IDispatcher dispatcher1, IState<CounterState> state1) = CreateStore(serviceProvider);
         (IServiceScope scope2, IStore store2, IDispatcher dispatcher2, IState<CounterState> state2) = CreateStore(serviceProvider);

         IncrementCounterAction action = new IncrementCounterAction();

         Assert.NotEqual(scope1, scope2);
         Assert.NotEqual(store1, store2);
         Assert.NotEqual(dispatcher1, dispatcher2);
         Assert.NotEqual(state1, state2);

         Assert.Equal(0, state1.Value.Counter);
         Assert.Equal(0, state2.Value.Counter);

         dispatcher1.Dispatch(action);
         Assert.Equal(1, state1.Value.Counter);
         Assert.Equal(0, state2.Value.Counter);

         dispatcher2.Dispatch(action);
         Assert.Equal(1, state1.Value.Counter);
         Assert.Equal(1, state2.Value.Counter);

         dispatcher2.Dispatch(action);
         Assert.Equal(1, state1.Value.Counter);
         Assert.Equal(2, state2.Value.Counter);

         scope1.Dispose();
         scope2.Dispose();
      }

      [Fact]
      public void WhenStoreIsCreatedWithSingletonLifecycle_ItIsNotUniqueToDependencyInjectionScope()
      {
         IServiceProvider serviceProvider = SetupServiceProvider(LifecycleEnum.Singleton);

         (IServiceScope scope1, IStore store1, IDispatcher dispatcher1, IState<CounterState> state1) = CreateStore(serviceProvider);
         (IServiceScope scope2, IStore store2, IDispatcher dispatcher2, IState<CounterState> state2) = CreateStore(serviceProvider);

         IncrementCounterAction action = new IncrementCounterAction();

         Assert.NotEqual(scope1, scope2);
         Assert.Equal(store1, store2);
         Assert.Equal(dispatcher1, dispatcher2);
         Assert.Equal(state1, state2);

         Assert.Equal(0, state1.Value.Counter);
         Assert.Equal(0, state2.Value.Counter);

         dispatcher1.Dispatch(action);
         Assert.Equal(1, state1.Value.Counter);
         Assert.Equal(1, state2.Value.Counter);

         dispatcher2.Dispatch(action);
         Assert.Equal(2, state1.Value.Counter);
         Assert.Equal(2, state2.Value.Counter);

         dispatcher2.Dispatch(action);
         Assert.Equal(3, state1.Value.Counter);
         Assert.Equal(3, state2.Value.Counter);
      }

      private static IServiceProvider SetupServiceProvider(LifecycleEnum lifecycle)
      {
         ServiceCollection services = new ServiceCollection();
         services.AddFluxor(x => x
            .SetRegistrationLifecycle(lifecycle)
            .AddMiddleware<IsolatedTests>()
            .ScanAssemblies(typeof(IsolatedTests).Assembly));
         IServiceProvider serviceProvider = services.BuildServiceProvider();
         return serviceProvider;
      }

      private static (IServiceScope, IStore, IDispatcher, IState<CounterState>) CreateStore(IServiceProvider serviceProvider)
      {
         IServiceScope serviceScope = serviceProvider.CreateScope();
         IServiceProvider serviceScopeProvider = serviceScope.ServiceProvider;

         IStore store = serviceScopeProvider.GetRequiredService<IStore>();
         store.InitializeAsync().Wait();
         IDispatcher dispatcher = serviceScopeProvider.GetRequiredService<IDispatcher>();
         IState<CounterState> state = serviceScopeProvider.GetRequiredService<IState<CounterState>>();

         return (serviceScope, store, dispatcher, state);
      }
   }
}
