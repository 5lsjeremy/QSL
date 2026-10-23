# ============================================================
# QSL Core Stabilization Fix Pass
# ============================================================
#
# Run from:
#   Assets\QSL\QSL
#
# Expected folders:
#   .\Core
#   .\Abstractions
#
# Performs:
#   1. Modernize QslMultiplyOperator
#   2. Modernize QslDivideOperator
#   3. Modernize QslModuloOperator
#   4. Remove UnityEngine dependency from DampenVelocityFunction
#   5. Remove System.Numerics from QslLiteralEvaluator
#   6. Normalize Qsl.Core.Base -> QSL.Core.Base
#   7. Normalize duplicated abstraction namespace
#   8. Route block evaluation through context.Evaluate(...)
#   9. Consolidate identifier resolution through the context
#  10. Add guarded casts to identifier/literal evaluators
#
# A timestamped backup is created before modification.
#
# ============================================================

$ErrorActionPreference = "Stop"

$Root = (Get-Location).Path

$Core = Join-Path $Root "Core"
$Abstractions = Join-Path $Root "Abstractions"

if (-not (Test-Path -LiteralPath $Core -PathType Container))
{
    throw "Core directory not found: $Core"
}

if (-not (Test-Path -LiteralPath $Abstractions -PathType Container))
{
    throw "Abstractions directory not found: $Abstractions"
}

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$BackupRoot = Join-Path $Root "QSL_Backup_$Timestamp"

New-Item `
    -ItemType Directory `
    -Path $BackupRoot `
    -Force |
        Out-Null

$Utf8 = New-Object System.Text.UTF8Encoding($false)

# ------------------------------------------------------------
# Helpers
# ------------------------------------------------------------

function Backup-File
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf))
    {
        throw "Required file not found: $Path"
    }

    $Relative = $Path.Substring($Root.Length).TrimStart('\')
    $Destination = Join-Path $BackupRoot $Relative
    $DestinationDirectory = Split-Path -Parent $Destination

    if (-not (Test-Path -LiteralPath $DestinationDirectory))
    {
        New-Item `
            -ItemType Directory `
            -Path $DestinationDirectory `
            -Force |
                Out-Null
    }

    if (-not (Test-Path -LiteralPath $Destination))
    {
        Copy-Item `
            -LiteralPath $Path `
            -Destination $Destination
    }
}

function Write-QslFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    Backup-File $Path

    [IO.File]::WriteAllText(
            $Path,
            $Content.TrimStart("`r", "`n") + [Environment]::
            $Utf8
    )

    $Relative = $Path.Substring($Root.Length).TrimStart('\')

    Write-Host "UPDATED  $Relative" -ForegroundColor Green
}

function Update-TextFile
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [string]$OldText,

        [Parameter(Mandatory = $true)]
        [string]$NewText
    )

    $Content = Get-Content `
        -LiteralPath $Path `
        -Raw

    if ($Content -notlike "*$OldText*")
    {
        return $false
    }

    Backup-File $Path

    $Content = $Content.Replace(
            $OldText,
            $NewText
    )

    [IO.File]::WriteAllText(
            $Path,
            $Content,
            $Utf8
    )

    return $true
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " QSL CORE STABILIZATION FIX PASS" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Root:"
Write-Host "  $Root"
Write-Host ""
Write-Host "Backup:"
Write-Host "  $BackupRoot" -ForegroundColor Yellow
Write-Host ""

# ============================================================
# 1. QslMultiplyOperator
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Operators\QslMultiplyOperator.cs"

$Content = @'
using QSL.Abstractions.Runtime;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslMultiplyOperator : QslOperatorBase
    {
        public override string Name => "*";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (left is double leftNumber &&
                right is double rightNumber)
            {
                return leftNumber * rightNumber;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '*' requires numeric operands of compatible types."
            );

            return null;
        }
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 2. QslDivideOperator
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Operators\QslDivideOperator.cs"

$Content = @'
using QSL.Abstractions.Runtime;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslDivideOperator : QslOperatorBase
    {
        public override string Name => "/";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (left is double leftNumber &&
                right is double rightNumber)
            {
                if (rightNumber == 0.0)
                {
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        Name,
                        "Division by zero is not allowed."
                    );

                    return null;
                }

                return leftNumber / rightNumber;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '/' requires numeric operands of compatible types."
            );

            return null;
        }
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 3. QslModuloOperator
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Operators\QslModuloOperator.cs"

