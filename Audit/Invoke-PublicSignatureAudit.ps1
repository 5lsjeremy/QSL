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
    "QSL_Public_Signature_Audit_$Timestamp.txt"

$LatestPath = Join-Path `
    $ReportsPath `
    "QSL_Public_Signature_Audit_Latest.txt"

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

# ============================================================
# REPORT HELPERS
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

function Get-RelativeSourcePath {
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

    $NamespaceMatch =
    Select-String `
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

function Get-TypeCategory {
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

    if ($RelativePath -like "Core\CompilerShims\*") {
        return "COMPILER SHIM"
    }

    return "IMPLEMENTATION CANDIDATE"
}

# ============================================================
# SIGNATURE COLLECTION
# ============================================================

function Get-PublicSignatureLines {
    param(
        [string]$Path
    )

    $Lines = @(
        Get-Content `
            -LiteralPath $Path
    )

    $Results = @()

    $Buffer = ""
    $StartLine = 0
    $Collecting = $false
    $ParenDepth = 0

    for ($Index = 0; $Index -lt $Lines.Count; $Index++) {
        $Trimmed = $Lines[$Index].Trim()

        if (-not $Collecting) {
            if (
            $Trimmed -notmatch
                    '^(public|protected|protected\s+internal)\b'
            ) {
                continue
            }

            $Collecting = $true
            $StartLine = $Index + 1
            $Buffer = $Trimmed
            $ParenDepth = 0
        }
        else {
            $Buffer =
            $Buffer +
                    " " +
                    $Trimmed
        }

        $OpenParens = @(
        $Trimmed.ToCharArray() |
                Where-Object {
                    $_ -eq '('
                }
        ).Count

        $CloseParens = @(
        $Trimmed.ToCharArray() |
                Where-Object {
                    $_ -eq ')'
                }
        ).Count

        $ParenDepth +=
        $OpenParens -
                $CloseParens

        $EndsWithOpeningBrace =
        $Trimmed -match '\{\s*$'

        $EndsWithClosingBrace =
        $Trimmed -match '\}\s*$'

        $EndsWithSemicolon =
        $Trimmed -match ';\s*$'

        $ExpressionBodied =
        $Trimmed -match '=>'

        $SignatureComplete =
        $ParenDepth -le 0 -and
                (
                $EndsWithOpeningBrace -or
                        $EndsWithClosingBrace -or
                        $EndsWithSemicolon -or
                        $ExpressionBodied
                )

        if (-not $SignatureComplete) {
            continue
        }

        $Results +=
        [pscustomobject]@{
            LineNumber =
            $StartLine

            Text =
            $Buffer
        }

        $Buffer = ""
        $StartLine = 0
        $Collecting = $false
        $ParenDepth = 0
    }

    if ($Collecting -and $Buffer.Length -gt 0) {
        $Results +=
        [pscustomobject]@{
            LineNumber =
            $StartLine

            Text =
            $Buffer
        }
    }

    return $Results
}

# ============================================================
# PUBLIC TYPE INVENTORY
# ============================================================

$PublicTypePattern =
'^\s*public\s+' +
        '(?:(?:abstract|sealed|static|partial|readonly)\s+)*' +
        '(?<Kind>class|interface|struct|enum)\s+' +
        '(?<Name>[A-Za-z_][A-Za-z0-9_]*)'

$PublicTypes = @()

foreach ($File in $SourceFiles) {
    $Namespace =
    Get-SourceNamespace `
            -Path $File.FullName

    $RelativePath =
    Get-RelativeSourcePath `
            -Path $File.FullName

    $DeclarationMatches = @(
        Select-String `
            -LiteralPath $File.FullName `
            -Pattern $PublicTypePattern
    )

    foreach ($DeclarationMatch in $DeclarationMatches) {
        if (
        $DeclarationMatch.Line -notmatch
                $PublicTypePattern
        ) {
            continue
        }

        $TypeKind =
        $Matches["Kind"]

        $TypeName =
        $Matches["Name"]

        $Category =
        Get-TypeCategory `
                -RelativePath $RelativePath `
                -TypeName $TypeName

        $PublicTypes +=
        [pscustomobject]@{
            Name =
            $TypeName

            Kind =
            $TypeKind

            Namespace =
            $Namespace

            QualifiedName =
            $Namespace +
                    "." +
                    $TypeName

            File =
            $File.FullName

            RelativePath =
            $RelativePath

            DeclarationLine =
            $DeclarationMatch.LineNumber

            Category =
            $Category
        }
    }
}

