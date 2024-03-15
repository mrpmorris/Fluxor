using System.Collections.Generic;

namespace Fluxor.UnitTests.DependencyInjectionTests.ReducerDiscoveryTests.DiscoverGenericReducersWithActionInMethodSignatureTests.SupportFiles;

public class TestFeature : Feature<TestState<char>>
{
	public override string GetName() => "Test";
	protected override TestState<char> GetInitialState() =>
		new TestState<char>(new Dictionary<char, int>
		{
			['A'] = 0,
			['B'] = 0
		});
}
