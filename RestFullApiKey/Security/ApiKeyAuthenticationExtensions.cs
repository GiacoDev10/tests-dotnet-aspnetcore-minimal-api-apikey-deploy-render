using RestFullApiKey.Options;

namespace RestFullApiKey.Security;

public static class ApiKeyAuthenticationExtensions
{
    public static IServiceCollection AddApiKeySecurity(this IServiceCollection services, IConfiguration configuration)
    {
        var scheme = ApiKeyAuthenticationOptions.DefaultScheme;
        var secret = configuration["SecretKey"];

        services.AddOptions<ApiKeyAuthenticationOptions>(scheme)
            .Configure(options => options.SecretKey = secret)
            .Validate(options => !string.IsNullOrWhiteSpace(options.SecretKey), "SecretKey must be provided.")
            .ValidateOnStart();

        services.AddAuthentication(scheme)
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(scheme, null);

        services.AddAuthorization();

        return services;
    }
}