$Content = @'
using QSL.Abstractions.Runtime;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Operators
{
    public sealed class QslModuloOperator : QslOperatorBase
    {
        public override string Name => "%";

        public override object? ApplyBinary(
            object? left,
            object? right,
            QslRuntimeContext context)
        {
            if (left is double leftNumber &&
                right is double rightNumber)
            {
                if (rightNumber == 0.0)
                {
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        Name,
                        "Modulo by zero is not allowed."
                    );

                    return null;
                }

                return leftNumber % rightNumber;
            }

            AddProvenance(
                QslProvenanceType.Error,
                QslProvenanceCategory.Evaluation,
                Name,
                "Binary '%' requires numeric operands of compatible types."
            );

            return null;
        }
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 4. Remove UnityEngine from DampenVelocityFunction
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Functions\DampenVelocityFunction.cs"

$Content = @'
using QSL.Abstractions.Runtime;
using QSL.Core.Base;
using QSL.Core.Provenance;

public sealed class DampenVelocityFunction :
    QslRegistryItem,
    IQslFunction
{
    public override string Type => "function";

    public override string Name => "DampenVelocity";

    public object? Invoke(
        object?[] args,
        QslRuntimeContext context)
    {
        if (args.Length == 2 &&
            args[0] is Velocity velocity &&
            args[1] is double factor)
        {
            var dampened =
                velocity.Scale(factor);

            context.SetVariable(
                "LastDampenedVelocity",
                dampened
            );

            AddProvenance(
                QslProvenanceType.Info,
                QslProvenanceCategory.Capability,
                Name,
                $"Dampened velocity {velocity} by factor {factor}"
            );

            return dampened;
        }

        AddProvenance(
            QslProvenanceType.Error,
            QslProvenanceCategory.Capability,
            Name,
            "DampenVelocity requires (Velocity, double) arguments."
        );

        return null;
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 5 + 10. QslLiteralEvaluator
#
# - Remove System.Numerics
# - Add guarded node conversion
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Evaluators\QslLiteralEvaluator.cs"

$Content = @'
using System;
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Ast.Types;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslLiteralEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "literal";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslLiteralNode literal)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Literal evaluator received an incompatible node."
                );

                return null;
            }

            return literal.LiteralKind switch
            {
                QslLiteralKind.Number =>
                    Convert.ToDouble(literal.Value),

                QslLiteralKind.String =>
                    literal.Value?.ToString(),

                QslLiteralKind.Boolean =>
                    Convert.ToBoolean(literal.Value),

                QslLiteralKind.Null =>
                    null,

                QslLiteralKind.Vector =>
                    (Vector)literal.Value!,

                QslLiteralKind.Velocity =>
                    (Velocity)literal.Value!,

                _ =>
                    literal.Value
            };
        }
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 8. QslBlockEvaluator
#
# Canonical nested evaluation goes through context.Evaluate().
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Evaluators\QslBlockEvaluator.cs"

$Content = @'
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslBlockEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "Block";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslBlockNode block)
            {
                const string message =
                    "QslBlockEvaluator received a non-block node.";

                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    message
                );

                return new QslErrorNode(message);
            }

            object? lastValue = null;

            foreach (var statement in block.Statements)
            {
                if (statement is QslErrorNode error)
                {
                    AddProvenance(
                        QslProvenanceType.Error,
                        QslProvenanceCategory.Evaluation,
                        Name,
                        error.Message
                    );

                    lastValue = error;
                    continue;
                }

                lastValue =
                    context.Evaluate(statement);
            }

            return lastValue;
        }
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 9 + 10. QslIdentifierEvaluator
#
# Identifier lookup now has one canonical runtime path:
# QslRuntimeContext.ResolveIdentifier(...)
#
# Also adds guarded node conversion.
# ============================================================

$Path = Join-Path `
    $Core `
    "Runtime\Evaluators\QslIdentifierEvaluator.cs"

$Content = @'
using QSL.Abstractions.Base;
using QSL.Abstractions.Runtime;
using QSL.Core.Ast;
using QSL.Core.Base;
using QSL.Core.Provenance;

namespace QSL.Core.Runtime.Evaluators
{
    public sealed class QslIdentifierEvaluator :
        QslRegistryItem,
        IQslEvaluator
    {
        public override string Type => "evaluator";

        public override string Name => "identifier";

        public object? Evaluate(
            IQslNode node,
            QslRuntimeContext context)
        {
            if (node is not QslIdentifierNode identifier)
            {
                AddProvenance(
                    QslProvenanceType.Error,
                    QslProvenanceCategory.Evaluation,
                    Name,
                    "Identifier evaluator received an incompatible node."
                );

                return null;
            }

            return context.ResolveIdentifier(
                identifier.Name);
        }
    }
}
'@

