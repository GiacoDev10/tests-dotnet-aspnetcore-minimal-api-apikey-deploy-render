using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using RestFullApiKey.Options;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace RestFullApiKey.Security;

public static class RateLimitingExtensions
{
    /// <summary>
    /// Name of the policy to limit POST requests. Apply with .RequireRateLimiting(PostPolicyName).
    /// </summary>
    public const string PostPolicyName = "post-limit";

    public static IServiceCollection AddPostRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .Validate(options => options.PermitLimit > 0, "PermitLimit must be greater than 0.")
            .Validate(options => options.WindowSeconds > 0, "WindowSeconds must be greater than 0.")
            .Validate(options => options.QueueLimit >= 0, "QueueLimit must be a non-negative integer.")
            .ValidateOnStart();

        services.AddRateLimiter();

        services.AddOptions<RateLimiterOptions>().Configure<IOptions<RateLimitingOptions>>((options, ratelimitOptions) =>
            {
                var rateLimit = ratelimitOptions.Value;
                var window = TimeSpan.FromSeconds(rateLimit.WindowSeconds);

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(PostPolicyName, httpContext =>
                {
                    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"ip-{clientIp}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimit.PermitLimit,
                            Window = window,
                            QueueLimit = rateLimit.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var response = context.HttpContext.Response;

                    var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? (int)Math.Ceiling(retryAfter.TotalSeconds)
                        : rateLimit.WindowSeconds;

                    response.StatusCode = StatusCodes.Status429TooManyRequests;
                    response.ContentType = "application/problem+json";
                    response.Headers.RetryAfter = retryAfterSeconds.ToString();

                    var problem = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Too Many Requests",
                        Detail = $"Rate limit exceeded. {rateLimit.PermitLimit} requests per {rateLimit.WindowSeconds}s.",
                        Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Status/429",
                        Instance = context.HttpContext.Request.Path
                    };

                    var json = JsonSerializer.Serialize(
                        problem,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });

                    await response.WriteAsync(json, cancellationToken);
                };
            });

        return services;
    }

    /// <summary>
    /// Configure the processing of headers forwarded by the Render proxy.
    /// It must be called in the builder, followed by `app.UseForwardedHeaders()` as the FIRST middleware.
    /// </summary>
    public static IServiceCollection AddRenderForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;

            // In Render you do not control the proxy IP range, so the trust lists are emptied.
            // ForwardLimit = 1 uses only the last value added by the proxy (the client cannot forge it).
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            options.ForwardLimit = 1;
        });

        return services;
    }
}
