using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Fluxor.Blazor.Web.Analyzers.CallBaseOnInitialized, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Fluxor.Blazor.Web.Analyzers.Tests;

[TestClass]
public sealed class CallBaseOnInitializedTests
{
	[TestMethod]
	public async Task WhenNotOverridden_ThenNoDiagnosticIsEmitted()
	{
		string source =
			@"""
			namespace Fluxor.Blazor.Web.Components
			{
				public class FluxorComponent { }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
			{
				void OnInitialized() { }
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[TestMethod]
	public async Task NoDiagnostic_WhenBaseIsCalled()
	{
		string test = @"
namespace Fluxor.Blazor.Web.Components
{
	public class FluxorComponent { protected virtual void OnInitialized() { } }
}
class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
{
	protected override void OnInitialized()
	{
		base.OnInitialized();
	}
}
";
		await VerifyCS.VerifyAnalyzerAsync(source: test);
	}

	[TestMethod]
	public async Task Diagnostic_WhenBaseNotCalled()
	{
		var test = @"
namespace Fluxor.Blazor.Web.Components
{
	public class FluxorComponent { protected virtual void OnInitialized() { } }
}
class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
{
	protected override void OnInitialized()
	{
		int x = 42;
	}
}
";
		var expected = VerifyCS.Diagnostic("FLXW01").WithSpan(8, 26, 8, 39);
		await VerifyCS.VerifyAnalyzerAsync(source: test, expected: new[] { expected });
	}

	[TestMethod]
	public async Task NoDiagnostic_WhenNotFluxorType()
	{
		var test = @"
class MyBaseClass { protected virtual void OnInitialized() { } }
class MyComponent : MyBaseClass
{
	protected override void OnInitialized()
	{
	}
}
";
		await VerifyCS.VerifyAnalyzerAsync(source: test);
	}

	[TestMethod]
	public async Task NoDiagnostic_WhenExpressionBodyCallsBase()
	{
		var test = @"
namespace Fluxor.Blazor.Web.Components
{
	public class FluxorComponent { protected virtual void OnInitialized() { } }
}
class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
{
	protected override void OnInitialized() => base.OnInitialized();
}
";
		await VerifyCS.VerifyAnalyzerAsync(source: test);
	}
}
