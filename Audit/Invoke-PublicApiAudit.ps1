param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root = (Resolve-Path -LiteralPath $Root).Path

$ProjectPath = Join-Path $Root "QSL.csproj"
$ReportsPath = Join-Path $Root "Audit\Reports"

if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "QSL project was not found: $ProjectPath"
}

New-Item `
    -ItemType Directory `
    -Path $ReportsPath `
    -Force |
        Out-Null

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$ReportPath = Join-Path `
    $ReportsPath `
    "QSL_Public_API_Audit_$Timestamp.txt"

$LatestPath = Join-Path `
    $ReportsPath `
    "QSL_Public_API_Audit_Latest.txt"

# ============================================================
# SOURCE INVENTORY
# ============================================================

$SourceFiles = @()

foreach ($RelativeRoot in @("Abstractions", "Core")) {
    $SourceRoot = Join-Path $Root $RelativeRoot

    if (-not (Test-Path -LiteralPath $SourceRoot)) {
        continue
    }

    $SourceFiles += @(
        Get-ChildItem `
            -LiteralPath $SourceRoot `
            -Recurse `
            -File `
            -Filter "*.cs"
    )
}

$SourceFiles = @(
$SourceFiles |
        Where-Object {
            $_.FullName -notmatch '\\bin\\' -and
                    $_.FullName -notmatch '\\obj\\'
        } |
        Sort-Object FullName
)

if ($SourceFiles.Count -eq 0) {
    throw "No QSL library source files were found."
}

$SourcePaths = @(
$SourceFiles |
        ForEach-Object {
            $_.FullName
        }
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

function Get-RelativePath {
    param(
        [string]$Path
    )

    return $Path.Substring(
            $Root.Length
    ).TrimStart('\')
}

function Get-SourceNamespace {
    param(
        [string]$Path
    )

    $NamespaceMatch = Select-String `
        -LiteralPath $Path `
        -Pattern '^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)' |
            Select-Object -First 1

    if ($null -eq $NamespaceMatch) {
        return "<global>"
    }

    if (
    $NamespaceMatch.Line -match
            '^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)'
    ) {
        return $Matches[1]
    }

    return "<global>"
}

function Get-PreliminaryCategory {
    param(
        [string]$RelativePath,
        [string]$TypeName
    )

    if ($RelativePath -like "Abstractions\*") {
        return "PUBLIC CONTRACT"
    }

    if (
    $TypeName -eq "QslTokenKind" -or
            $TypeName -eq "QslTokenAttribute" -or
            $TypeName -eq "QslKindExtensions" -or
            $TypeName -eq "QslEvaluationStrategy"
    ) {
        return "PUBLIC LANGUAGE METADATA"
    }

    if (
    $TypeName -eq "QslEngine" -or
            $TypeName -eq "QslParseResult" -or
            $TypeName -eq "QslProcessingResult"
    ) {
        return "PUBLIC SURFACE"
    }

    if (
    $TypeName -eq "QslRuntime" -or
            $TypeName -eq "QslRuntimeRegistry" -or
            $TypeName -eq "QslRuntimeContext"
    ) {
        return "PUBLIC INTEGRATION"
    }

    if (
    $RelativePath -like "Core\Base\*" -or
            $TypeName -eq "QslContextBase"
    ) {
        return "PUBLIC EXTENSION"
    }

    if ($RelativePath -like "Core\Provenance\*") {
        return "PUBLIC DATA/DIAGNOSTICS"
    }

    if (
    $RelativePath -like "Core\Custom\*" -or
            $RelativePath -like "Core\Runtime\Capabilities\*" -or
            $RelativePath -like "Core\Runtime\Descriptors\*" -or
            $RelativePath -like "Core\Runtime\Functions\*"
    ) {
        return "PUBLIC SAMPLE/INTEGRATION"
    }

    if (
    $RelativePath -like "Core\AST\*" -or
            $RelativePath -like "Core\Lexer\*" -or
            $RelativePath -like "Core\Parser\*" -or
            $RelativePath -like "Core\Runtime\Evaluators\*" -or
            $RelativePath -like "Core\Runtime\Helpers\*" -or
            $RelativePath -like "Core\Runtime\Operators\*" -or
            $RelativePath -like "Core\Extensions\*"
    ) {
        return "IMPLEMENTATION CANDIDATE"
    }

    return "REVIEW"
}

# ============================================================
# PUBLIC TYPE DISCOVERY
# ============================================================

$DeclarationPattern =
'^\s*public\s+' +
        '(?<Modifiers>(?:(?:abstract|sealed|static|partial|readonly)\s+)*)' +
        '(?<Kind>class|interface|struct|enum)\s+' +
        '(?<Name>[A-Za-z_][A-Za-z0-9_]*)'

$TypeRecords = @()

foreach ($File in $SourceFiles) {
    $Namespace = Get-SourceNamespace `
        -Path $File.FullName

    $RelativePath = Get-RelativePath `
        -Path $File.FullName

    $DeclarationMatches = @(
        Select-String `
            -LiteralPath $File.FullName `
            -Pattern $DeclarationPattern
    )

    foreach ($DeclarationMatch in $DeclarationMatches) {
        if ($DeclarationMatch.Line -notmatch $DeclarationPattern) {
            continue
        }

        $Modifiers = $Matches["Modifiers"].Trim()
        $TypeKind = $Matches["Kind"]
        $TypeName = $Matches["Name"]

        $Category = Get-PreliminaryCategory `
            -RelativePath $RelativePath `
            -TypeName $TypeName

        $TypeRecords += [pscustomobject]@{
            Name = $TypeName
            Kind = $TypeKind
            Modifiers = $Modifiers
            Namespace = $Namespace
            File = $File.FullName
            RelativePath = $RelativePath
            DeclarationLine = $DeclarationMatch.LineNumber
            Declaration = $DeclarationMatch.Line.Trim()
            Category = $Category
            SameFileReferenceCount = 0
            ExternalReferenceCount = 0
            ExternalReferences = @()
        }
    }
}

$TypeRecords = @(
$TypeRecords |
        Sort-Object Category, Namespace, Name
)

# ============================================================
# REFERENCE ANALYSIS
# ============================================================

foreach ($Type in $TypeRecords) {
    $AllReferences = @(
        Select-String `
            -LiteralPath $SourcePaths `
            -SimpleMatch `
            -Pattern $Type.Name
    )

    $SameFileReferences = @(
    $AllReferences |
            Where-Object {
                $_.Path -eq $Type.File -and
                        $_.LineNumber -ne $Type.DeclarationLine
            }
    )

    $ExternalReferences = @(
    $AllReferences |
            Where-Object {
                $_.Path -ne $Type.File
            }
    )

    $Type.SameFileReferenceCount =
    $SameFileReferences.Count

    $Type.ExternalReferenceCount =
    $ExternalReferences.Count

    $Type.ExternalReferences =
    $ExternalReferences
}

