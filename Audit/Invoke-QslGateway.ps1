param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root =
(Resolve-Path -LiteralPath $Root).Path

$SolutionPath =
Join-Path $Root "QSL.sln"

$CliPath =
Join-Path $Root "QSL.Cli"

$AuditPath =
Join-Path $Root "Audit"

$AstConsistencyPath =
Join-Path $AuditPath "Invoke-AstConsistencyAudit.ps1"

$AstCoveragePath =
Join-Path $AuditPath "Invoke-AstCoverageAudit.ps1"

$PublicApiPath =
Join-Path $AuditPath "Invoke-PublicApiAudit.ps1"

$PublicSignaturePath =
Join-Path $AuditPath "Invoke-PublicSignatureAudit.ps1"

# ============================================================
# VALIDATION
# ============================================================

$RequiredPaths = @(
    $SolutionPath,
    $CliPath,
    $AstConsistencyPath,
    $AstCoveragePath,
    $PublicApiPath,
    $PublicSignaturePath
)

foreach ($Path in $RequiredPaths) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required gateway path was not found: $Path"
    }
}

# ============================================================
# HELPERS
# ============================================================

function Write-GateHeader {
    param(
        [int]$Number,
        [string]$Name
    )

    Write-Host
    Write-Host "============================================================"
    Write-Host "GATE $Number - $Name"
    Write-Host "============================================================"
    Write-Host
}

function Stop-Gateway {
    param(
        [string]$Gate,
        [string]$Message
    )

    Write-Host
    Write-Host "============================================================"
    Write-Host "QSL GATEWAY FAILED"
    Write-Host "============================================================"
    Write-Host
    Write-Host "Gate:"
    Write-Host "  $Gate"
    Write-Host
    Write-Host "Reason:"
    Write-Host "  $Message"
    Write-Host

    exit 1
}

function Invoke-PowerShellGate {
    param(
        [string]$Gate,
        [string]$ScriptPath
    )

    try {
        & $ScriptPath

        if (-not $?) {
            Stop-Gateway `
                -Gate $Gate `
                -Message "Audit script reported failure."
        }
    }
    catch {
        Stop-Gateway `
            -Gate $Gate `
            -Message $_.Exception.Message
    }
}

# ============================================================
# START
# ============================================================

Push-Location $Root

try {
    Write-Host
    Write-Host "============================================================"
    Write-Host "QSL FULL GATEWAY"
    Write-Host "============================================================"
    Write-Host
    Write-Host "Root:"
    Write-Host "  $Root"

    # ========================================================
    # GATE 1 - BUILD
    # ========================================================

    Write-GateHeader `
        -Number 1 `
        -Name "SOLUTION BUILD"

    dotnet build $SolutionPath

    if ($LASTEXITCODE -ne 0) {
        Stop-Gateway `
            -Gate "SOLUTION BUILD" `
            -Message "dotnet build returned exit code $LASTEXITCODE."
    }

    # ========================================================
    # GATE 2 - CERTIFICATION
    # ========================================================

    Write-GateHeader `
        -Number 2 `
        -Name "200-TEST CERTIFICATION"

    dotnet run `
        --project $CliPath `
        -- certify

    if ($LASTEXITCODE -ne 0) {
        Stop-Gateway `
            -Gate "200-TEST CERTIFICATION" `
            -Message "Certification returned exit code $LASTEXITCODE."
    }

    # ========================================================
    # GATE 3 - AST CONSISTENCY
    # ========================================================

    Write-GateHeader `
        -Number 3 `
        -Name "AST CONSISTENCY"

    Invoke-PowerShellGate `
        -Gate "AST CONSISTENCY" `
        -ScriptPath $AstConsistencyPath

    # ========================================================
    # GATE 4 - AST COVERAGE
    # ========================================================

    Write-GateHeader `
        -Number 4 `
        -Name "AST COVERAGE"

    Invoke-PowerShellGate `
        -Gate "AST COVERAGE" `
        -ScriptPath $AstCoveragePath

    # ========================================================
    # GATE 5 - PUBLIC API
    # ========================================================

    Write-GateHeader `
        -Number 5 `
        -Name "PUBLIC API"

    Invoke-PowerShellGate `
        -Gate "PUBLIC API" `
        -ScriptPath $PublicApiPath

    # ========================================================
    # GATE 6 - PUBLIC SIGNATURE REACHABILITY
    # ========================================================

    Write-GateHeader `
        -Number 6 `
        -Name "PUBLIC SIGNATURE REACHABILITY"

    Invoke-PowerShellGate `
        -Gate "PUBLIC SIGNATURE REACHABILITY" `
        -ScriptPath $PublicSignaturePath

    # ========================================================
    # GATE 7 - GIT WHITESPACE
    # ========================================================

    Write-GateHeader `
        -Number 7 `
        -Name "GIT WHITESPACE"

    $WhitespaceProblems = @(
        git diff --check
    )

    if ($LASTEXITCODE -ne 0) {
        $WhitespaceProblems |
                ForEach-Object {
                    Write-Host $_
                }

        Stop-Gateway `
            -Gate "GIT WHITESPACE" `
            -Message "git diff --check found problems."
    }

    if ($WhitespaceProblems.Count -gt 0) {
        $WhitespaceProblems |
                ForEach-Object {
                    Write-Host $_
                }

        Stop-Gateway `
            -Gate "GIT WHITESPACE" `
            -Message "Whitespace problems were reported."
    }

    Write-Host "Working-tree whitespace check passed."

    # ========================================================
    # GATE 8 - REPOSITORY STATE
    # ========================================================

    Write-GateHeader `
        -Number 8 `
        -Name "REPOSITORY STATE"

    $GitStatus = @(
        git status --short
    )

    if ($LASTEXITCODE -ne 0) {
        Stop-Gateway `
            -Gate "REPOSITORY STATE" `
            -Message "git status returned exit code $LASTEXITCODE."
    }

    if ($GitStatus.Count -gt 0) {
        $GitStatus |
                ForEach-Object {
                    Write-Host $_
                }

        Stop-Gateway `
            -Gate "REPOSITORY STATE" `
            -Message "Working tree is not clean."
    }

    Write-Host "Working tree is clean."

    # ========================================================
    # SUCCESS
    # ========================================================

    Write-Host
    Write-Host "============================================================"
    Write-Host "QSL GATEWAY PASSED"
    Write-Host "============================================================"
    Write-Host
    Write-Host "Build:                         PASS"
    Write-Host "Certification:                 PASS"
    Write-Host "AST consistency:               PASS"
    Write-Host "AST coverage:                  PASS"
    Write-Host "Public API:                    PASS"
    Write-Host "Public signature reachability: PASS"
    Write-Host "Git whitespace:                PASS"
    Write-Host "Repository state:              CLEAN"
    Write-Host
    Write-Host "STATUS: CERTIFIED"
    Write-Host

    exit 0
}
finally {
    Pop-Location
}