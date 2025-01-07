using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Fluxor.Blazor.Web.Analyzers.CallBaseOnInitialized, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Fluxor.Blazor.Web.Analyzers.Tests;

[TestClass]
public sealed class CallBaseOnInitializedTests
{
	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenNotOverridden_ThenNoDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			namespace Fluxor.Blazor.Web.Components
			{
				public class {{baseClass}} { }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.{{baseClass}}
			{
				void OnInitialized() { }
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenBaseOnInitializedIsExecuted_ThenNoDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			namespace Fluxor.Blazor.Web.Components
			{
				public class {{baseClass}} { protected virtual void OnInitialized() { } }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.{{baseClass}}
			{
				protected override void OnInitialized()
				{
					base.OnInitialized();
				}
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenBaseOnInitializedAsyncIsExecuted_ThenNoDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			using System.Threading.Tasks;
			namespace Fluxor.Blazor.Web.Components
			{
				public class {{baseClass}} { protected virtual Task OnInitializedAsync() { return Task.CompletedTask; } }
			}

			class MyComponent1 : Fluxor.Blazor.Web.Components.{{baseClass}}
			{
				protected override async Task OnInitializedAsync()
				{
					await base.OnInitializedAsync();
				}
			}
			
			class MyComponent2 : Fluxor.Blazor.Web.Components.{{baseClass}}
			{
				protected override Task OnInitializedAsync()
				{
					return base.OnInitializedAsync();
				}
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenBaseOnInitializedIsNotCalled_ThenDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			namespace Fluxor.Blazor.Web.Components
			{
				public class {{baseClass}} { protected virtual void OnInitialized() { } }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.{{baseClass}}
			{
				protected override void OnInitialized()
				{
				}
			}
			""";

		DiagnosticResult expected = VerifyCS.Diagnostic("FLXW01").WithSpan(7, 26, 7, 39);
		await VerifyCS.VerifyAnalyzerAsync(
			source: source,
			expected: [expected]);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenBaseOnInitializedAsyncIsNotCalled_ThenDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			using System.Threading.Tasks;
			namespace Fluxor.Blazor.Web.Components
			{
				public class {{baseClass}} { protected virtual Task OnInitializedAsync() { return Task.CompletedTask; } }
			}
			class MyComponent : Fluxor.Blazor.Web.Components.{{baseClass}}
			{
				protected override Task OnInitializedAsync()
				{
					return Task.CompletedTask;
				}
			}
			""";

		DiagnosticResult expected = VerifyCS.Diagnostic("FLXW01").WithSpan(8, 26, 8, 44);
		await VerifyCS.VerifyAnalyzerAsync(
			source: source,
			expected: [expected]);
	}
}
