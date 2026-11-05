using System.Text.Json.Serialization;

internal sealed class QslRuntimeStateCertificationReport
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("passed")]
    public int Passed { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("results")]
    public List<QslRuntimeStateCertificationResult>
        Results { get; set; } =
            new();
}

internal sealed class QslRuntimeStateCertificationResult
{
    [JsonPropertyName("name")]
    public string Name { get; set; } =
        string.Empty;

    [JsonPropertyName("passed")]
    public bool Passed { get; set; }

    [JsonPropertyName("details")]
    public string Details { get; set; } =
        string.Empty;

    [JsonPropertyName("exception")]
    public string? Exception { get; set; }
}