Write-QslFile `
    -Path $Path `
    -Content $Content

# ============================================================
# 6. Normalize Qsl.Core.Base -> QSL.Core.Base
#
# This targets both namespace declarations and using statements.
# ============================================================

Write-Host ""
Write-Host "Normalizing Qsl.Core.Base namespace..." -ForegroundColor Cyan

$AllSourceFiles = @(
    Get-ChildItem `
        -LiteralPath $Root `
        -Recurse `
        -File `
        -Filter "*.cs"
)

$ChangedNamespaceFiles = 0

foreach ($File in $AllSourceFiles)
{
    $Changed = Update-TextFile `
        -Path $File.FullName `
        -OldText "Qsl.Core.Base" `
        -NewText "QSL.Core.Base"

    if ($Changed)
    {
        $ChangedNamespaceFiles++

        $Relative =
        $File.FullName.Substring(
                $Root.Length
        ).TrimStart('\')

        Write-Host "UPDATED  $Relative" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Qsl.Core.Base replacements: $ChangedNamespaceFiles"

# ============================================================
# 7. Normalize duplicated abstraction namespace
#
# Old:
# QSL.Abstractions.Base.QSL.Abstractions.Runtime
#
# New:
# QSL.Abstractions.Runtime
#
# ============================================================

Write-Host ""
Write-Host "Normalizing duplicated abstraction namespace..." -ForegroundColor Cyan

# Refresh source list in case prior writes changed anything.
$AllSourceFiles = @(
    Get-ChildItem `
        -LiteralPath $Root `
        -Recurse `
        -File `
        -Filter "*.cs"
)

$ChangedAbstractionFiles = 0

foreach ($File in $AllSourceFiles)
{
    $Changed = Update-TextFile `
        -Path $File.FullName `
        -OldText "QSL.Abstractions.Base.QSL.Abstractions.Runtime" `
        -NewText "QSL.Abstractions.Runtime"

    if ($Changed)
    {
        $ChangedAbstractionFiles++

        $Relative =
        $File.FullName.Substring(
                $Root.Length
        ).TrimStart('\')

        Write-Host "UPDATED  $Relative" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Abstraction namespace replacements: $ChangedAbstractionFiles"

# ============================================================
# Validation
# ============================================================

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " VALIDATING FIX PASS" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

$ValidationErrors = @()

$AllSourceFiles = @(
    Get-ChildItem `
        -LiteralPath $Root `
        -Recurse `
        -File `
        -Filter "*.cs"
)

foreach ($File in $AllSourceFiles)
{
    $Content = Get-Content `
        -LiteralPath $File.FullName `
        -Raw

    $Relative =
    $File.FullName.Substring(
            $Root.Length
    ).TrimStart('\')

    if ($Content.Contains(
            "QSL.Abstractions.Base.QSL.Abstractions.Runtime"))
    {
        $ValidationErrors +=
        "$Relative still contains duplicated abstraction namespace."
    }

    if ($Content.Contains(
            "Qsl.Core.Base"))
    {
        $ValidationErrors +=
        "$Relative still contains Qsl.Core.Base."
    }
}

# ------------------------------------------------------------
# Validate UnityEngine removal
# ------------------------------------------------------------

$DampenPath = Join-Path `
    $Core `
    "Runtime\Functions\DampenVelocityFunction.cs"

$DampenContent = Get-Content `
    -LiteralPath $DampenPath `
    -Raw

if ($DampenContent -match "\bUnityEngine\b")
{
    $ValidationErrors +=
    "DampenVelocityFunction.cs still references UnityEngine."
}

if ($DampenContent -match "\bDebug\.Log\s*\(")
{
    $ValidationErrors +=
    "DampenVelocityFunction.cs still contains Debug.Log."
}

# ------------------------------------------------------------
# Validate System.Numerics removal
# ------------------------------------------------------------

$LiteralPath = Join-Path `
    $Core `
    "Runtime\Evaluators\QslLiteralEvaluator.cs"

$LiteralContent = Get-Content `
    -LiteralPath $LiteralPath `
    -Raw

if ($LiteralContent -match "\bSystem\.Numerics\b")
{
    $ValidationErrors +=
    "QslLiteralEvaluator.cs still references System.Numerics."
}

# ------------------------------------------------------------
# Validate block evaluation path
# ------------------------------------------------------------

$BlockPath = Join-Path `
    $Core `
    "Runtime\Evaluators\QslBlockEvaluator.cs"

$BlockContent = Get-Content `
    -LiteralPath $BlockPath `
    -Raw

if ($BlockContent -match "context\.Registry\.Evaluate")
{
    $ValidationErrors +=
    "QslBlockEvaluator.cs still calls context.Registry.Evaluate."
}

if ($BlockContent -notmatch "context\.Evaluate\s*\(")
{
    $ValidationErrors +=
    "QslBlockEvaluator.cs does not call context.Evaluate."
}

# ------------------------------------------------------------
# Validate identifier consolidation
# ------------------------------------------------------------

$IdentifierPath = Join-Path `
    $Core `
    "Runtime\Evaluators\QslIdentifierEvaluator.cs"

$IdentifierContent = Get-Content `
    -LiteralPath $IdentifierPath `
    -Raw

if ($IdentifierContent -notmatch "context\.ResolveIdentifier\s*\(")
{
    $ValidationErrors +=
    "QslIdentifierEvaluator.cs does not use ResolveIdentifier."
}

if ($IdentifierContent -match "context\.Registry\.Functions")
{
    $ValidationErrors +=
    "QslIdentifierEvaluator.cs still performs direct function lookup."
}

if ($IdentifierContent -match "context\.Registry\.Descriptors")
{
    $ValidationErrors +=
    "QslIdentifierEvaluator.cs still performs direct descriptor lookup."
}

if ($IdentifierContent -match "context\.Registry\.Capabilities")
{
    $ValidationErrors +=
    "QslIdentifierEvaluator.cs still performs direct capability lookup."
}

# ------------------------------------------------------------
# Validate guarded evaluator casts
# ------------------------------------------------------------

if ($IdentifierContent -match
        "\(QslIdentifierNode\)\s*node")
{
    $ValidationErrors +=
    "QslIdentifierEvaluator.cs still contains a direct node cast."
}

if ($LiteralContent -match
        "\(QslLiteralNode\)\s*node")
{
    $ValidationErrors +=
    "QslLiteralEvaluator.cs still contains a direct node cast."
}

# ------------------------------------------------------------
# Validate arithmetic operators use QslOperatorBase
# ------------------------------------------------------------

$OperatorFiles = @(
    "QslMultiplyOperator.cs",
    "QslDivideOperator.cs",
    "QslModuloOperator.cs"
)

foreach ($OperatorFileName in $OperatorFiles)
{
    $OperatorPath = Join-Path `
        $Core `
        "Runtime\Operators\$OperatorFileName"

    $OperatorContent = Get-Content `
        -LiteralPath $OperatorPath `
        -Raw

    if ($OperatorContent -notmatch
            ":\s*QslOperatorBase")
    {
        $ValidationErrors +=
        "$OperatorFileName does not inherit QslOperatorBase."
    }

    if ($OperatorContent -match
            "throw\s+new\s+Exception")
    {
        $ValidationErrors +=
        "$OperatorFileName still throws a raw Exception."
    }

    if ($OperatorContent -match
            "public\s+int\s+Precedence")
    {
        $ValidationErrors +=
        "$OperatorFileName still owns precedence."
    }

    if ($OperatorContent -match
            "public\s+bool\s+RightAssociative")
    {
        $ValidationErrors +=
        "$OperatorFileName still owns associativity."
    }
}

# ============================================================
# Result
# ============================================================

Write-Host ""

if ($ValidationErrors.Count -gt 0)
{
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host " QSL FIX PASS HAS VALIDATION ERRORS" -ForegroundColor Red
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host ""

    foreach ($ValidationError in $ValidationErrors)
    {
        Write-Host "  ERROR: $ValidationError" -ForegroundColor Red
    }

    Write-Host ""
    Write-Host "Backup available at:" -ForegroundColor Yellow
    Write-Host "  $BackupRoot"
    Write-Host ""

    exit 1
}

Write-Host "==========================================" -ForegroundColor Green
Write-Host " QSL CORE FIX PASS COMPLETE" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "All requested transformations validated." -ForegroundColor Green
Write-Host ""
Write-Host "Backup:"
Write-Host "  $BackupRoot" -ForegroundColor Yellow
Write-Host ""
Write-Host "Next:"
Write-Host "  1. Allow Unity to recompile."
Write-Host "  2. Resolve any compiler errors before further cleanup."
Write-Host "  3. Regenerate QSL_Core_Audit.txt."
Write-Host "  4. Compare the new audit before deleting legacy wrappers."
Write-Host ""