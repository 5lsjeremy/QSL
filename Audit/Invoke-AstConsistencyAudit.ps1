param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root =
(Resolve-Path -LiteralPath $Root).Path

$AstPath =
Join-Path $Root "Core\AST"

$NodeKindPath =
Join-Path $AstPath "Types\QslNodeKind.cs"

$RegistryPath =
Join-Path $Root "Core\Runtime\QslRuntimeRegistry.cs"

$ReportsPath =
Join-Path $Root "Audit\Reports"

foreach ($Path in @(
    $AstPath,
    $NodeKindPath,
    $RegistryPath
)) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required path not found: $Path"
    }
}

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
        "QSL_AST_Consistency_$Timestamp.txt"

$LatestPath =
Join-Path `
        $ReportsPath `
        "QSL_AST_Consistency_Latest.txt"

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

# ============================================================
# NODE -> KIND
# ============================================================

$NodeMappings = @()

$NodeFiles = @(
Get-ChildItem `
        -LiteralPath $AstPath `
        -File `
        -Filter "Qsl*Node.cs" |
        Where-Object {
            $_.Name -ne "QslNode.cs"
        } |
        Sort-Object Name
)

foreach ($File in $NodeFiles) {

    $KindMatches = @(
        Select-String `
            -LiteralPath $File.FullName `
            -Pattern 'QslNodeKind\.(\w+)' `
            -AllMatches
    )

    $Kinds = @()

    foreach ($LineMatch in $KindMatches) {
        foreach ($Match in $LineMatch.Matches) {
            $Kinds += $Match.Groups[1].Value
        }
    }

    $Kinds = @(
    $Kinds |
            Sort-Object -Unique
    )

    $NodeMappings +=
    [pscustomobject]@{
        Node =
        $File.BaseName

        KindCount =
        $Kinds.Count

        Kind =
        if ($Kinds.Count -eq 1) {
            $Kinds[0]
        }
        elseif ($Kinds.Count -eq 0) {
            "<missing>"
        }
        else {
            $Kinds -join ","
        }
    }
}

# ============================================================
# ENUM INVENTORY
# ============================================================

$EnumKinds = @()

$InsideEnum = $false

foreach ($Line in Get-Content -LiteralPath $NodeKindPath) {

    if ($Line -match 'public enum QslNodeKind') {
        $InsideEnum = $true
        continue
    }

    if (-not $InsideEnum) {
        continue
    }

    if ($Line -match '}') {
        break
    }

    $Clean =
    $Line.Trim().TrimEnd(',')

    if ($Clean -match '^[A-Za-z_][A-Za-z0-9_]*$') {
        $EnumKinds += $Clean
    }
}

$EnumKinds = @(
$EnumKinds |
        Sort-Object -Unique
)

# ============================================================
# REGISTRY MAPPINGS
# ============================================================

$RegistryLines =
Get-Content -LiteralPath $RegistryPath

$RegistryMappings = @()

for (
        $Index = 0;
        $Index -lt $RegistryLines.Count;
        $Index++
) {
    $Line =
    $RegistryLines[$Index]

    if ($Line -notmatch 'QslNodeKind\.(\w+)') {
        continue
    }

    $Kind =
    $Matches[1]

    $WindowEnd =
    [Math]::Min(
            $Index + 3,
            $RegistryLines.Count - 1
    )

    $Window =
    $RegistryLines[
    $Index..$WindowEnd
    ] -join " "

    if ($Window -match 'new\s+(Qsl\w+Evaluator)\s*\(') {

        $RegistryMappings +=
        [pscustomobject]@{
            Kind =
            $Kind

            Evaluator =
            $Matches[1]
        }
    }
}

# ============================================================
# CONSISTENCY MATRIX
# ============================================================

$Rows = @()

foreach ($Kind in $EnumKinds) {

    $Nodes = @(
    $NodeMappings |
            Where-Object {
                $_.Kind -eq $Kind -and
                        $_.KindCount -eq 1
            }
    )

    $Registrations = @(
    $RegistryMappings |
            Where-Object {
                $_.Kind -eq $Kind
            }
    )

    $NodeName =
    if ($Nodes.Count -eq 1) {
        $Nodes[0].Node
    }
    elseif ($Nodes.Count -eq 0) {
        "<missing>"
    }
    else {
        "<multiple>"
    }

    $EvaluatorName =
    if ($Registrations.Count -eq 1) {
        $Registrations[0].Evaluator
    }
    elseif ($Registrations.Count -eq 0) {
        "<missing>"
    }
    else {
        "<multiple>"
    }

    $ExpectedNode =
    "Qsl" +
            $Kind +
            "Node"

    $ExpectedEvaluator =
    "Qsl" +
            $Kind +
            "Evaluator"

    $Status =
    if (
    $Nodes.Count -eq 1 -and
            $Registrations.Count -eq 1 -and
            $NodeName -eq $ExpectedNode -and
            $EvaluatorName -eq $ExpectedEvaluator
    ) {
        "CONSISTENT"
    }
    else {
        "REVIEW"
    }

    $Rows +=
    [pscustomobject]@{
        Kind =
        $Kind

        Node =
        $NodeName

        Evaluator =
        $EvaluatorName

        ExpectedNode =
        $ExpectedNode

        ExpectedEvaluator =
        $ExpectedEvaluator

        Status =
        $Status
    }
}

