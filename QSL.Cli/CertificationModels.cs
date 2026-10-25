using System.Text.Json.Serialization;

internal sealed class QslCertificationSuite
{
    [JsonPropertyName("tests")]
    public List<QslCertificationTest> Tests { get; set; } =
        new();
}

internal sealed class QslCertificationTest
{
    [JsonPropertyName("category")]
    public string Category { get; set; } =
        string.Empty;

    [JsonPropertyName("statement")]
    public string Statement { get; set; } =
        string.Empty;

    [JsonPropertyName("expected")]
    public string Expected { get; set; } =
        string.Empty;
}

internal sealed class QslCertificationReport
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("passed")]
    public int Passed { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("results")]
    public List<QslCertificationResult> Results { get; set; } =
        new();
}

internal sealed class QslCertificationResult
{
    [JsonPropertyName("category")]
    public string Category { get; set; } =
        string.Empty;

    [JsonPropertyName("statement")]
    public string Statement { get; set; } =
        string.Empty;

    [JsonPropertyName("expected")]
    public string Expected { get; set; } =
        string.Empty;

    [JsonPropertyName("output")]
    public string Output { get; set; } =
        string.Empty;

    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("succeeded")]
    public bool Succeeded { get; set; }

    [JsonPropertyName("parseFailed")]
    public bool ParseFailed { get; set; }

    [JsonPropertyName("evaluationFailed")]
    public bool EvaluationFailed { get; set; }

    [JsonPropertyName("parserDiagnostics")]
    public string[] ParserDiagnostics { get; set; } =
        Array.Empty<string>();

    [JsonPropertyName("runtimeDiagnostics")]
    public string[] RuntimeDiagnostics { get; set; } =
        Array.Empty<string>();

    [JsonPropertyName("exception")]
    public string? Exception { get; set; }
}