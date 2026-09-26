using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using System.Net;

namespace RestFullApiKeyTests.Security;

public class CorsConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsConfigurationTests(WebApplicationFactory<Program> factory)
    {
        // Create a new client with the CORS policy configured for the test - working in ci/cd pipeline
        _factory = factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
            services.Configure<CorsOptions>(options =>
            {
                options.AddPolicy("_myAllowSpecificOrigins", policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "https://giaco.dev")
                          .WithMethods("GET")
                          .AllowAnyHeader();
                });
            })
        ));
    }

    [Theory]
    [InlineData("http://localhost:4200")]
    [InlineData("https://giaco.dev")]
    public async Task Get_AllowedOrigin_ReturnsHeader(string origin)
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Origin", origin);

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(origin, response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Get_DeniedOrigin_NoHeader()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Origin", "http://evil.com");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"), "Expected no Access-Control-Allow-Origin header for denied origin.");
    }

    [Theory]
    [InlineData("http://localhost:4200")]
    [InlineData("https://giaco.dev")]
    public async Task Preflight_AllowedOrigin_ReturnsHeaders(string origin)
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(origin, response.Headers.GetValues("Access-Control-Allow-Origin").First());
        Assert.Contains("GET", response.Headers.GetValues("Access-Control-Allow-Methods").First());
    }

    [Fact]
    public async Task Preflight_DeniedOrigin_NoHeader()
    {
        var client = _factory.CreateClient();
        var req = new HttpRequestMessage(HttpMethod.Options, "/");
        req.Headers.Add("Origin", "https://evil.com");
        req.Headers.Add("Access-Control-Request-Method", "GET");

        var res = await client.SendAsync(req, TestContext.Current.CancellationToken);

        Assert.False(res.Headers.Contains("Access-Control-Allow-Origin"), "preflight de evil.com no debe devolver header");
    }
}
