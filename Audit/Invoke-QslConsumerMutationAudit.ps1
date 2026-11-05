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
    "QSL_Consumer_Mutation_Audit_$Timestamp.txt"

$LatestPath = Join-Path `
    $ReportsPath `
    "QSL_Consumer_Mutation_Audit_Latest.txt"

$Report = New-Object System.Collections.Generic.List[string]

function Add-Line {
    param(
        [AllowEmptyString()]
        [string]$Text = ""
    )

    $script:Report.Add($Text)
}

function Add-Header {
    param(
        [Parameter(Mandatory)]
        [string]$Title
    )

    Add-Line
    Add-Line "============================================================"
    Add-Line $Title
    Add-Line "============================================================"
}

function Add-SubHeader {
    param(
        [Parameter(Mandatory)]
        [string]$Title
    )

    Add-Line
    Add-Line "------------------------------------------------------------"
    Add-Line $Title
    Add-Line "------------------------------------------------------------"
}

function Get-Relative {
    param(
        [Parameter(Mandatory)]
        [string]$Path
    )

    return $Path.Substring(
            $Root.Length
    ).TrimStart('\')
}

function Add-SourceFile {
    param(
        [Parameter(Mandatory)]
        [string]$RelativePath
    )

    $FullPath = Join-Path $Root $RelativePath

    Add-SubHeader "FILE: $RelativePath"

    if (-not (Test-Path -LiteralPath $FullPath)) {
        Add-Line "<not found>"
        return
    }

    Get-Content `
        -LiteralPath $FullPath |
            ForEach-Object {
                Add-Line $_
            }
}

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
                    $_.FullName -notmatch '\\\.idea\\' -and
                    $_.FullName -notmatch '\\Audit\\Reports\\'
        } |
        Sort-Object FullName
)

$SourcePaths = @(
$SourceFiles |
        ForEach-Object {
            $_.FullName
        }
)

# ============================================================
# HEADER
# ============================================================

Add-Line "============================================================"
Add-Line "QSL CONSUMER MUTATION AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"
Add-Line "C# source files scanned: $($SourceFiles.Count)"
Add-Line
Add-Line "PURPOSE:"
Add-Line "Inspect the current QSL runtime path for consumer-owned"
Add-Line "objects, member access, assignment, callable members,"
Add-Line "runtime variables, reflection, provenance, and tests."
Add-Line
Add-Line "This audit does not modify QSL source."

# ============================================================
# TARGET FILES
# ============================================================

$TargetFiles = @(
    "Core\AST\QslCallNode.cs",
    "Core\AST\QslMemberAccessNode.cs",

    "Abstractions\Runtime\IQslFunction.cs",
    "Abstractions\Contracts\IQslFunction.cs",

    "Core\Runtime\Context\QslContextBase.cs",
    "Core\Runtime\Context\QslRuntimeContext.cs",

    "Core\Runtime\Evaluators\QslCallEvaluator.cs",
    "Core\Runtime\Evaluators\QslIdentifierEvaluator.cs",
    "Core\Runtime\Evaluators\QslMemberAccessEvaluator.cs",
    "Core\Runtime\Evaluators\QslBinaryEvaluator.cs",

    "Core\Runtime\Functions\QslMethodFunction.cs",
    "Core\Runtime\Functions\HealFunction.cs",

    "Core\Runtime\QslRuntimeRegistry.cs",

    "Core\Surface\QslEngine.cs",
    "Core\QslEngine.cs"
)

Add-Header "TARGET SOURCE FILES"

$CapturedTargets = 0

foreach ($RelativePath in $TargetFiles) {
    $FullPath = Join-Path $Root $RelativePath

    if (Test-Path -LiteralPath $FullPath) {
        Add-SourceFile $RelativePath
        $CapturedTargets++
    }
}

Add-Line
Add-Line "Target source files captured: $CapturedTargets"

# ============================================================
# SYMBOL REFERENCE AUDIT
# ============================================================

$Patterns = @(
    "SetVariable",
    "GetVariable",
    "ResolveIdentifier",
    "QslMethodFunction",
    "IQslFunction",
    "QslCallNode",
    "QslMemberAccessNode",
    "MethodInfo",
    "PropertyInfo",
    "FieldInfo",
    "GetProperty",
    "GetField",
    "GetMethod",
    "SetValue",
    "GetValue",
    "DynamicInvoke",
    "Invoke",
    "Apply(",
    "Assignment",
    "RegisterType",
    "RegisterEvaluator"
)

Add-Header "SYMBOL REFERENCE INDEX"

$ReferencedFiles = New-Object `
    "System.Collections.Generic.Dictionary[string,System.IO.FileInfo]" `
    ([System.StringComparer]::OrdinalIgnoreCase)

foreach ($Pattern in $Patterns) {
    Add-SubHeader "PATTERN: $Pattern"

    $Matches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Pattern
    )

    if ($Matches.Count -eq 0) {
        Add-Line "<no matches>"
        continue
    }

    foreach ($Match in $Matches) {
        $RelativePath = Get-Relative $Match.Path

        Add-Line (
        $RelativePath +
                ":" +
                $Match.LineNumber +
                "  " +
                $Match.Line.Trim()
        )

        if (-not $ReferencedFiles.ContainsKey($Match.Path)) {
            $ReferencedFiles.Add(
                    $Match.Path,
                    [System.IO.FileInfo]::new($Match.Path)
            )
        }
    }
}

# ============================================================
# ASSIGNMENT IMPLEMENTATION
# ============================================================

Add-Header "ASSIGNMENT IMPLEMENTATION REFERENCES"

$AssignmentPatterns = @(
    "EvaluateAssignment",
    "QslEvaluationStrategy.Assignment",
    "SetVariable",
    "Left operand of assignment",
    "QslMemberAccessNode"
)

foreach ($Pattern in $AssignmentPatterns) {
    Add-SubHeader "PATTERN: $Pattern"

    $Matches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Pattern
    )

    if ($Matches.Count -eq 0) {
        Add-Line "<no matches>"
        continue
    }

    foreach ($Match in $Matches) {
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
# MEMBER ACCESS / REFLECTION
# ============================================================

Add-Header "MEMBER ACCESS AND REFLECTION REFERENCES"

$ReflectionPatterns = @(
    "System.Reflection",
    "GetProperty(",
    "GetField(",
    "GetMethod(",
    ".SetValue(",
    ".GetValue(",
    "MethodInfo",
    "PropertyInfo",
    "FieldInfo",
    "BindingFlags"
)

foreach ($Pattern in $ReflectionPatterns) {
    Add-SubHeader "PATTERN: $Pattern"

    $Matches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Pattern
    )

    if ($Matches.Count -eq 0) {
        Add-Line "<no matches>"
        continue
    }

    foreach ($Match in $Matches) {
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
# FUNCTION REGISTRATION / INVOCATION
# ============================================================

Add-Header "FUNCTION REGISTRATION AND INVOCATION"

$FunctionPatterns = @(
    "IQslFunction",
    "QslMethodFunction",
    "HealFunction",
    "RegisterType",
    "Registry.Functions",
    "Functions.TryGetValue",
    ".Invoke(",
    "function.Invoke"
)

foreach ($Pattern in $FunctionPatterns) {
    Add-SubHeader "PATTERN: $Pattern"

    $Matches = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Pattern
    )

    if ($Matches.Count -eq 0) {
        Add-Line "<no matches>"
        continue
    }

    foreach ($Match in $Matches) {
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
# RELEVANT TESTS
# ============================================================

Add-Header "RELEVANT TEST REFERENCES"

$TestPatterns = @(
    "assignment",
    "member access",
    "MemberAccess",
    "HealFunction",
    "heal(",
    "QslMethodFunction",
    "RegisterType",
    "SetVariable",
    "GetVariable",
    "QslCallNode"
)

$TestMatches = New-Object System.Collections.Generic.List[object]

foreach ($Pattern in $TestPatterns) {
    $Matches = @(
    Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Pattern |
            Where-Object {
                $_.Path -match '(Test|Tests|Certification)'
            }
    )

    foreach ($Match in $Matches) {
        $TestMatches.Add($Match)
    }
}

$UniqueTestMatches = @(
$TestMatches |
        Sort-Object Path, LineNumber -Unique
)

if ($UniqueTestMatches.Count -eq 0) {
    Add-Line "<no matching test references>"
}
else {
    foreach ($Match in $UniqueTestMatches) {
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
# FULL RELEVANT TEST FILES
# ============================================================

Add-Header "FULL RELEVANT TEST FILES"

$RelevantTestFiles = @(
$UniqueTestMatches |
        ForEach-Object {
            $_.Path
        } |
        Sort-Object -Unique
)

if ($RelevantTestFiles.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Path in $RelevantTestFiles) {
        Add-SourceFile (Get-Relative $Path)
    }
}

# ============================================================
# PARSER SHAPE
# ============================================================

Add-Header "PARSER MEMBER / CALL CONSTRUCTION"

$ParserPath = Join-Path $Root "Core\Parser\QslParser.cs"

if (Test-Path -LiteralPath $ParserPath) {
    $ParserPatterns = @(
        "ParsePostfixExpression",
        "ParseCallExpression",
        "QslMemberAccessNode",
        "QslCallNode"
    )

    foreach ($Pattern in $ParserPatterns) {
        Add-SubHeader "PATTERN: $Pattern"

        $Matches = @(
            Select-String `
                -LiteralPath $ParserPath `
                -SimpleMatch `
                -Pattern $Pattern
        )

        foreach ($Match in $Matches) {
            Add-Line (
            $Match.LineNumber.ToString() +
                    ": " +
                    $Match.Line.Trim()
            )
        }
    }
}
else {
    Add-Line "<QslParser.cs not found>"
}

