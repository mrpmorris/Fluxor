using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<Fluxor.Reactor.Maui.Analyzers.CallBaseOnMounted, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace Fluxor.Reactor.Maui.Analyzers.Tests;

[TestClass]
public sealed class CallBaseOnMountedTests
{
	[DataTestMethod]
	[DataRow("FluxorComponent")]
	public async Task WhenNotOverridden_ThenNoDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			namespace Fluxor.Reactor.Maui.Components
			{
				public class {{baseClass}} { }
			}
			class MyComponent : Fluxor.Reactor.Maui.Components.{{baseClass}}
			{
				void OnMounted() { }
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenBaseOnMountedIsExecuted_ThenNoDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			namespace Fluxor.Reactor.Maui.Components
			{
				public class {{baseClass}} { protected virtual void OnMounted() { } }
			}
			class MyComponent : Fluxor.Reactor.Maui.Components.{{baseClass}}
			{
				protected override void OnMounted()
				{
					base.OnMounted();
				}
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	[DataRow("FluxorLayout")]
	public async Task WhenBaseOnMountedAsyncIsExecuted_ThenNoDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			using System.Threading.Tasks;
			namespace Fluxor.Reactor.Maui.Components
			{
				public class {{baseClass}} { protected virtual Task OnMountedAsync() { return Task.CompletedTask; } }
			}

			class MyComponent1 : Fluxor.Reactor.Maui.Components.{{baseClass}}
			{
				protected override async Task OnMountedAsync()
				{
					await base.OnMountedAsync();
				}
			}
			
			class MyComponent2 : Fluxor.Reactor.Maui.Components.{{baseClass}}
			{
				protected override Task OnMountedAsync()
				{
					return base.OnMountedAsync();
				}
			}
			""";
		await VerifyCS.VerifyAnalyzerAsync(source);
	}

	[DataTestMethod]
	[DataRow("FluxorComponent")]
	public async Task WhenBaseOnMountedIsNotCalled_ThenDiagnosticIsEmitted(string baseClass)
	{
		string source = $$"""
			namespace Fluxor.Reactor.Maui.Components
			{
				public class {{baseClass}} { protected virtual void OnMounted() { } }
			}
			class MyComponent : Fluxor.Reactor.Maui.Components.{{baseClass}}
			{
				protected override void OnMounted()
				{
				}
			}
			""";

		DiagnosticResult expected = VerifyCS.Diagnostic("FLXM01").WithSpan(7, 26, 7, 35);
		await VerifyCS.VerifyAnalyzerAsync(
			source: source,
			expected: [expected]);
	}
}
