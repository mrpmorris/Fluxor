﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Fluxor.UnitTests.DependencyInjectionTests.IsolationTests.SupportFiles;

namespace Fluxor.UnitTests.DependencyInjectionTests.IsolationTests
{
	public class IsolationTests
	{
		[Fact]
		public void WhenStoreIsCreated_ItIsUniqueToDependencyInjectionScope()
		{
			IDispatcher dispatcher1;
			IState<CounterState> state1;
			(dispatcher1, state1) = CreateStore();

			IDispatcher dispatcher2;
			IState<CounterState> state2;
			(dispatcher2, state2) = CreateStore();

			var action = new IncrementCounterAction();

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
		}

		private static (IDispatcher, IState<CounterState>) CreateStore()
		{
			var services = new ServiceCollection();
			services.AddFluxor(x => x
				.AddMiddleware<IsolatedTests>()
				.ScanAssemblies(typeof(IsolatedTests).Assembly));
			IServiceProvider serviceProvider = services.BuildServiceProvider();

			serviceProvider.GetRequiredService<IStore>().InitializeAsync().Wait();
			var dispatcher = serviceProvider.GetRequiredService<IDispatcher>();
			var state = serviceProvider.GetRequiredService<IState<CounterState>>();

			return (dispatcher, state);
		}
	}
}