$PublicTypes = @(
$PublicTypes |
        Sort-Object `
        Category,
    Namespace,
    Name
)

$FilesContainingPublicTypes = @(
$PublicTypes |
        ForEach-Object {
            $_.File
        } |
        Sort-Object -Unique
)

# ============================================================
# PUBLIC SIGNATURE INVENTORY
# ============================================================

$Signatures = @()
$ExcludedInternalOnlyFiles = @()

foreach ($File in $SourceFiles) {
    $RelativePath =
    Get-RelativeSourcePath `
            -Path $File.FullName

    $FileContainsPublicType =
    $FilesContainingPublicTypes -contains
            $File.FullName

    if (-not $FileContainsPublicType) {
        $ExcludedInternalOnlyFiles +=
        $RelativePath

        continue
    }

    $PublicSignatureLines =
    Get-PublicSignatureLines `
            -Path $File.FullName

    foreach ($Signature in $PublicSignatureLines) {
        $Signatures +=
        [pscustomobject]@{
            File =
            $File.FullName

            RelativePath =
            $RelativePath

            LineNumber =
            $Signature.LineNumber

            Text =
            $Signature.Text
        }
    }
}

$ExcludedInternalOnlyFiles = @(
$ExcludedInternalOnlyFiles |
        Sort-Object -Unique
)

# ============================================================
# SIGNATURE REACHABILITY
# ============================================================

$ReachabilityRecords = @()

foreach ($Type in $PublicTypes) {
    $TypeNamePattern =
    '\b' +
            $Type.Name +
            '\b'

    $MatchingSignatures = @(
    $Signatures |
            Where-Object {
                $_.Text -match $TypeNamePattern
            }
    )

    $ExposingSignatures = @(
    $MatchingSignatures |
            Where-Object {
                -not (
                $_.File -eq $Type.File -and
                        $_.LineNumber -eq $Type.DeclarationLine
                )
            }
    )

    $ExternalExposingSignatures = @(
    $ExposingSignatures |
            Where-Object {
                $_.File -ne $Type.File
            }
    )

    if ($ExternalExposingSignatures.Count -gt 0) {
        $Reachability =
        "EXTERNALLY REACHABLE"
    }
    elseif ($ExposingSignatures.Count -gt 0) {
        $Reachability =
        "SAME-FILE REACHABLE"
    }
    else {
        $Reachability =
        "NOT SIGNATURE REACHABLE"
    }

    $ReachabilityRecords +=
    [pscustomobject]@{
        Type =
        $Type

        Reachability =
        $Reachability

        ExposingSignatures =
        $ExposingSignatures

        ExternalExposingSignatures =
        $ExternalExposingSignatures
    }
}

# ============================================================
# RESULT GROUPS
# ============================================================

$ExternallyReachable = @(
$ReachabilityRecords |
        Where-Object {
            $_.Reachability -eq "EXTERNALLY REACHABLE"
        }
)

$SameFileReachable = @(
$ReachabilityRecords |
        Where-Object {
            $_.Reachability -eq "SAME-FILE REACHABLE"
        }
)

$NotSignatureReachable = @(
$ReachabilityRecords |
        Where-Object {
            $_.Reachability -eq "NOT SIGNATURE REACHABLE"
        }
)

$UnreachableImplementationCandidates = @(
$ReachabilityRecords |
        Where-Object {
            $_.Type.Category -eq "IMPLEMENTATION CANDIDATE" -and
                    $_.Reachability -eq "NOT SIGNATURE REACHABLE"
        }
)

# ============================================================
# REPORT HEADER
# ============================================================

Add-Line "============================================================"
Add-Line "QSL PUBLIC SIGNATURE REACHABILITY AUDIT"
Add-Line "============================================================"
Add-Line
Add-Line "Generated: $((Get-Date).ToString('O'))"
Add-Line "Root: $Root"
Add-Line "Library C# files scanned: $($SourceFiles.Count)"
Add-Line "Public types discovered: $($PublicTypes.Count)"
Add-Line "Public/protected signatures discovered: $($Signatures.Count)"

Add-Line (
"Internal-only source files excluded: " +
        $ExcludedInternalOnlyFiles.Count
)

Add-Line
Add-Line "This audit is read-only."
Add-Line "It does not change accessibility or authorize removal."
Add-Line
Add-Line "Reachability means that a public type name appears in a"
Add-Line "public or protected signature located in a source file"
Add-Line "containing at least one public top-level type."
Add-Line
Add-Line "Files containing mixed public and internal top-level types"
Add-Line "require manual review."

# ============================================================
# INTERNAL-ONLY FILES EXCLUDED
# ============================================================

Add-Header "INTERNAL-ONLY SOURCE FILES EXCLUDED"

if ($ExcludedInternalOnlyFiles.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($RelativePath in $ExcludedInternalOnlyFiles) {
        Add-Line $RelativePath
    }
}

# ============================================================
# SUMMARY
# ============================================================

Add-Header "SUMMARY"

Add-Line (
"Externally reachable: " +
        $ExternallyReachable.Count
)

Add-Line (
"Same-file reachable: " +
        $SameFileReachable.Count
)

Add-Line (
"Not signature reachable: " +
        $NotSignatureReachable.Count
)

Add-Line (
"Unreachable implementation candidates: " +
        $UnreachableImplementationCandidates.Count
)

# ============================================================
# EXTERNALLY REACHABLE
# ============================================================

Add-Header "EXTERNALLY REACHABLE TYPES"

if ($ExternallyReachable.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Record in $ExternallyReachable) {
        Add-Line
        Add-Line "TYPE: $($Record.Type.QualifiedName)"
        Add-Line "Category: $($Record.Type.Category)"
        Add-Line "File: $($Record.Type.RelativePath)"

        foreach (
        $Signature in
        $Record.ExternalExposingSignatures
        ) {
            Add-Line (
            "  " +
                    $Signature.RelativePath +
                    ":" +
                    $Signature.LineNumber +
                    "  " +
                    $Signature.Text
            )
        }
    }
}

# ============================================================
# SAME-FILE REACHABLE
# ============================================================

Add-Header "SAME-FILE REACHABLE TYPES"

if ($SameFileReachable.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Record in $SameFileReachable) {
        Add-Line
        Add-Line "TYPE: $($Record.Type.QualifiedName)"
        Add-Line "Category: $($Record.Type.Category)"
        Add-Line "File: $($Record.Type.RelativePath)"

        foreach (
        $Signature in
        $Record.ExposingSignatures
        ) {
            Add-Line (
            "  " +
                    $Signature.RelativePath +
                    ":" +
                    $Signature.LineNumber +
                    "  " +
                    $Signature.Text
            )
        }
    }
}

# ============================================================
# NOT SIGNATURE REACHABLE
# ============================================================

Add-Header "NOT SIGNATURE REACHABLE"

if ($NotSignatureReachable.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach ($Record in $NotSignatureReachable) {
        Add-Line (
        $Record.Type.Category.PadRight(32) +
                $Record.Type.QualifiedName +
                "  [" +
                $Record.Type.RelativePath +
                "]"
        )
    }
}

# ============================================================
# UNREACHABLE IMPLEMENTATION CANDIDATES
# ============================================================

Add-Header "UNREACHABLE IMPLEMENTATION CANDIDATES"

if ($UnreachableImplementationCandidates.Count -eq 0) {
    Add-Line "<none>"
}
else {
    foreach (
    $Record in
    $UnreachableImplementationCandidates
    ) {
        Add-Line (
        $Record.Type.QualifiedName +
                "  [" +
                $Record.Type.RelativePath +
                "]"
        )
    }
}

# ============================================================
# COMPLETE REACHABILITY INVENTORY
# ============================================================

Add-Header "COMPLETE REACHABILITY INVENTORY"

foreach ($Record in $ReachabilityRecords) {
    Add-Line (
    $Record.Reachability.PadRight(28) +
            $Record.Type.Category.PadRight(32) +
            $Record.Type.QualifiedName +
            "  [" +
            $Record.Type.RelativePath +
            "]"
    )
}

# ============================================================
# REVIEW RULE
# ============================================================

Add-Header "REVIEW RULE"

Add-Line "A public type that is externally signature-reachable cannot"
Add-Line "be made internal unless the exposing public API is redesigned."
Add-Line
Add-Line "A type that is not signature-reachable may still be an"
Add-Line "intentional construction, tooling, metadata, reflection, or"
Add-Line "extension API."
Add-Line
Add-Line "Not-signature-reachable does not automatically mean internal."
Add-Line
Add-Line "This audit excludes internal-only source files, but it does"
Add-Line "not distinguish member ownership in mixed-accessibility files."

# ============================================================
# FINAL STATUS
# ============================================================

Add-Header "FINAL STATUS"

Add-Line "STATUS: PUBLIC SIGNATURE REVIEW READY"
Add-Line
Add-Line "No QSL source files were modified."

# ============================================================
# WRITE REPORTS
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
Write-Host "QSL PUBLIC SIGNATURE AUDIT COMPLETE"
Write-Host "========================================"
Write-Host
Write-Host "Library C# files: $($SourceFiles.Count)"
Write-Host "Public types: $($PublicTypes.Count)"
Write-Host "Public/protected signatures: $($Signatures.Count)"

Write-Host (
"Internal-only source files excluded: " +
        $ExcludedInternalOnlyFiles.Count
)

Write-Host "Externally reachable: $($ExternallyReachable.Count)"
Write-Host "Same-file reachable: $($SameFileReachable.Count)"
Write-Host "Not signature reachable: $($NotSignatureReachable.Count)"

Write-Host (
"Unreachable implementation candidates: " +
        $UnreachableImplementationCandidates.Count
)

Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host