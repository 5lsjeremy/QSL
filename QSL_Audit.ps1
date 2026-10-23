# ============================================================
# QSL Core Source Audit Collector
# ============================================================
#
# Run from:
#   Assets\QSL\QSL\Core
#
# Produces:
#   QSL_Core_Audit.txt
#
# The report contains every C# source file beneath Core,
# excluding generated/build folders.
#
# ============================================================

$ErrorActionPreference = "Stop"

$Root = (Get-Location).Path
$OutputFile = Join-Path $Root "QSL_Core_Audit.txt"

$ExcludedDirectories = @(
    "Library",
    "Temp",
    "Obj",
    "obj",
    "Bin",
    "bin",
    "Logs",
    ".git",
    ".vs",
    ".idea"
)

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " QSL CORE AUDIT COLLECTOR" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Root: $Root"
Write-Host ""

# ------------------------------------------------------------
# Find all C# source files
# ------------------------------------------------------------

$Files = @(
Get-ChildItem `
        -LiteralPath $Root `
        -Recurse `
        -File `
        -Filter "*.cs" |
        Where-Object {

            $FullPath = $_.FullName
            $Excluded = $false

            foreach ($Directory in $ExcludedDirectories)
            {
                $Pattern =
                [IO.Path]::DirectorySeparatorChar +
                        $Directory +
                        [IO.Path]::DirectorySeparatorChar

                if ($FullPath.IndexOf(
                        $Pattern,
                        [StringComparison]::OrdinalIgnoreCase) -ge 0)
                {
                    $Excluded = $true
                    break
                }
            }

            -not $Excluded
        } |
        Sort-Object FullName
)

if ($Files.Count -eq 0)
{
    Write-Host "No C# files found." -ForegroundColor Yellow
    exit 1
}

# ------------------------------------------------------------
# Report builder
# ------------------------------------------------------------

$Builder = New-Object System.Text.StringBuilder

[void]$Builder.AppendLine(
        "============================================================"
)

[void]$Builder.AppendLine(
        "QSL CORE ARCHITECTURE AUDIT"
)

[void]$Builder.AppendLine(
        "============================================================"
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "ROOT: $Root"
)

[void]$Builder.AppendLine(
        "FILES: $($Files.Count)"
)

[void]$Builder.AppendLine("")

# ------------------------------------------------------------
# Certified architecture
# ------------------------------------------------------------

[void]$Builder.AppendLine(
        "CERTIFIED ARCHITECTURE - DO NOT REOPEN WITHOUT A REGRESSION:"
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "  - Provenance architecture"
)

[void]$Builder.AppendLine(
        "  - SubjectId identity"
)

[void]$Builder.AppendLine(
        "  - SubjectSlug heritage snapshots"
)

[void]$Builder.AppendLine(
        "  - Object-owned provenance"
)

[void]$Builder.AppendLine(
        "  - QslProvenanceBus.Publish entry retention and indexing"
)

[void]$Builder.AppendLine(
        "  - Global QslRuntime.Instance integration surface"
)

[void]$Builder.AppendLine(
        "  - Runtime Errors channel removal"
)

[void]$Builder.AppendLine(
        "  - QslParser.Errors as intentional parser diagnostics"
)

[void]$Builder.AppendLine(
        "  - Registry modernization"
)

[void]$Builder.AppendLine(
        "  - Typed QslNodeKind evaluator dispatch"
)

[void]$Builder.AppendLine(
        "  - Hardcoded RegisterDefaultOperators removal"
)

[void]$Builder.AppendLine(
        "  - QslEngine external API boundary"
)

[void]$Builder.AppendLine(
        "  - QslParseResult and QslProcessingResult"
)

[void]$Builder.AppendLine(
        "  - Assignment metadata and evaluator dispatch"
)

[void]$Builder.AppendLine(
        "  - Arrow metadata and ordered left-then-right evaluation"
)

[void]$Builder.AppendLine(
        "  - Current stable Unity driver integration"
)

[void]$Builder.AppendLine("")

# ------------------------------------------------------------
# Active audit goals
# ------------------------------------------------------------

[void]$Builder.AppendLine(
        "AUDIT GOALS:"
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "  - AST modernization and consistency"
)

[void]$Builder.AppendLine(
        "  - Legacy and dead operator wrapper detection"
)

[void]$Builder.AppendLine(
        "  - Namespace normalization"
)

[void]$Builder.AppendLine(
        "  - Unity and netstandard2.1 compatibility"
)

[void]$Builder.AppendLine(
        "  - Metadata-driven dispatch consistency"
)

[void]$Builder.AppendLine(
        "  - Assignment and Arrow execution verification"
)

[void]$Builder.AppendLine(
        "  - Silent failure detection"
)

