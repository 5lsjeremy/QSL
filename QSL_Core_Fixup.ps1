# ============================================================
# QSL Core Stabilization Fixup
# ============================================================
#
# Run from:
#   Assets\QSL\QSL
#
# Script location:
#   Assets\QSL\QSL\QSL_Core_Fixup.ps1
#
# Scope:
#   Core
#   Abstractions
#
# This script is intentionally idempotent.
# Once the source tree is current, repeated runs should make
# no changes and should report no review items.
#
# ============================================================

$ErrorActionPreference = "Stop"

# ------------------------------------------------------------
# Resolve paths from the script location
# ------------------------------------------------------------

$QslRoot = $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($QslRoot))
{
    $QslRoot = (Get-Location).Path
}

$CoreRoot = Join-Path $QslRoot "Core"
$AbstractionsRoot = Join-Path $QslRoot "Abstractions"

if (-not (Test-Path -LiteralPath $CoreRoot -PathType Container))
{
    throw "Core directory not found: $CoreRoot"
}

if (-not (Test-Path -LiteralPath $AbstractionsRoot -PathType Container))
{
    throw "Abstractions directory not found: $AbstractionsRoot"
}

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupRoot = Join-Path $QslRoot "QSL_Fixup_Backup_$Timestamp"

New-Item `
    -ItemType Directory `
    -Path $BackupRoot `
    -Force | Out-Null

$ChangedCount = 0
$BackedUpCount = 0
$ProblemCount = 0

# ------------------------------------------------------------
# Console header
# ------------------------------------------------------------

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " QSL CORE STABILIZATION FIXUP" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "QSL root:"
Write-Host "  $QslRoot" -ForegroundColor Gray
Write-Host ""
Write-Host "Core:"
Write-Host "  $CoreRoot" -ForegroundColor Gray
Write-Host ""
Write-Host "Abstractions:"
Write-Host "  $AbstractionsRoot" -ForegroundColor Gray
Write-Host ""
Write-Host "Backup:"
Write-Host "  $BackupRoot" -ForegroundColor Gray
Write-Host ""

# ------------------------------------------------------------
# Helpers
# ------------------------------------------------------------

function Get-RequiredFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $FullPath = Join-Path $QslRoot $RelativePath

    if (-not (Test-Path -LiteralPath $FullPath -PathType Leaf))
    {
        throw "Required file not found: $RelativePath"
    }

    return $FullPath
}

function Get-RelativeQslPath
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$FullPath
    )

    return $FullPath.Substring($QslRoot.Length).TrimStart(
            [IO.Path]::DirectorySeparatorChar,
            [IO.Path]::AltDirectorySeparatorChar
    )
}

function Get-QslSourceFiles
{
    return @(
    Get-ChildItem `
            -LiteralPath $CoreRoot, $AbstractionsRoot `
            -Recurse `
            -File `
            -Filter "*.cs" |
            Sort-Object FullName
    )
}

function Backup-SourceFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$FullPath
    )

    $RelativePath = Get-RelativeQslPath $FullPath
    $BackupPath = Join-Path $BackupRoot $RelativePath
    $BackupDirectory = Split-Path -Parent $BackupPath

    if (-not (Test-Path -LiteralPath $BackupDirectory -PathType Container))
    {
        New-Item `
            -ItemType Directory `
            -Path $BackupDirectory `
            -Force | Out-Null
    }

    Copy-Item `
        -LiteralPath $FullPath `
        -Destination $BackupPath `
        -Force

    $script:BackedUpCount++
}

function Update-SourceText
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Transformation
    )

    $FullPath = Get-RequiredFile $RelativePath
    $ExistingContent = Get-Content -LiteralPath $FullPath -Raw

    if ($null -eq $ExistingContent)
    {
        $ExistingContent = ""
    }

    $UpdatedContent = & $Transformation $ExistingContent

    if ($null -eq $UpdatedContent)
    {
        throw "Transformation returned null for $RelativePath"
    }

    if ($ExistingContent -ceq $UpdatedContent)
    {
        Write-Host "[OK]      $RelativePath already current" -ForegroundColor DarkGreen
        return
    }

    Backup-SourceFile $FullPath

    $Utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)

    [IO.File]::WriteAllText(
            $FullPath,
            $UpdatedContent,
            $Utf8WithoutBom
    )

    $script:ChangedCount++
    Write-Host "[UPDATED] $RelativePath" -ForegroundColor Green
}

function Find-SourceMatches
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Pattern,

        [switch]$CaseSensitive
    )

    $Files = Get-QslSourceFiles

    if ($Files.Count -eq 0)
    {
        return @()
    }

    $Parameters = @{
        Path = $Files.FullName
        Pattern = $Pattern
    }

    if ($CaseSensitive)
    {
        $Parameters["CaseSensitive"] = $true
    }

    return @(Select-String @Parameters)
}

function Show-Verification
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Label,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [AllowEmptyCollection()]
        [object[]]$Matches
    )

    $MatchList = @($Matches)

    if ($MatchList.Count -eq 0)
    {
        Write-Host `
            "[PASS]   $Label" `
            -ForegroundColor Green

        return
    }

    $script:ProblemCount++

    Write-Host `
        "[REVIEW] $Label" `
        -ForegroundColor Yellow

    $MatchList |
            Select-Object -ExpandProperty Path -Unique |
            ForEach-Object {
                $RelativePath =
                Get-RelativeQslPath $_

                Write-Host "         $RelativePath"
            }
}

# ------------------------------------------------------------
# Normalize namespace casing
# ------------------------------------------------------------

foreach ($File in Get-QslSourceFiles)
{
    $RelativePath = Get-RelativeQslPath $File.FullName

    Update-SourceText `
        -RelativePath $RelativePath `
        -Transformation {
        param($Content)

        return $Content.Replace(
                "Qsl.Core.Base",
                "QSL.Core.Base"
        )
    }
}

# ------------------------------------------------------------
# Normalize duplicated Abstractions namespace
# ------------------------------------------------------------

foreach ($File in Get-QslSourceFiles)
{
    $RelativePath = Get-RelativeQslPath $File.FullName

    Update-SourceText `
        -RelativePath $RelativePath `
        -Transformation {
        param($Content)

        return $Content.Replace(
                "QSL.Abstractions.Base.QSL.Abstractions.Runtime",
                "QSL.Abstractions.Runtime"
        )
    }
}

# ------------------------------------------------------------
# Remove Unity dependency from DampenVelocityFunction
# ------------------------------------------------------------

$DampenPath = "Core\Runtime\Functions\DampenVelocityFunction.cs"

Update-SourceText `
    -RelativePath $DampenPath `
    -Transformation {
    param($Content)

    $Updated = [regex]::Replace(
            $Content,
            '(?m)^\s*using\s+UnityEngine\s*;\s*\r?\n',
            ''
    )

    $Updated = [regex]::Replace(
            $Updated,
            '(?ms)^\s*Debug\.Log\s*\(.*?\)\s*;\s*\r?\n',
            ''
    )

    return $Updated
}

# ------------------------------------------------------------
# Remove System.Numerics from QslLiteralEvaluator
# ------------------------------------------------------------

$LiteralEvaluatorPath = "Core\Runtime\Evaluators\QslLiteralEvaluator.cs"

Update-SourceText `
    -RelativePath $LiteralEvaluatorPath `
    -Transformation {
    param($Content)

    return [regex]::Replace(
            $Content,
            '(?m)^\s*using\s+System\.Numerics\s*;\s*\r?\n',
            ''
    )
}

# ------------------------------------------------------------
# Consolidate block evaluation through context.Evaluate
# ------------------------------------------------------------

$BlockEvaluatorPath = "Core\Runtime\Evaluators\QslBlockEvaluator.cs"

Update-SourceText `
    -RelativePath $BlockEvaluatorPath `
    -Transformation {
    param($Content)

    return $Content.Replace(
            "context.Registry.Evaluate(statement, context)",
            "context.Evaluate(statement)"
    )
}

# ------------------------------------------------------------
# Legacy wrapper deletion candidates
# ------------------------------------------------------------

$LegacyCandidates = @(
    "Core\Runtime\Operators\QslBlockOperator.cs",
    "Core\Runtime\Operators\QslDoubleQuoteOperator.cs",
    "Core\Runtime\Operators\QslEmbeddedExpressionOperator.cs",
    "Core\Runtime\Operators\QslGroupOperator.cs",
    "Core\Runtime\Operators\QslSingleQuoteOperator.cs",
    "Core\Runtime\Operators\QslNumericBinaryOperator.cs",
    "Core\Runtime\Operators\QslNumericOperator.cs",
    "Core\Runtime\Operators\QslNumericUnaryOperator.cs"
)

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " LEGACY DELETION CANDIDATES" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

foreach ($RelativePath in $LegacyCandidates)
{
    $FullPath = Join-Path $QslRoot $RelativePath

    if (-not (Test-Path -LiteralPath $FullPath -PathType Leaf))
    {
        Write-Host "[PASS]   .\$RelativePath removed" -ForegroundColor Green
        continue
    }

    $ClassName = [IO.Path]::GetFileNameWithoutExtension($RelativePath)
    $ReferencePattern = '\b{0}\b' -f [regex]::Escape($ClassName)

    $References = @(
    Get-QslSourceFiles |
            Where-Object {
                $_.FullName -ne $FullPath
            } |
            Select-String -Pattern $ReferencePattern
    )

    if ($References.Count -gt 0)
    {
        $ProblemCount++
        Write-Host "[REVIEW] $RelativePath still referenced" -ForegroundColor Yellow

        $References |
                Select-Object -ExpandProperty Path -Unique |
                ForEach-Object {
                    $ReferencePath = Get-RelativeQslPath $_
                    Write-Host "         $ReferencePath"
                }

        continue
    }

    Backup-SourceFile $FullPath
    Remove-Item -LiteralPath $FullPath -Force

    $MetaPath = "$FullPath.meta"

    if (Test-Path -LiteralPath $MetaPath -PathType Leaf)
    {
        Backup-SourceFile $MetaPath
        Remove-Item -LiteralPath $MetaPath -Force
    }

    $ChangedCount++
    Write-Host "[REMOVED] $RelativePath" -ForegroundColor Green
}

# ------------------------------------------------------------
# Post-fix verification
# ------------------------------------------------------------

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " POST-FIX VERIFICATION" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Case-sensitive by design:
# Qsl.Core.Base is invalid; QSL.Core.Base is valid.

$LowercaseBaseMatches = Find-SourceMatches `
    -Pattern '\bQsl\.Core\.Base\b' `
    -CaseSensitive

Show-Verification `
    -Label "Legacy lowercase Qsl.Core.Base" `
    -Matches $LowercaseBaseMatches

$DuplicatedAbstractionsMatches = Find-SourceMatches `
    -Pattern '\bQSL\.Abstractions\.Base\.QSL\.Abstractions\.Runtime\b' `
    -CaseSensitive

Show-Verification `
    -Label "Duplicated Abstractions namespace" `
    -Matches $DuplicatedAbstractionsMatches

$UnityMatches = Find-SourceMatches `
    -Pattern '\busing\s+UnityEngine\s*;|\bUnityEngine\.'

