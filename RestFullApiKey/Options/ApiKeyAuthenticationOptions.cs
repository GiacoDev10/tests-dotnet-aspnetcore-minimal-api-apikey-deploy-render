using Microsoft.AspNetCore.Authentication;

namespace RestFullApiKey.Options;

public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public const string ApiKeyHeaderName = "X-API-KEY";
    public string? SecretKey { get; set; }
}
