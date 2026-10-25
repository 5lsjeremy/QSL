param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root = (Resolve-Path -LiteralPath $Root).Path
$ReportsPath = Join-Path $Root "Audit\Reports"

New-Item `
    -ItemType Directory `
    -Path $ReportsPath `
    -Force |
        Out-Null

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$ReportPath = Join-Path `
    $ReportsPath `
    "QSL_Legacy_Audit_$Timestamp.txt"

$LatestPath = Join-Path `
    $ReportsPath `
    "QSL_Legacy_Audit_Latest.txt"

# ============================================================
# SOURCE INVENTORY
# ============================================================

$SourceFiles = @(
Get-ChildItem `
        -LiteralPath $Root `
        -Recurse `
        -File `
        -Filter "*.cs" |
        Where-Object {
            $_.FullName -notmatch '\\bin\\' -and
                    $_.FullName -notmatch '\\obj\\' -and
                    $_.FullName -notmatch '\\\.idea\\'
        } |
        Sort-Object FullName
)

$SourcePaths = @(
$SourceFiles |
        ForEach-Object {
            $_.FullName
        }
)

if ($SourceFiles.Count -eq 0) {
    throw "No C# source files found."
}

# ============================================================
# HELPERS
# ============================================================

$Report =
New-Object System.Collections.Generic.List[string]

function Add-Line {
    param(
        [string]$Text = ""
    )

    $script:Report.Add($Text)
}

function Add-Header {
    param(
        [string]$Title
    )

    Add-Line
    Add-Line "============================================================"
    Add-Line $Title
    Add-Line "============================================================"
}

function Get-RelativePath {
    param(
        [string]$Path
    )

    return $Path.Substring(
            $Root.Length
    ).TrimStart('\')
}

# ============================================================
# TYPES TO AUDIT
# ============================================================

$Candidates = @(
    "QslBaseObject",
    "QslRegistryItem",
    "QslRuntimeObject",
    "IQSLObject",
    "IQslRegistryItem",
    "IQslRuntimeObject",
    "IQslCapability",
    "IQslDescriptor",
    "IQslEvaluator",
    "IQslFunction",
    "IQslNode",
    "IQslOperator",
    "QslOperatorBase",
    "QslOperatorResolver"
)

# Add concrete operator classes automatically.

$OperatorPath =
Join-Path `
        $Root `
        "Core\Runtime\Operators"

if (Test-Path -LiteralPath $OperatorPath) {

    $OperatorFiles = @(
        Get-ChildItem `
            -LiteralPath $OperatorPath `
            -File `
            -Filter "*.cs"
    )

    foreach ($File in $OperatorFiles) {

        if ($Candidates -notcontains $File.BaseName) {
            $Candidates += $File.BaseName
        }
    }
}

$Candidates =
@(
$Candidates |
        Sort-Object -Unique
)

# ============================================================
# HEADER
# ============================================================

Add-Line "============================================================"
Add-Line "QSL LEGACY / DEAD-CODE AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"
Add-Line "C# files scanned: $($SourceFiles.Count)"
Add-Line "Candidates: $($Candidates.Count)"
Add-Line
Add-Line "This report identifies review candidates only."
Add-Line "It does not authorize deletion."

# ============================================================
# ANALYZE CANDIDATES
# ============================================================

$Unreferenced = @()
$Referenced = @()

