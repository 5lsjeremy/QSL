param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root = (Resolve-Path -LiteralPath $Root).Path

$AstPath = Join-Path $Root "Core\AST"
$ParserPath = Join-Path $Root "Core\Parser\QslParser.cs"
$EvaluatorPath = Join-Path $Root "Core\Runtime\Evaluators"
$RegistryPath = Join-Path $Root "Core\Runtime\QslRuntimeRegistry.cs"
$ReportsPath = Join-Path $Root "Audit\Reports"

if (-not (Test-Path -LiteralPath $AstPath)) {
    throw "AST directory not found: $AstPath"
}

New-Item `
    -ItemType Directory `
    -Path $ReportsPath `
    -Force |
        Out-Null

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$ReportPath = Join-Path `
    $ReportsPath `
    "QSL_AST_Audit_$Timestamp.txt"

$LatestPath = Join-Path `
    $ReportsPath `
    "QSL_AST_Audit_Latest.txt"

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

$AstFiles = @(
Get-ChildItem `
        -LiteralPath $AstPath `
        -File `
        -Filter "*.cs" |
        Sort-Object Name
)

# ============================================================
# DISCOVER CONCRETE AST NODE TYPES
# ============================================================

$NodeNames = @()

foreach ($File in $AstFiles) {
    $Content = Get-Content `
        -LiteralPath $File.FullName `
        -Raw

    $Matches = [regex]::Matches(
            $Content,
            'public\s+sealed\s+class\s+(Qsl\w+Node)\b'
    )

    foreach ($Match in $Matches) {
        $NodeNames += $Match.Groups[1].Value
    }
}

$NodeNames = @(
$NodeNames |
        Sort-Object -Unique
)

# ============================================================
# REPORT HELPERS
# ============================================================

$Report = New-Object System.Collections.Generic.List[string]

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

function Get-Relative {
    param(
        [string]$Path
    )

    return $Path.Substring(
            $Root.Length
    ).TrimStart('\')
}

function Find-SourceReference {
    param(
        [string]$Text
    )

    if ($SourcePaths.Count -eq 0) {
        return @()
    }

    return @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Text
    )
}

# ============================================================
# HEADER
# ============================================================

Add-Line "============================================================"
Add-Line "QSL AST AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"
Add-Line "C# source files scanned: $($SourceFiles.Count)"
Add-Line "Concrete AST nodes discovered: $($NodeNames.Count)"

# ============================================================
# AST INVENTORY
# ============================================================

Add-Header "AST FILE INVENTORY"

foreach ($File in $AstFiles) {
    Add-Line (Get-Relative $File.FullName)
}

Add-Header "CONCRETE AST NODE INVENTORY"

foreach ($NodeName in $NodeNames) {
    Add-Line $NodeName
}

# ============================================================
# NODE ANALYSIS
# ============================================================

Add-Header "NODE ANALYSIS"

$OrphanCandidates = @()

foreach ($NodeName in $NodeNames) {
    Add-Line
    Add-Line "------------------------------------------------------------"
    Add-Line "NODE: $NodeName"
    Add-Line "------------------------------------------------------------"

    $Definition = $AstFiles |
            Where-Object {
                $_.BaseName -eq $NodeName
            } |
            Select-Object -First 1

    if ($null -ne $Definition) {
        Add-Line "Definition: $(Get-Relative $Definition.FullName)"
    }
    else {
        Add-Line "Definition: <not found>"
    }

    $References = @(
    Find-SourceReference $NodeName |
            Where-Object {
                $null -eq $Definition -or
                        $_.Path -ne $Definition.FullName
            }
    )

    $ParserReferences = @(
    $References |
            Where-Object {
                $_.Path -match '\\Parser\\'
            }
    )

    $EvaluatorReferences = @(
    $References |
            Where-Object {
                $_.Path -match '\\Evaluators\\'
            }
    )

    $OtherReferences = @(
    $References |
            Where-Object {
                $_.Path -notmatch '\\Parser\\' -and
                        $_.Path -notmatch '\\Evaluators\\'
            }
    )

    Add-Line "Parser references: $($ParserReferences.Count)"

    foreach ($Reference in $ParserReferences) {
        Add-Line (
        "  " +
                (Get-Relative $Reference.Path) +
                ":" +
                $Reference.LineNumber +
                "  " +
                $Reference.Line.Trim()
        )
    }

    Add-Line "Evaluator references: $($EvaluatorReferences.Count)"

    foreach ($Reference in $EvaluatorReferences) {
        Add-Line (
        "  " +
                (Get-Relative $Reference.Path) +
                ":" +
                $Reference.LineNumber +
                "  " +
                $Reference.Line.Trim()
        )
    }

    Add-Line "Other references: $($OtherReferences.Count)"

    foreach ($Reference in $OtherReferences) {
        Add-Line (
        "  " +
                (Get-Relative $Reference.Path) +
                ":" +
                $Reference.LineNumber +
                "  " +
                $Reference.Line.Trim()
        )
    }

    if ($References.Count -eq 0) {
        Add-Line "STATUS: POSSIBLY ORPHANED"
        $OrphanCandidates += $NodeName
    }
    else {
        Add-Line "STATUS: REFERENCED"
    }
}

# ============================================================
# NODE KIND
# ============================================================