Show-Verification `
    -Label "UnityEngine references inside Core or Abstractions" `
    -Matches $UnityMatches

$NumericsMatches = Find-SourceMatches `
    -Pattern '\busing\s+System\.Numerics\s*;'

Show-Verification `
    -Label "System.Numerics references inside Core or Abstractions" `
    -Matches $NumericsMatches

$BlockRegistryEvaluationMatches = Find-SourceMatches `
    -Pattern '\bcontext\.Registry\.Evaluate\s*\('

Show-Verification `
    -Label "Legacy block direct registry evaluation" `
    -Matches $BlockRegistryEvaluationMatches

$IdentifierCastMatches = Find-SourceMatches `
    -Pattern '\(\s*QslIdentifierNode\s*\)\s*node'

Show-Verification `
    -Label "Legacy identifier direct cast" `
    -Matches $IdentifierCastMatches

$LiteralCastMatches = Find-SourceMatches `
    -Pattern '\(\s*QslLiteralNode\s*\)\s*node'

Show-Verification `
    -Label "Legacy literal direct cast" `
    -Matches $LiteralCastMatches

$RecordStateChangeMatches = Find-SourceMatches `
    -Pattern '\bRecordStateChange\s*\('

Show-Verification `
    -Label "Legacy RecordStateChange" `
    -Matches $RecordStateChangeMatches

$OnEntryMatches = Find-SourceMatches `
    -Pattern '\bOnEntry\s*\('