foreach ($Candidate in $Candidates) {

    Add-Header $Candidate

    $DefinitionMatches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -Pattern (
        '\b(class|interface|struct)\s+' +
                $Candidate +
                '\b'
        )
    )

    if ($DefinitionMatches.Count -eq 0) {
        Add-Line "Definition: <not found>"
        Add-Line "Status: NOT FOUND"
        continue
    }

    foreach ($Definition in $DefinitionMatches) {
        Add-Line (
        "Definition: " +
                (Get-RelativePath $Definition.Path) +
                ":" +
                $Definition.LineNumber
        )

        Add-Line (
        "  " +
                $Definition.Line.Trim()
        )
    }

    $AllMatches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Candidate
    )

    $References = @(
    foreach ($Match in $AllMatches) {

        $IsDefinition = $false

        foreach ($Definition in $DefinitionMatches) {
            if (
            $Match.Path -eq $Definition.Path -and
                    $Match.LineNumber -eq $Definition.LineNumber
            ) {
                $IsDefinition = $true
                break
            }
        }

        if (-not $IsDefinition) {
            $Match
        }
    }
    )

    Add-Line "References: $($References.Count)"

    if ($References.Count -eq 0) {
        Add-Line "Status: UNREFERENCED"
        $Unreferenced += $Candidate
    }
    else {
        Add-Line "Status: REFERENCED"
        $Referenced += $Candidate
    }

    foreach ($Reference in $References) {
        Add-Line (
        "  " +
                (Get-RelativePath $Reference.Path) +
                ":" +
                $Reference.LineNumber +
                "  " +
                $Reference.Line.Trim()
        )
    }
}

# ============================================================
# CORE BASE VISIBILITY
# ============================================================

Add-Header "CORE BASE VISIBILITY"

$CoreBasePath =
Join-Path `
        $Root `
        "Core\Base"

if (Test-Path -LiteralPath $CoreBasePath) {

    $BaseFiles = @(
    Get-ChildItem `
            -LiteralPath $CoreBasePath `
            -File `
            -Filter "*.cs" |
            Sort-Object Name
    )

    foreach ($File in $BaseFiles) {

        Add-Line (
        Get-RelativePath $File.FullName
        )

        $Usings = @(
            Select-String `
                -LiteralPath $File.FullName `
                -Pattern '^\s*using\s+'
        )

        foreach ($Using in $Usings) {
            Add-Line (
            "  " +
                    $Using.Line.Trim()
            )
        }
    }
}
else {
    Add-Line "<Core\Base not found>"
}

Add-Line
Add-Line "Promotion question:"
Add-Line "Can each Core\Base type move to Abstractions\Base"
Add-Line "without introducing an Abstractions -> Core dependency?"

# ============================================================
# ABSTRACTIONS INVENTORY
# ============================================================

Add-Header "ABSTRACTIONS INVENTORY"

$AbstractionsPath =
Join-Path `
        $Root `
        "Abstractions"

if (Test-Path -LiteralPath $AbstractionsPath) {

    Get-ChildItem `
        -LiteralPath $AbstractionsPath `
        -Recurse `
        -File `
        -Filter "*.cs" |
            Sort-Object FullName |
            ForEach-Object {
                Add-Line (
                Get-RelativePath $_.FullName
                )
            }
}

# ============================================================
# SUMMARY
# ============================================================

Add-Header "SUMMARY"

Add-Line "C# files scanned: $($SourceFiles.Count)"
Add-Line "Candidates inspected: $($Candidates.Count)"
Add-Line "Referenced candidates: $($Referenced.Count)"
Add-Line "Unreferenced candidates: $($Unreferenced.Count)"

Add-Line

if ($Unreferenced.Count -eq 0) {
    Add-Line "Unreferenced candidates: <none>"
}
else {
    Add-Line "Unreferenced candidates:"

    foreach ($Candidate in $Unreferenced) {
        Add-Line "  $Candidate"
    }
}

Add-Line
Add-Line "STATUS: LEGACY REVIEW READY"
Add-Line
Add-Line "No QSL source files were modified."

# ============================================================
# WRITE
# ============================================================

$Report |
        Set-Content `
        -LiteralPath $ReportPath `
        -Encoding UTF8

$Report |
        Set-Content `
        -LiteralPath $LatestPath `
        -Encoding UTF8

Write-Host
Write-Host "========================================"
Write-Host "QSL LEGACY AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "C# files: $($SourceFiles.Count)"
Write-Host "Candidates: $($Candidates.Count)"
Write-Host "Referenced: $($Referenced.Count)"
Write-Host "Unreferenced: $($Unreferenced.Count)"
Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host