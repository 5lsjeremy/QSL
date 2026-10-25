param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root =
(Resolve-Path -LiteralPath $Root).Path

$CliProject =
Join-Path `
        $Root `
        "QSL.Cli\QSL.Cli.csproj"

$ReportsPath =
Join-Path `
        $Root `
        "Audit\Reports"

if (-not (Test-Path -LiteralPath $CliProject)) {
    throw "QSL CLI project was not found: $CliProject"
}

Write-Host ""
Write-Host "========================================"
Write-Host "QSL AST COVERAGE AUDIT"
Write-Host "========================================"
Write-Host ""
Write-Host "Root:"
Write-Host "  $Root"
Write-Host ""
Write-Host "CLI:"
Write-Host "  $CliProject"
Write-Host ""

Push-Location $Root

try {
    dotnet run `
        --project $CliProject `
        -- audit-ast-coverage

    $ExitCode =
    $LASTEXITCODE
}
finally {
    Pop-Location
}

Write-Host ""

if ($ExitCode -eq 0) {
    Write-Host "AST coverage audit passed." `
        -ForegroundColor Green
}
else {
    Write-Host "AST coverage audit failed." `
        -ForegroundColor Red
}

Write-Host ""
Write-Host "Exit code:"
Write-Host "  $ExitCode"
Write-Host ""
Write-Host "Reports:"
Write-Host "  $ReportsPath"
Write-Host ""

exit $ExitCode