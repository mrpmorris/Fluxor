<#
.SYNOPSIS
	Runs Stryker.NET mutation testing on the Fluxor libraries (on demand, not part of CI).

.DESCRIPTION
	Each library has its own config: Source/stryker-<library>.json.
	See Docs/mutation-testing.md.

.EXAMPLE
	./Source/run-mutation-tests.ps1                  # all libraries
	./Source/run-mutation-tests.ps1 -Library fluxor  # one library
#>
param(
	[ValidateSet('all', 'fluxor', 'blazor-web', 'reduxdevtools')]
	[string] $Library = 'all',
	[string] $Output = (Join-Path $PSScriptRoot '..\StrykerOutput')
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$libraries = if ($Library -eq 'all') { 'fluxor', 'blazor-web', 'reduxdevtools' } else { , $Library }

$failed = @()
Push-Location $root
try {
	dotnet tool restore | Out-Null
	foreach ($name in $libraries) {
		Write-Host "=== Stryker: $name" -ForegroundColor Cyan
		dotnet stryker -f "Source/stryker-$name.json" -O (Join-Path $Output $name)
		if ($LASTEXITCODE -ne 0) { $failed += $name }
	}
}
finally {
	Pop-Location
}

if ($failed) { Write-Host "Below the break threshold or failed: $($failed -join ', ')" -ForegroundColor Red; exit 1 }
Write-Host "Reports: $Output\<library>\reports\mutation-report.html" -ForegroundColor Green