[void]$Builder.AppendLine(
        "  - Duplicate execution-path detection"
)

[void]$Builder.AppendLine(
        "  - Dead or unreachable runtime path detection"
)

[void]$Builder.AppendLine(
        "  - Missing evaluator or executor detection"
)

[void]$Builder.AppendLine(
        "  - Metadata gaps or conflicting metadata detection"
)

[void]$Builder.AppendLine("")

# ------------------------------------------------------------
# Review contract
# ------------------------------------------------------------

[void]$Builder.AppendLine(
        "REVIEW CONTRACT:"
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "  STATUS: CORRECT"
)

[void]$Builder.AppendLine(
        "  or"
)

[void]$Builder.AppendLine(
        "  STATUS: UPDATE REQUIRED"
)

[void]$Builder.AppendLine(
        "  followed by the complete updated file."
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "  Certified architecture listed above is not to be"
)

[void]$Builder.AppendLine(
        "  redesigned unless the current source demonstrates"
)

[void]$Builder.AppendLine(
        "  an actual regression or implementation conflict."
)

[void]$Builder.AppendLine("")

# ------------------------------------------------------------
# File index
# ------------------------------------------------------------

[void]$Builder.AppendLine(
        "============================================================"
)

[void]$Builder.AppendLine(
        "FILE INDEX"
)

[void]$Builder.AppendLine(
        "============================================================"
)

[void]$Builder.AppendLine("")

$Index = 1

foreach ($File in $Files)
{
    $RelativePath = $File.FullName.Substring(
            $Root.Length
    ).TrimStart('\')

    [void]$Builder.AppendLine(
            ("{0:D3}  {1}" -f $Index, $RelativePath)
    )

    $Index++
}

[void]$Builder.AppendLine("")

# ------------------------------------------------------------
# Full source dump
# ------------------------------------------------------------

$Index = 1

foreach ($File in $Files)
{
    $RelativePath = $File.FullName.Substring(
            $Root.Length
    ).TrimStart('\')

    Write-Host (
    "[{0:D3}/{1:D3}] {2}" -f
    $Index,
    $Files.Count,
    $RelativePath
    ) -ForegroundColor Gray

    [void]$Builder.AppendLine(
            "============================================================"
    )

    [void]$Builder.AppendLine(
            ("FILE {0:D3}: {1}" -f $Index, $RelativePath)
    )

    [void]$Builder.AppendLine(
            "============================================================"
    )

    [void]$Builder.AppendLine(
            "STATUS: REVIEW"
    )

    [void]$Builder.AppendLine("")

    [void]$Builder.AppendLine(
            "FULL PATH:"
    )

    [void]$Builder.AppendLine(
            $File.FullName
    )

    [void]$Builder.AppendLine("")

    [void]$Builder.AppendLine(
            "SOURCE:"
    )

    [void]$Builder.AppendLine(
            "------------------------------------------------------------"
    )

    $Content = Get-Content `
        -LiteralPath $File.FullName `
        -Raw

    [void]$Builder.AppendLine(
            $Content
    )

    [void]$Builder.AppendLine(
            "------------------------------------------------------------"
    )

    [void]$Builder.AppendLine("")

    $Index++
}

# ------------------------------------------------------------
# Audit footer
# ------------------------------------------------------------

[void]$Builder.AppendLine(
        "============================================================"
)

[void]$Builder.AppendLine(
        "END OF QSL CORE ARCHITECTURE AUDIT"
)

[void]$Builder.AppendLine(
        "============================================================"
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "FILES COLLECTED: $($Files.Count)"
)

[void]$Builder.AppendLine("")

[void]$Builder.AppendLine(
        "NEXT PHASE AFTER AUDIT:"
)

[void]$Builder.AppendLine(
        "  - Apply only verified Core corrections"
)

[void]$Builder.AppendLine(
        "  - Complete namespace and Unity compatibility passes"
)

[void]$Builder.AppendLine(
        "  - Run the provenance-aware 200/200 QSL stress suite"
)

[void]$Builder.AppendLine(
        "  - Perform final silent-failure, ambiguity, evaluator,"
)

[void]$Builder.AppendLine(
        "    metadata, and determinism certification"
)

[void]$Builder.AppendLine("")

# ------------------------------------------------------------
# Write report
# ------------------------------------------------------------

[IO.File]::WriteAllText(
        $OutputFile,
        $Builder.ToString(),
        [Text.Encoding]::UTF8
)

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " QSL AUDIT COLLECTION COMPLETE" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Files collected: $($Files.Count)"
Write-Host ""
Write-Host "Report:"
Write-Host "  $OutputFile" -ForegroundColor Yellow
Write-Host ""
Write-Host "Send QSL_Core_Audit.txt to Mac for review." -ForegroundColor Cyan
Write-Host ""