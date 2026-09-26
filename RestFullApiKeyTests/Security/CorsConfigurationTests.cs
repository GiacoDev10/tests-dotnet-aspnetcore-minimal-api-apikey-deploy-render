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
}