Show-Verification `
    -Label "Legacy OnEntry" `
    -Matches $OnEntryMatches

$RuntimeErrorsMatches = Find-SourceMatches `
    -Pattern '\bcontext\.Errors\b|\bQslRuntimeContext\b[\s\S]*\bErrors\s*\{'

Show-Verification `
    -Label "Legacy context runtime Errors" `
    -Matches $RuntimeErrorsMatches

# ------------------------------------------------------------
# Verify the three active arithmetic operators
# ------------------------------------------------------------

Write-Host ""
Write-Host "Arithmetic operator verification:" -ForegroundColor Cyan
Write-Host ""

$ArithmeticOperators = @(
    "Core\Runtime\Operators\QslMultiplyOperator.cs",
    "Core\Runtime\Operators\QslDivideOperator.cs",
    "Core\Runtime\Operators\QslModuloOperator.cs"
)

foreach ($RelativePath in $ArithmeticOperators)
{
    $Path = Get-RequiredFile $RelativePath
    $Content = Get-Content -LiteralPath $Path -Raw

    $UsesBase = $Content -match ':\s*QslOperatorBase'
    $HasApplyBinary = $Content -match '\bApplyBinary\s*\('
    $HasLegacyPrecedence = $Content -match '\bpublic\s+int\s+Precedence\s*\{'
    $HasLegacyAssociativity = $Content -match '\bpublic\s+bool\s+RightAssociative\s*\{'
    $HasLegacyEvaluate = $Content -match '\bobject\s+Evaluate\s*\('
    $HasRawOperandException = $Content -match '\bthrow\s+new\s+Exception\s*\('

    if (
    $UsesBase -and
            $HasApplyBinary -and
            -not $HasLegacyPrecedence -and
            -not $HasLegacyAssociativity -and
            -not $HasLegacyEvaluate -and
            -not $HasRawOperandException
    )
    {
        Write-Host "[PASS]   $RelativePath" -ForegroundColor Green
    }
    else
    {
        $ProblemCount++
        Write-Host "[REVIEW] $RelativePath" -ForegroundColor Yellow

        if (-not $UsesBase)
        {
            Write-Host "         Does not inherit QslOperatorBase" -ForegroundColor Yellow
        }

        if (-not $HasApplyBinary)
        {
            Write-Host "         ApplyBinary was not detected" -ForegroundColor Yellow
        }

        if ($HasLegacyPrecedence)
        {
            Write-Host "         Legacy Precedence property remains" -ForegroundColor Yellow
        }

        if ($HasLegacyAssociativity)
        {
            Write-Host "         Legacy RightAssociative property remains" -ForegroundColor Yellow
        }

        if ($HasLegacyEvaluate)
        {
            Write-Host "         Legacy Evaluate method remains" -ForegroundColor Yellow
        }

        if ($HasRawOperandException)
        {
            Write-Host "         Raw Exception throw remains" -ForegroundColor Yellow
        }
    }
}

# ------------------------------------------------------------
# Verify normalized Runtime contracts have Base visibility
# ------------------------------------------------------------

Write-Host ""
Write-Host "Abstractions contract verification:" -ForegroundColor Cyan
Write-Host ""

$RuntimeContracts = @(
    "Abstractions\Contracts\IQslBlock.cs",
    "Abstractions\Contracts\IQslBlockFactory.cs",
    "Abstractions\Contracts\IQslCapability.cs",
    "Abstractions\Contracts\IQslDescriptor.cs",
    "Abstractions\Contracts\IQslFunction.cs"
)

foreach ($RelativePath in $RuntimeContracts)
{
    $Path = Get-RequiredFile $RelativePath
    $Content = Get-Content -LiteralPath $Path -Raw

    $HasRuntimeNamespace = $Content.Contains(
            "namespace QSL.Abstractions.Runtime"
    )

    $NeedsBase =
    $Content.Contains("IQslObject") -or
            $Content.Contains("IQslRegistryItem")

    $HasBase = $Content.Contains(
            "using QSL.Abstractions.Base;"
    )

    if (
    $HasRuntimeNamespace -and
            (
            -not $NeedsBase -or
                    $HasBase
            )
    )
    {
        Write-Host "[PASS]   $RelativePath" -ForegroundColor Green
    }
    else
    {
        $ProblemCount++
        Write-Host "[REVIEW] $RelativePath" -ForegroundColor Yellow
    }
}

# ------------------------------------------------------------
# Verify legacy classes remain absent and unreferenced
# ------------------------------------------------------------

Write-Host ""
Write-Host "Legacy wrapper verification:" -ForegroundColor Cyan
Write-Host ""

foreach ($RelativePath in $LegacyCandidates)
{
    $FullPath = Join-Path $QslRoot $RelativePath
    $ClassName = [IO.Path]::GetFileNameWithoutExtension($RelativePath)
    $ReferencePattern = '\b{0}\b' -f [regex]::Escape($ClassName)

    if (Test-Path -LiteralPath $FullPath -PathType Leaf)
    {
        $ProblemCount++
        Write-Host "[REVIEW] .\$RelativePath still exists" -ForegroundColor Yellow
    }
    else
    {
        Write-Host "[PASS]   .\$RelativePath removed" -ForegroundColor Green
    }

    $References = @(
    Get-QslSourceFiles |
            Select-String -Pattern $ReferencePattern
    )

    if ($References.Count -eq 0)
    {
        Write-Host "[PASS]   No references to $ClassName" -ForegroundColor Green
    }
    else
    {
        $ProblemCount++
        Write-Host "[REVIEW] References remain for $ClassName" -ForegroundColor Yellow

        $References |
                Select-Object -ExpandProperty Path -Unique |
                ForEach-Object {
                    $ReferencePath = Get-RelativeQslPath $_
                    Write-Host "         $ReferencePath"
                }
    }
}

# ------------------------------------------------------------
# QslTokenKind legacy executor verification
# ------------------------------------------------------------

$TokenKindPath = Get-RequiredFile "Core\Lexer\QslTokenKind.cs"
$TokenKindContent = Get-Content -LiteralPath $TokenKindPath -Raw

Write-Host ""
Write-Host "Token metadata legacy verification:" -ForegroundColor Cyan
Write-Host ""

foreach ($RelativePath in $LegacyCandidates)
{
    $ClassName = [IO.Path]::GetFileNameWithoutExtension($RelativePath)
    $ReferencePattern = '\b{0}\b' -f [regex]::Escape($ClassName)

    if ($TokenKindContent -match $ReferencePattern)
    {
        $ProblemCount++
        Write-Host "[REVIEW] QslTokenKind references $ClassName" -ForegroundColor Yellow
    }
    else
    {
        Write-Host "[PASS]   QslTokenKind does not reference $ClassName" -ForegroundColor Green
    }
}

# ------------------------------------------------------------
# Remove empty backup folder when no files changed
# ------------------------------------------------------------

if (
$ChangedCount -eq 0 -and
        $BackedUpCount -eq 0 -and
        (Test-Path -LiteralPath $BackupRoot)
)
{
    $BackupChildren = @(
        Get-ChildItem -LiteralPath $BackupRoot -Force
    )

    if ($BackupChildren.Count -eq 0)
    {
        Remove-Item -LiteralPath $BackupRoot -Force
        $BackupRoot = "<none created>"
    }
}

# ------------------------------------------------------------
# Summary
# ------------------------------------------------------------

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " QSL CORE FIXUP COMPLETE" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Files changed: $ChangedCount"
Write-Host ""
Write-Host "Files backed up: $BackedUpCount"
Write-Host ""
Write-Host "Backup:"
Write-Host "  $BackupRoot" -ForegroundColor Gray
Write-Host ""

if ($ProblemCount -eq 0)
{
    Write-Host "Verification: PASS. No review items detected." -ForegroundColor Green
}
else
{
    Write-Host "Verification: $ProblemCount item(s) require review." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "NEXT:"
Write-Host "  1. Compile QSL / Unity."
Write-Host "  2. Run QSL_Audit.ps1 again."
Write-Host "  3. Review any compiler diagnostics."
Write-Host "  4. Begin the 200/200 stress suite."
Write-Host ""
