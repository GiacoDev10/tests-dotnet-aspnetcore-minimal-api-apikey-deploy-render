using RestFullApiKey.Options;

namespace RestFullApiKey.Security
{
    public static class TurnstileExtensions
    {
        public static IServiceCollection AddTurnstileSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<TurnstileOptions>()
                .Bind(configuration.GetSection(TurnstileOptions.SectionName))
                .Validate(o => !string.IsNullOrWhiteSpace(o.SecretKey), "Turnstile secret key must be provided.")
                .ValidateOnStart();

            return services;
        }
    }
}