# ============================================================
# AUDIT GROUPS
# ============================================================

$Categories = @(
    "PUBLIC SURFACE",
    "PUBLIC INTEGRATION",
    "PUBLIC EXTENSION",
    "PUBLIC CONTRACT",
    "PUBLIC LANGUAGE METADATA",
    "PUBLIC DATA/DIAGNOSTICS",
    "PUBLIC SAMPLE/INTEGRATION",
    "IMPLEMENTATION CANDIDATE",
    "REVIEW"
)

$ImplementationCandidates = @(
$TypeRecords |
        Where-Object {
            $_.Category -eq "IMPLEMENTATION CANDIDATE"
        }
)

$ZeroExternalReferences = @(
$TypeRecords |
        Where-Object {
            $_.ExternalReferenceCount -eq 0
        }
)

$ZeroReferenceImplementationCandidates = @(
$ImplementationCandidates |
        Where-Object {
            $_.ExternalReferenceCount -eq 0
        }
)

# ============================================================
# REPORT HEADER
# ============================================================

Add-Line "============================================================"
Add-Line "QSL PUBLIC API AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"
Add-Line "Project: $ProjectPath"
Add-Line "Library C# files scanned: $($SourceFiles.Count)"
Add-Line "Public types discovered: $($TypeRecords.Count)"
Add-Line
Add-Line "This report provides preliminary API classifications."
Add-Line "It does not change accessibility or authorize removal."
Add-Line
Add-Line "External references are references from another source file"
Add-Line "inside the QSL library project."
Add-Line "They do not measure usage by downstream applications."

# ============================================================
# CLASSIFICATION SUMMARY
# ============================================================

Add-Header "CLASSIFICATION SUMMARY"

foreach ($Category in $Categories) {
    $Count = @(
    $TypeRecords |
            Where-Object {
                $_.Category -eq $Category
            }
    ).Count

    Add-Line (
    $Category.PadRight(32) +
            $Count
    )
}

# ============================================================
# CATEGORY DETAILS
# ============================================================

