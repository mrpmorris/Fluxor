//using System.Threading.Tasks;
//using Xunit;
//using VerifyCS = Microsoft.CodeAnalysis.CSharp.

//namespace Fluxor.Blazor.Web.Analyzers.Tests;

//public sealed class CallBaseOnInitializedTests
//{
//	[Fact]
//	public async Task NoDiagnostic_WhenNotOverridden()
//	{
//		var test = @"
//namespace Fluxor.Blazor.Web.Components
//{
//	public class FluxorComponent { }
//}
//class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
//{
//	void OnInitialized() { }
//}
//";
//		await VerifyCS.VerifyAnalyzerAsync(source: test);
//	}

//	[Fact]
//	public async Task NoDiagnostic_WhenBaseIsCalled()
//	{
//		var test = @"
//namespace Fluxor.Blazor.Web.Components
//{
//	public class FluxorComponent { protected virtual void OnInitialized() { } }
//}
//class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
//{
//	protected override void OnInitialized()
//	{
//		base.OnInitialized();
//	}
//}
//";
//		await VerifyCS.VerifyAnalyzerAsync(source: test);
//	}

//	[Fact]
//	public async Task Diagnostic_WhenBaseNotCalled()
//	{
//		var test = @"
//namespace Fluxor.Blazor.Web.Components
//{
//	public class FluxorComponent { protected virtual void OnInitialized() { } }
//}
//class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
//{
//	protected override void OnInitialized()
//	{
//		int x = 42;
//	}
//}
//";
//		var expected = VerifyCS.Diagnostic("FLXW01").WithSpan(10, 19, 10, 32);
//		await VerifyCS.VerifyAnalyzerAsync(source: test, expectedDiagnostics: new[] { expected });
//	}

//	[Fact]
//	public async Task NoDiagnostic_WhenNotFluxorType()
//	{
//		var test = @"
//class MyBaseClass { protected virtual void OnInitialized() { } }
//class MyComponent : MyBaseClass
//{
//	protected override void OnInitialized()
//	{
//	}
//}
//";
//		await VerifyCS.VerifyAnalyzerAsync(source: test);
//	}

//	[Fact]
//	public async Task NoDiagnostic_WhenExpressionBodyCallsBase()
//	{
//		var test = @"
//namespace Fluxor.Blazor.Web.Components
//{
//	public class FluxorComponent { protected virtual void OnInitialized() { } }
//}
//class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
//{
//	protected override void OnInitialized() => base.OnInitialized();
//}
//";
//		await VerifyCS.VerifyAnalyzerAsync(source: test);
//	}
//}
