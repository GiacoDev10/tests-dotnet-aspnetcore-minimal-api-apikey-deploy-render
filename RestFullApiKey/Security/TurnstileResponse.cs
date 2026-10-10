using System.Text.Json.Serialization;

namespace RestFullApiKey.Security;

public class TurnstileResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    [JsonPropertyName("challenge_ts")]
    public string? ChallengeTimestamp { get; set; }
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }
    [JsonPropertyName("error-codes")]
    public string[]? ErrorCodes { get; set; }
    [JsonPropertyName("action")]
    public string? Action { get; set; }
    [JsonPropertyName("cdata")]
    public string? CData { get; set; }
    [JsonPropertyName("metadata")]
    public TurnstileMetadata? Metadata { get; set; }
}

public class TurnstileMetadata
{
    [JsonPropertyName("ephemeral_id")]
    public string? EphemeralId { get; set; }
}
