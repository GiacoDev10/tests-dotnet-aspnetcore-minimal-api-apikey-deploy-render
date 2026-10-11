using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RestFullApiKey.Options;
using RestFullApiKey.Security;
using System.Net;
using System.Text;

namespace RestFullApiKeyTests.Security;

public class TurnstileServiceTests
{
    private const string TestSecret = "test-secret";

    [Fact]
    public async Task ValidateTokenAsync_ReturnsSuccess_WhenCloudflareRespondsSuccess()
    {
        var json = """{ "success": true }""";
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new TurnstileService(httpClient, CreateOptions(TestSecret));

        var result = await service.ValidateTokenAsync("any-token", "1.2.3.4");

        Assert.True(result.Success);
        Assert.Null(result.ErrorCodes);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFailure_WithErrorCodes_WhenCloudflareFails()
    {
        var json = """{ "success": false, "error-codes": ["invalid-input-response"] }""";
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });

        var service = new TurnstileService(httpClient, CreateOptions(TestSecret));

        var result = await service.ValidateTokenAsync("bad-token", null!);

        Assert.False(result.Success);
        Assert.Contains("invalid-input-response", result.ErrorCodes!);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsInternalError_WhenResponseIsInvalidJson()
    {
        var badJson = "{ not‑json";
        var httpClient = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(badJson, Encoding.UTF8, "application/json")
        });

        var service = new TurnstileService(httpClient, CreateOptions(TestSecret));

        var result = await service.ValidateTokenAsync("token", null!);

        Assert.False(result.Success);
        Assert.Contains("internal-error", result.ErrorCodes!);
    }

    private static HttpClient CreateHttpClient(HttpResponseMessage response, Action<HttpRequestMessage>? requestInspector = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => requestInspector?.Invoke(req))
            .ReturnsAsync(response)
            .Verifiable();

        return new HttpClient(handlerMock.Object);
    }

    private static IOptions<TurnstileOptions> CreateOptions(string? secret) =>
        Options.Create(new TurnstileOptions { SecretKey = secret });
}
