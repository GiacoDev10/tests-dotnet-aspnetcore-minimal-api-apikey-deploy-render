using Microsoft.Extensions.Options;
using RestFullApiKey.Options;
using System.Text.Json;

namespace RestFullApiKey.Security;

public sealed class TurnstileService(HttpClient httpClient, IOptions<TurnstileOptions> options)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string? _secretKey = options.Value.SecretKey;
    private const string SiteverifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

    public async Task<TurnstileResponse> ValidateTokenAsync(string token, string remoteip)
    {
        var parameters = new Dictionary<string, string>
        {
            { "secret", _secretKey ?? "" },
            { "response", token }
        };

        if (!string.IsNullOrEmpty(remoteip)) parameters.Add("remoteip", remoteip);

        var postContent = new FormUrlEncodedContent(parameters);

        try
        {
            var response = await _httpClient.PostAsync(SiteverifyUrl, postContent);
            string stringContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<TurnstileResponse>(stringContent)
                ?? new TurnstileResponse
                {
                    Success = false,
                    ErrorCodes = new[] { "deserialization-error" }
                };
        }
        catch (Exception)
        {
            return new TurnstileResponse
            {
                Success = false,
                ErrorCodes = new[] { "internal-error" }
            };
        }

    }
}
