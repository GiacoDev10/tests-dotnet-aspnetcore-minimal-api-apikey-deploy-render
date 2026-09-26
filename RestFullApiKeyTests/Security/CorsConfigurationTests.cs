using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace RestFullApiKeyTests.Security;

public class CorsConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("http://localhost:4200")]
    [InlineData("https://localhost:4200")]
    [InlineData("http://localhost:7000")]
    [InlineData("http://localhost:5023")]
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

    [Fact]
    public async Task Preflight_AllowedOrigin_ReturnsHeaders()
    {
        var client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/");
        request.Headers.Add("Origin", "http://localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin").First());
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
