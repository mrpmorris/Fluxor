using System;

namespace Fluxor.UnitTests.SupportFiles
{
	public class MiddlewareWithExposedMembers : Middleware
	{
		public int OnInternalMiddlewareChangeEndingCallCount;
		public int _BeginMiddlewareChangeCount => BeginMiddlewareChangeCount;
		public bool _IsInsideMiddlewareChange => IsInsideMiddlewareChange;
		public IDisposable _BeginInternalMiddlewareChange() =>
			((IMiddleware)this).BeginInternalMiddlewareChange();

		protected override void OnInternalMiddlewareChangeEnding()
		{
			base.OnInternalMiddlewareChangeEnding();
			OnInternalMiddlewareChangeEndingCallCount++;
		}
	}
}
