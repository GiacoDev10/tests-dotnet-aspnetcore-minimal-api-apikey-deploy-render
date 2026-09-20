using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using RestFullApiKey.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace RestFullApiKey.Security;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<ApiKeyAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var secretKey = Options.SecretKey;

        if (string.IsNullOrEmpty(secretKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API key is not configured on the server."));
        }

        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.ApiKeyHeaderName, out var providedKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // Timing Attack Safe Comparison
        byte[] providedBytes = Encoding.UTF8.GetBytes(providedKey.ToString());
        byte[] expectedBytes = Encoding.UTF8.GetBytes(secretKey);

        if (providedBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[] {
            new Claim(ClaimTypes.Name, "static-client") ,
            new Claim("client_id", "static-client")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
