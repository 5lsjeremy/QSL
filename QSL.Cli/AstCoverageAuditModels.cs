using System.Text.Json.Serialization;

internal sealed class AstCoverageReport
{
    [JsonPropertyName("generatedUtc")]
    public string GeneratedUtc { get; set; } =
        string.Empty;

    [JsonPropertyName("suitePath")]
    public string SuitePath { get; set; } =
        string.Empty;

    [JsonPropertyName("totalStatements")]
    public int TotalStatements { get; set; }

    [JsonPropertyName("parsedStatements")]
    public int ParsedStatements { get; set; }

    [JsonPropertyName("totalAstNodes")]
    public int TotalAstNodes { get; set; }

    [JsonPropertyName("definedKindCount")]
    public int DefinedKindCount { get; set; }

    [JsonPropertyName("coveredKindCount")]
    public int CoveredKindCount { get; set; }

    [JsonPropertyName("uncoveredKindCount")]
    public int UncoveredKindCount { get; set; }

    [JsonPropertyName("complete")]
    public bool Complete { get; set; }

    [JsonPropertyName("kinds")]
    public List<AstKindCoverageRecord> Kinds { get; set; } =
        new();

    [JsonPropertyName("tests")]
    public List<AstTestCoverageRecord> Tests { get; set; } =
        new();
}

internal sealed class AstKindCoverageRecord
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } =
        string.Empty;

    [JsonPropertyName("occurrences")]
    public int Occurrences { get; set; }

    [JsonPropertyName("testCount")]
    public int TestCount { get; set; }

    [JsonPropertyName("covered")]
    public bool Covered { get; set; }
}

internal sealed class AstTestCoverageRecord
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; } =
        string.Empty;

    [JsonPropertyName("statement")]
    public string Statement { get; set; } =
        string.Empty;

    [JsonPropertyName("rootKind")]
    public string RootKind { get; set; } =
        string.Empty;

    [JsonPropertyName("nodeCount")]
    public int NodeCount { get; set; }

    [JsonPropertyName("kinds")]
    public string[] Kinds { get; set; } =
        Array.Empty<string>();

    [JsonPropertyName("parserDiagnostics")]
    public string[] ParserDiagnostics { get; set; } =
        Array.Empty<string>();
}