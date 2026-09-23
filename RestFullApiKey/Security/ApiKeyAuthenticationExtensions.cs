using RestFullApiKey.Options;

namespace RestFullApiKey.Security;

public static class ApiKeyAuthenticationExtensions
{
    public static IServiceCollection AddApiKeySecurity(this IServiceCollection services, string secretKey)
    {
        services.AddAuthentication(ApiKeyAuthenticationOptions.DefaultScheme)
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme,
                options =>
                {
                    options.SecretKey = secretKey;
                });
        services.AddAuthorization();

        return services;
    }
}
