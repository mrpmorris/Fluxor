using Fluxor.UnitTests.SupportFiles;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fluxor.UnitTests.StoreTests
{
	public partial class StoreTests
	{
		public class Initialize
		{
			[Fact]
			public async Task ActivatesMiddleware_WhenStoreInitializerCompletes()
			{
				var subject = new TestStore();
				await subject.InitializeAsync();
				var mockMiddleware = new Mock<IMiddleware>();
				subject.AddMiddleware(mockMiddleware.Object);

				mockMiddleware
					.Verify(x => x.InitializeAsync(subject));
			}

			[Fact]
			public async Task CallsAfterInitializeAllMiddlewares_WhenStoreInitializerCompletes()
			{
				var subject = new TestStore();
				var mockMiddleware = new Mock<IMiddleware>();
				subject.AddMiddleware(mockMiddleware.Object);

				await subject.InitializeAsync();

				mockMiddleware
					.Verify(x => x.AfterInitializeAllMiddlewares());
			}
		}
	}
}