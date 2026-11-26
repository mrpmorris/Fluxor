// Types in different namespaces, used to check which namespaces a scan includes.
// A namespace that contains a Middleware is skipped by assembly scans unless that middleware is added.

namespace Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.Visible
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading.Tasks;

	// Visible to every assembly scan. It is an effect rather than a feature so that it does not change the
	// feature counts other tests assert; each test dispatches its own Id, so tests cannot see each other's runs.
	public record ProbeAction(Guid Id);

	public static class ProbeEffects
	{
		public static readonly ConcurrentDictionary<Guid, bool> Seen = new();

		[EffectMethod]
		public static Task Handle(ProbeAction action, IDispatcher dispatcher)
		{
			Seen[action.Id] = true;
			return Task.CompletedTask;
		}
	}
}

namespace Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.Hidden
{
	public class HiddenMarker : Middleware { }

	[FeatureState]
	public record HiddenState(int Value)
	{
		private HiddenState() : this(0) { }
	}
}

namespace Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.HiddenSibling
{
	// Shares the "Hidden" prefix but is a different namespace, so including "Hidden" must not include it.
	public class HiddenSiblingMarker : Middleware { }

	[FeatureState]
	public record HiddenSiblingState(int Value)
	{
		private HiddenSiblingState() : this(0) { }
	}
}

namespace Fluxor.UnitTests.DependencyInjectionTests.ScanOptionsTests.SupportFiles.Hidden2
{
	public class Hidden2Marker : Middleware { }

	[FeatureState]
	public record Hidden2State(int Value)
	{
		private Hidden2State() : this(0) { }
	}
}
