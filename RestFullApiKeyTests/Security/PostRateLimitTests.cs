using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestFullApiKey.Options;
using System.Net;
using System.Net.Http.Json;

namespace RestFullApiKeyTests.Security;

public class PostRateLimitTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string TestApiKey = "test-secret-key";
    private HttpClient _client;

    public PostRateLimitTests(WebApplicationFactory<Program> factory)
    {
        var configuresFactory = factory.WithWebHostBuilder(b =>
        {
            b.ConfigureAppConfiguration((_, cfg) =>
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SecretKey"] = TestApiKey,
                    ["RateLimiting:PermitLimit"] = "2",
                    ["RateLimiting:WindowSeconds"] = "60",
                    ["RateLimiting:QueueLimit"] = "0",
                }));

            b.ConfigureTestServices(services =>
                services.Configure<ApiKeyAuthenticationOptions>(
                    ApiKeyAuthenticationOptions.DefaultScheme,
                    o => o.SecretKey = TestApiKey));
        });

        _client = configuresFactory.CreateClient();
        _client.DefaultRequestHeaders.Add(ApiKeyAuthenticationOptions.ApiKeyHeaderName, TestApiKey);
    }


    [Fact]
    public async Task PostData_ThirdRequest_Returns429ProblemDetails()
    {
        var ct = TestContext.Current.CancellationToken;

        for (var i = 0; i < 2; i++)
        {
            using var ok = await _client.PostAsJsonAsync("/data", new { message = "Hi!" }, ct);
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        using var limited = await _client.PostAsJsonAsync("/data", new { message = "Hi!" }, ct);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);

        Assert.Equal("application/problem+json", limited.Content.Headers.ContentType?.MediaType);

        Assert.True(limited.Headers.Contains("Retry-After"));
        var retryAfter = limited.Headers.GetValues("Retry-After").Single();
        Assert.Equal("60", retryAfter);

        var problem = await limited.Content.ReadFromJsonAsync<ProblemDetails>(ct);
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status429TooManyRequests, problem.Status);
        Assert.Equal("Too Many Requests", problem.Title);
        Assert.Contains("2 requests per 60s", problem.Detail);
    }
}