Add-Header "QSL NODE KIND"

$NodeKindPath = Join-Path `
    $AstPath `
    "Types\QslNodeKind.cs"

if (Test-Path -LiteralPath $NodeKindPath) {
    Get-Content -LiteralPath $NodeKindPath |
            ForEach-Object {
                Add-Line $_
            }
}
else {
    Add-Line "<QslNodeKind.cs not found>"
}

# ============================================================
# PARSER CONSTRUCTION
# ============================================================

Add-Header "PARSER AST CONSTRUCTION"

if (Test-Path -LiteralPath $ParserPath) {
    $Matches = @(
        Select-String `
            -LiteralPath $ParserPath `
            -Pattern 'new\s+Qsl\w+Node\s*\('
    )

    foreach ($Match in $Matches) {
        Add-Line (
        $Match.LineNumber.ToString() +
                ": " +
                $Match.Line.Trim()
        )
    }
}

# ============================================================
# EVALUATOR CONSUMPTION
# ============================================================

Add-Header "EVALUATOR AST REFERENCES"

if (Test-Path -LiteralPath $EvaluatorPath) {
    $EvaluatorFiles = @(
    Get-ChildItem `
            -LiteralPath $EvaluatorPath `
            -File `
            -Filter "*.cs" |
            Sort-Object Name
    )

    foreach ($File in $EvaluatorFiles) {
        $Matches = @(
            Select-String `
                -LiteralPath $File.FullName `
                -Pattern '\bQsl\w+Node\b'
        )

        if ($Matches.Count -eq 0) {
            continue
        }

        Add-Line
        Add-Line "FILE: $($File.Name)"

        foreach ($Match in $Matches) {
            Add-Line (
            "  " +
                    $Match.LineNumber +
                    ": " +
                    $Match.Line.Trim()
            )
        }
    }
}

# ============================================================
# EVALUATOR REGISTRATION
# ======================================================Add-Header "EVALUATOR REGISTRATION"

if (Test-Path -LiteralPath $RegistryPath) {
    $Matches = @(
        Select-String `
            -LiteralPath $RegistryPath `
            -Pattern 'RegisterEvaluator|QslNodeKind\.'
    )

    foreach ($Match in $Matches) {
        Add-Line (
        $Match.LineNumber.ToString() +
                ": " +
                $Match.Line.Trim()
        )
    }
}

# ============================================================
# EMPTY AST DIRECTORIES
# ============================================================

Add-Header "EMPTY AST DIRECTORIES"

$EmptyDirectories = @()

$AstDirectories = @(
    Get-ChildItem `
        -LiteralPath $AstPath `
        -Directory `
        -Recurse
)

foreach ($Directory in $AstDirectories) {
    $Children = @(
        Get-ChildItem `
            -LiteralPath $Directory.FullName `
            -Force
    )

    if ($Children.Count -eq 0) {
        $EmptyDirectories += $Directory
        Add-Line "EMPTY: $(Get-Relative $Directory.FullName)"
    }
}

if ($EmptyDirectories.Count -eq 0) {
    Add-Line "<none>"
}

# ============================================================
# LEGACY MARKERS
# ============================================================

Add-Header "LEGACY / TODO MARKERS"

$LegacyMatches = @()

if ($SourcePaths.Count -gt 0) {
    $LegacyMatches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -Pattern `
                '\bTODO\b',
        '\bFIXME\b',
        '\bHACK\b',
        '\bOBSOLETE\b',
        '\blegacy\b' `
            -CaseSensitive:$false
    )
}

if ($LegacyMatches.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Match in $LegacyMatches) {
        Add-Line (
        (Get-Relative $Match.Path) +
                ":" +
                $Match.LineNumber +
                "  " +
                $Match.Line.Trim()
        )
    }
}

# ============================================================
# SUMMARY
# ============================================================

Add-Header "SUMMARY"

Add-Line "C# source files scanned: $($SourceFiles.Count)"
Add-Line "Concrete AST nodes: $($NodeNames.Count)"
Add-Line "Possible orphan candidates: $($OrphanCandidates.Count)"
Add-Line "Empty AST directories: $($EmptyDirectories.Count)"

if ($OrphanCandidates.Count -gt 0) {
    Add-Line
    Add-Line "Possible orphan candidates:"

    foreach ($Node in $OrphanCandidates) {
        Add-Line "  $Node"
    }
}

if ($EmptyDirectories.Count -gt 0) {
    Add-Line
    Add-Line "Empty directories:"

    foreach ($Directory in $EmptyDirectories) {
        Add-Line "  $(Get-Relative $Directory.FullName)"
    }
}

Add-Line
Add-Line "NOTE:"
Add-Line "Reference counts identify audit candidates only."
Add-Line "They do not prove that a node is valid, invalid, used, or unused."
Add-Line
Add-Line "No QSL source files were modified by this audit."

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
Write-Host "QSL AST AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "Report:"
Write-Host "  $ReportPath"
Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host
Write-Host "C# source files scanned: $($SourceFiles.Count)"
Write-Host "Concrete AST nodes: $($NodeNames.Count)"
Write-Host "Possible orphan candidates: $($OrphanCandidates.Count)"
Write-Host "Empty AST directories: $($EmptyDirectories.Count)"
Write-Host