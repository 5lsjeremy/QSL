param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root =
(Resolve-Path -LiteralPath $Root).Path

$ReportsPath =
Join-Path `
        $Root `
        "Audit\Reports"

New-Item `
    -ItemType Directory `
    -Path $ReportsPath `
    -Force |
        Out-Null

$Timestamp =
Get-Date -Format "yyyyMMdd_HHmmss"

$ReportPath =
Join-Path `
        $ReportsPath `
        "QSL_Namespace_Audit_$Timestamp.txt"

$LatestPath =
Join-Path `
        $ReportsPath `
        "QSL_Namespace_Audit_Latest.txt"

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

$Report =
New-Object `
        System.Collections.Generic.List[string]

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

$Records = @()

foreach ($File in $SourceFiles) {

    $NamespaceMatch =
    Select-String `
            -LiteralPath $File.FullName `
            -Pattern '^\s*namespace\s+([A-Za-z0-9_.]+)' |
            Select-Object -First 1

    $Namespace = "<global>"

    if ($null -ne $NamespaceMatch) {

        if (
        $NamespaceMatch.Line -match
                '^\s*namespace\s+([A-Za-z0-9_.]+)'
        ) {
            $Namespace =
            $Matches[1]
        }
    }

    $PublicTypes = @(
        Select-String `
            -LiteralPath $File.FullName `
            -Pattern `
                '^\s*public\s+(?:(?:abstract|sealed|static|partial)\s+)*(?:class|interface|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)'
    )

    $TypeNames = @()

    foreach ($Match in $PublicTypes) {

        if (
        $Match.Line -match
                '^\s*public\s+(?:(?:abstract|sealed|static|partial)\s+)*(?:class|interface|struct|enum)\s+([A-Za-z_][A-Za-z0-9_]*)'
        ) {
            $TypeNames +=
            $Matches[1]
        }
    }

    $Records +=
    [pscustomobject]@{
        Path =
        Get-RelativePath $File.FullName

        Namespace =
        $Namespace

        PublicTypes =
        $TypeNames
    }
}

$GlobalRecords = @(
$Records |
        Where-Object {
            $_.Namespace -eq "<global>"
        }
)

$AbstractionRecords = @(
$Records |
        Where-Object {
            $_.Path -like "Abstractions\*"
        }
)

$CoreBaseRecords = @(
$Records |
        Where-Object {
            $_.Path -like "Core\Base\*"
        }
)

Add-Line "============================================================"
Add-Line "QSL NAMESPACE AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"
Add-Line "C# files scanned: $($SourceFiles.Count)"
Add-Line "Global-namespace files: $($GlobalRecords.Count)"

Add-Header "COMPLETE NAMESPACE INVENTORY"

foreach ($Record in $Records) {

    $Types =
    if ($Record.PublicTypes.Count -eq 0) {
        "<none>"
    }
    else {
        $Record.PublicTypes -join ", "
    }

    Add-Line "FILE: $($Record.Path)"
    Add-Line "  Namespace: $($Record.Namespace)"
    Add-Line "  Public types: $Types"
}

Add-Header "GLOBAL NAMESPACE FILES"

if ($GlobalRecords.Count -eq 0) {

    Add-Line "<none>"
}
else {

    foreach ($Record in $GlobalRecords) {

        $Types =
        if ($Record.PublicTypes.Count -eq 0) {
            "<none>"
        }
        else {
            $Record.PublicTypes -join ", "
        }

        Add-Line (
        $Record.Path +
                "  [" +
                $Types +
                "]"
        )
    }
}

Add-Header "ABSTRACTIONS"

foreach ($Record in $AbstractionRecords) {

    Add-Line "FILE: $($Record.Path)"
    Add-Line "  Namespace: $($Record.Namespace)"

    if ($Record.PublicTypes.Count -gt 0) {
        Add-Line (
        "  Public: " +
                ($Record.PublicTypes -join ", ")
        )
    }
}

Add-Header "CORE BASE"

foreach ($Record in $CoreBaseRecords) {

    Add-Line "FILE: $($Record.Path)"
    Add-Line "  Namespace: $($Record.Namespace)"

    if ($Record.PublicTypes.Count -gt 0) {
        Add-Line (
        "  Public: " +
                ($Record.PublicTypes -join ", ")
        )
    }
}

Add-Header "DISTINCT NAMESPACES"

$DistinctNamespaces =
@(
$Records.Namespace |
        Sort-Object -Unique
)

foreach ($Namespace in $DistinctNamespaces) {

    $Count =
    @(
    $Records |
            Where-Object {
                $_.Namespace -eq $Namespace
            }
    ).Count

    Add-Line (
    $Namespace.PadRight(48) +
            $Count
    )
}

Add-Header "SUMMARY"

Add-Line "C# files: $($SourceFiles.Count)"
Add-Line "Distinct namespaces: $($DistinctNamespaces.Count)"
Add-Line "Global namespace files: $($GlobalRecords.Count)"
Add-Line "Abstractions files: $($AbstractionRecords.Count)"
Add-Line "Core\Base files: $($CoreBaseRecords.Count)"
Add-Line
Add-Line "STATUS: NAMESPACE REVIEW READY"
Add-Line
Add-Line "No QSL source files were modified."

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
Write-Host "QSL NAMESPACE AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "C# files: $($SourceFiles.Count)"
Write-Host "Distinct namespaces: $($DistinctNamespaces.Count)"
Write-Host "Global namespace files: $($GlobalRecords.Count)"
Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host