# ============================================================
# CONSUMER MUTATION QUESTIONS
# ============================================================

Add-Header "AUDIT QUESTIONS"

$Questions = @(
    "1. How is an arbitrary consumer object placed into QslRuntimeContext?",
    "2. Does ResolveIdentifier return the exact stored object instance?",
    "3. How does QslMemberAccessEvaluator read a plain-object property?",
    "4. How does QslMemberAccessEvaluator read a plain-object field?",
    "5. Does plain-object member access currently enforce an exposure policy?",
    "6. Can assignment currently target anything except QslIdentifierNode?",
    "7. Where would QslMemberAccessNode assignment naturally belong?",
    "8. Is any SetValue path already present in QSL?",
    "9. Can writable property assignment preserve the exact consumer object?",
    "10. How does QslCallEvaluator resolve its target?",
    "11. Does QslCallEvaluator accept a QslMemberAccessNode target?",
    "12. What exact contract does IQslFunction require?",
    "13. What purpose does QslMethodFunction currently serve?",
    "14. Does QslMethodFunction already adapt reflected methods?",
    "15. Why does HealFunction need registration today?",
    "16. Can consumer state mutation avoid function registration entirely?",
    "17. Which existing tests protect assignment semantics?",
    "18. Which existing tests protect member-access semantics?",
    "19. Which existing tests protect function-call semantics?",
    "20. Can member assignment be added without parser or AST changes?"
)