# ============================================================
# EXTRA / DUPLICATE DETECTION
# ============================================================

$Problems = @()

foreach ($Node in $NodeMappings) {

    if ($Node.KindCount -ne 1) {
        $Problems +=
        "Node $($Node.Node) declares $($Node.KindCount) kinds."
        continue
    }

    if ($EnumKinds -notcontains $Node.Kind) {
        $Problems +=
        "Node $($Node.Node) references unknown kind $($Node.Kind)."
    }
}

foreach ($Kind in $EnumKinds) {

    $NodeCount =
    @(
    $NodeMappings |
            Where-Object {
                $_.Kind -eq $Kind
            }
    ).Count

    $RegistrationCount =
    @(
    $RegistryMappings |
            Where-Object {
                $_.Kind -eq $Kind
            }
    ).Count

    if ($NodeCount -ne 1) {
        $Problems +=
        "QslNodeKind.$Kind has $NodeCount concrete node mappings."
    }

    if ($RegistrationCount -ne 1) {
        $Problems +=
        "QslNodeKind.$Kind has $RegistrationCount evaluator registrations."
    }
}

$ReviewRows =
@(
$Rows |
        Where-Object {
            $_.Status -ne "CONSISTENT"
        }
)

# ============================================================
# REPORT
# ============================================================

Add-Line "============================================================"
Add-Line "QSL AST CONSISTENCY AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"

Add-Header "NODE -> KIND"

foreach ($Node in $NodeMappings) {
    Add-Line (
    $Node.Node.PadRight(32) +
            " -> " +
            $Node.Kind
    )
}

Add-Header "KIND -> EVALUATOR"

foreach ($Mapping in $RegistryMappings) {
    Add-Line (
    $Mapping.Kind.PadRight(24) +
            " -> " +
            $Mapping.Evaluator
    )
}

Add-Header "CONSISTENCY MATRIX"

foreach ($Row in $Rows) {

    Add-Line (
    $Row.Kind.PadRight(20) +
            $Row.Node.PadRight(34) +
            $Row.Evaluator.PadRight(40) +
            $Row.Status
    )
}

Add-Header "PROBLEMS"

if (
$Problems.Count -eq 0 -and
        $ReviewRows.Count -eq 0
) {
    Add-Line "<none>"
}
else {

    foreach ($Problem in $Problems) {
        Add-Line $Problem
    }

    foreach ($Row in $ReviewRows) {
        Add-Line (
        "Dispatch review required: " +
                $Row.Kind +
                " | Node=" +
                $Row.Node +
                " | Evaluator=" +
                $Row.Evaluator
        )
    }
}

Add-Header "SUMMARY"

Add-Line "Enum kinds: $($EnumKinds.Count)"
Add-Line "Concrete nodes: $($NodeMappings.Count)"
Add-Line "Registry mappings: $($RegistryMappings.Count)"
Add-Line "Consistent rows: $($Rows.Count - $ReviewRows.Count)"
Add-Line "Review rows: $($ReviewRows.Count)"
Add-Line "Problems: $($Problems.Count)"
Add-Line

if (
$Problems.Count -eq 0 -and
        $ReviewRows.Count -eq 0
) {
    Add-Line "STATUS: AST DISPATCH CONSISTENT"
}
else {
    Add-Line "STATUS: REVIEW REQUIRED"
}

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
Write-Host "QSL AST CONSISTENCY AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "Enum kinds: $($EnumKinds.Count)"
Write-Host "Concrete nodes: $($NodeMappings.Count)"
Write-Host "Registry mappings: $($RegistryMappings.Count)"
Write-Host "Consistent rows: $($Rows.Count - $ReviewRows.Count)"
Write-Host "Review rows: $($ReviewRows.Count)"
Write-Host "Problems: $($Problems.Count)"
Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host

if (
$Problems.Count -gt 0 -or
        $ReviewRows.Count -gt 0
) {
    exit 1
}

exit 0