foreach ($Category in $Categories) {
    Add-Header $Category

    $CategoryRecords = @(
    $TypeRecords |
            Where-Object {
                $_.Category -eq $Category
            }
    )

    if ($CategoryRecords.Count -eq 0) {
        Add-Line "<none>"
        continue
    }

    foreach ($Type in $CategoryRecords) {
        Add-Line
        Add-Line "TYPE: $($Type.Namespace).$($Type.Name)"
        Add-Line "Kind: $($Type.Kind)"

        if (
        $null -eq $Type.Modifiers -or
                $Type.Modifiers.Length -eq 0
        ) {
            Add-Line "Modifiers: <none>"
        }
        else {
            Add-Line "Modifiers: $($Type.Modifiers)"
        }

        Add-Line "File: $($Type.RelativePath)"
        Add-Line "External references: $($Type.ExternalReferenceCount)"
        Add-Line "Same-file references: $($Type.SameFileReferenceCount)"
        Add-Line "Declaration: $($Type.Declaration)"

        if ($Type.ExternalReferenceCount -gt 0) {
            Add-Line "External reference locations:"

            foreach ($Reference in $Type.ExternalReferences) {
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
    }
}

# ============================================================
# ZERO-REFERENCE REVIEW
# ============================================================

Add-Header "PUBLIC TYPES WITH ZERO EXTERNAL REFERENCES"

if ($ZeroExternalReferences.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Type in $ZeroExternalReferences) {
        Add-Line (
        $Type.Category.PadRight(32) +
                $Type.Namespace +
                "." +
                $Type.Name +
                "  [" +
                $Type.RelativePath +
                "]"
        )
    }
}

Add-Header "ZERO-REFERENCE IMPLEMENTATION CANDIDATES"

if ($ZeroReferenceImplementationCandidates.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Type in $ZeroReferenceImplementationCandidates) {
        Add-Line (
        $Type.Namespace +
                "." +
                $Type.Name +
                "  [" +
                $Type.RelativePath +
                "]"
        )
    }
}

# ============================================================
# COMPLETE INVENTORY
# ============================================================

Add-Header "COMPLETE PUBLIC TYPE INVENTORY"

foreach ($Type in $TypeRecords) {
    $QualifiedName =
    $Type.Namespace +
            "." +
            $Type.Name

    Add-Line (
    $Type.Category.PadRight(32) +
            $Type.Kind.PadRight(12) +
            $QualifiedName.PadRight(68) +
            $Type.ExternalReferenceCount.ToString().PadLeft(5) +
            "  " +
            $Type.RelativePath
    )
}

# ============================================================
# ARCHITECTURAL REVIEW QUESTIONS
# ============================================================

Add-Header "ARCHITECTURAL REVIEW QUESTIONS"

Add-Line "1. Is QslEngine the primary external language boundary?"
Add-Line
Add-Line "2. Which runtime types are intentional host-integration APIs?"
Add-Line
Add-Line "3. Which base classes are supported inheritance points?"
Add-Line
Add-Line "4. Should tokenizer, parser, evaluator, and operator concrete"
Add-Line "   implementations remain directly public?"
Add-Line
Add-Line "5. Which provenance types are intentional public diagnostics?"
Add-Line
Add-Line "6. Are capabilities, descriptors, functions, Vector, and"
Add-Line "   Velocity product APIs or reference integrations?"
Add-Line
Add-Line "7. Should AST nodes remain public for tooling and inspection?"
Add-Line
Add-Line "8. Which public enums are required by public contracts?"

# ============================================================
# SUMMARY
# ============================================================

Add-Header "SUMMARY"

Add-Line "Library C# files: $($SourceFiles.Count)"
Add-Line "Public types: $($TypeRecords.Count)"
Add-Line "Implementation candidates: $($ImplementationCandidates.Count)"
Add-Line "Public types with zero external references: $($ZeroExternalReferences.Count)"

Add-Line (
"Zero-reference implementation candidates: " +
        $ZeroReferenceImplementationCandidates.Count
)

Add-Line
Add-Line "STATUS: PUBLIC API REVIEW READY"
Add-Line
Add-Line "No QSL source files were modified."

# ============================================================
# WRITE REPORT
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
Write-Host "QSL PUBLIC API AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "Library C# files: $($SourceFiles.Count)"
Write-Host "Public types: $($TypeRecords.Count)"
Write-Host "Implementation candidates: $($ImplementationCandidates.Count)"
Write-Host "Zero external references: $($ZeroExternalReferences.Count)"

Write-Host (
"Zero-reference implementation candidates: " +
        $ZeroReferenceImplementationCandidates.Count
)

Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host