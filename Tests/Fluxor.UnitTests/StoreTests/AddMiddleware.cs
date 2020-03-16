using Fluxor.UnitTests.SupportFiles;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests
{
	public partial class StoreTests
	{
		public class AddMiddleware
		{
			[Fact]
			public async Task ActivatesMiddleware_WhenPageHasAlreadyLoaded()
			{
				var subject = new TestStore();
				await subject.InitializeAsync();

				var mockMiddleware = new Mock<IMiddleware>();
				subject.AddMiddleware(mockMiddleware.Object);

				mockMiddleware
					.Verify(x => x.InitializeAsync(subject));
			}

			[Fact]
			public async Task CallsAfterInitializeAllMiddlewares_WhenPageHasAlreadyLoaded()
			{
				var subject = new TestStore();
				var signal = new ManualResetEvent(false);
				var mockMiddleware = new Mock<IMiddleware>();
				mockMiddleware
					.Setup(x => x.AfterInitializeAllMiddlewares())
					.Callback(() => signal.Set());

				await subject.InitializeAsync();
				subject.AddMiddleware(mockMiddleware.Object);

				// Wait no more than 1 second for AfterInitializeAllMiddlewares to be executed
				signal.WaitOne(TimeSpan.FromSeconds(1));

				mockMiddleware
					.Verify(x => x.AfterInitializeAllMiddlewares());
			}
		}
	}
}