foreach ($Question in $Questions) {
    Add-Line $Question
}

# ============================================================
# PROPOSED CERTIFICATION SHAPES
# ============================================================

Add-Header "PROPOSED CERTIFICATION SHAPES - REVIEW ONLY"

Add-Line @'
These are review targets only. This audit does not claim that
the expressions currently succeed.

CASE A - existing consumer member read

    player.Health

Questions:
    Does QSL return the current Health value?
    Is the exact supplied Player instance used?

CASE B - proposed consumer member assignment

    player.Health = 75

Questions:
    Can Assignment recognize QslMemberAccessNode?
    Can the member be validated as writable?
    Can the exact Player instance be updated?

CASE C - proposed metadata-driven mutation

    player.Health = player.Health + action.Amount

Questions:
    Can player and action both be supplied as runtime values?
    Can QSL read both values?
    Can QSL assign the resulting value to Player.Health?

CASE D - incompatible assignment

    player.Health = "bad-value"

Questions:
    Does the mutation fail honestly?
    Is provenance recorded?
    Is the original consumer state preserved?

CASE E - read-only member

    player.Name = "Changed"

Questions:
    Is a non-writable property rejected?
    Is provenance recorded?

CASE F - current registered function behavior

    heal(10)

Questions:
    What registry dependency currently makes this work?
    What occurs when HealFunction is not registered?

CASE G - possible member call, not yet proposed for implementation

    player.Heal(10)

Questions:
    Does the parser already represent this as a call whose
    target is QslMemberAccessNode?
    What runtime support would be required?
    Is that support actually desirable?
'@

# ============================================================
# GIT STATUS
# ============================================================

Add-Header "GIT STATUS"

Push-Location $Root

try {
    $GitStatus = git status --short 2>&1

    if ($null -eq $GitStatus) {
        Add-Line "<clean>"
    }
    else {
        foreach ($Line in $GitStatus) {
            Add-Line $Line
        }
    }
}
finally {
    Pop-Location
}

# ============================================================
# SUMMARY
# ============================================================

Add-Header "SUMMARY"

Add-Line "C# source files scanned: $($SourceFiles.Count)"
Add-Line "Target source files captured: $CapturedTargets"
Add-Line "Referenced source files discovered: $($ReferencedFiles.Count)"
Add-Line "Relevant test files captured: $($RelevantTestFiles.Count)"
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
Write-Host "QSL CONSUMER MUTATION AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "Report:"
Write-Host "  $ReportPath"
Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host
Write-Host "C# source files scanned: $($SourceFiles.Count)"
Write-Host "Target source files captured: $CapturedTargets"
Write-Host "Relevant test files captured: $($RelevantTestFiles.Count)"
Write-Host