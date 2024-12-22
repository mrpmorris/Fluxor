using Microsoft.CodeAnalysis.Testing;
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
		string source = @"
			namespace Fluxor.Blazor.Web.Components
			{
				public class FluxorComponent { }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
			{
				void OnInitialized() { }
			}
			";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[TestMethod]
	public async Task WhenBaseOnInitializedIsExecuted_ThenNoDiagnosticIsEmitted()
	{
		string source = @"
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
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[TestMethod]
	public async Task WhenBaseOnInitializedAsyncIsExecuted_ThenNoDiagnosticIsEmitted()
	{
		string source = @"
			using System.Threading.Tasks;
			namespace Fluxor.Blazor.Web.Components
			{
				public class FluxorComponent { protected virtual Task OnInitializedAsync() { return Task.CompletedTask; } }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
			{
				protected override async Task OnInitializedAsync()
				{
					await base.OnInitializedAsync();
				}
			}
			";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[TestMethod]
	public async Task WhenBaseOnInitializedIsNotCalled_ThenDiagnosticIsEmitted()
	{
		string source = @"
			namespace Fluxor.Blazor.Web.Components
			{
				public class FluxorComponent { protected virtual void OnInitialized() { } }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
			{
				protected override void OnInitialized()
				{
				}
			}
			";

		DiagnosticResult expected = VerifyCS.Diagnostic("FLXW01").WithSpan(8, 29, 8, 42);
		await VerifyCS.VerifyAnalyzerAsync(
			source: source,
			expected: [expected]);
	}

	[TestMethod]
	public async Task WhenBaseOnInitializedAsyncIsNotCalled_ThenDiagnosticIsEmitted()
	{
		string source = @"
			using System.Threading.Tasks;
			namespace Fluxor.Blazor.Web.Components
			{
				public class FluxorComponent { protected virtual Task OnInitializedAsync() { return Task.CompletedTask; } }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.FluxorComponent
			{
				protected override Task OnInitializedAsync()
				{
					return Task.CompletedTask;
				}
			}
			";

		DiagnosticResult expected = VerifyCS.Diagnostic("FLXW01").WithSpan(9, 29, 9, 47);
		await VerifyCS.VerifyAnalyzerAsync(
			source: source,
			expected: [expected]);
	}
}
