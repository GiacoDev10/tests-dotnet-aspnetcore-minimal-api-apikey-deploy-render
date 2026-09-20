using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RestFullApiKey.Options;
using RestFullApiKey.Security;
using System.Text.Encodings.Web;

namespace RestFullApiKeyTests.Security;

public class ApiKeyAuthenticationHandlerTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task HandleAuthenticateAsync_Fail_WhenSecretKeyIsEmptyOrNotProvided(string? secretKey)
    {
        var apiOptions = new ApiKeyAuthenticationOptions { SecretKey = secretKey };

        var handler = CreateHandler(apiOptions);

        var scheme = new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, "X-API-KEY", typeof(ApiKeyAuthenticationHandler));
        var context = new DefaultHttpContext();

        await handler.InitializeAsync(scheme, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("API key is not configured on the server.", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_NoResult_NoHeaderProvided()
    {
        var apiOptions = new ApiKeyAuthenticationOptions { SecretKey = "test-secret-key" };

        var handler = CreateHandler(apiOptions);

        var scheme = new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, "X-API-KEY", typeof(ApiKeyAuthenticationHandler));
        var context = new DefaultHttpContext();

        await handler.InitializeAsync(scheme, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.False(result.Failure is not null);
        Assert.True(result.None);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Fail_InvalidApiKey()
    {
        var apiOptions = new ApiKeyAuthenticationOptions { SecretKey = "test-secret-key" };

        var handler = CreateHandler(apiOptions);

        var scheme = new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, "X-API-KEY", typeof(ApiKeyAuthenticationHandler));
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiKeyAuthenticationOptions.ApiKeyHeaderName] = "invalid-api";

        await handler.InitializeAsync(scheme, context);

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid API key.", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_Success_WhenKeyMatches()
    {
        var apiOptions = new ApiKeyAuthenticationOptions { SecretKey = "test-secret-key" };

        var handler = CreateHandler(apiOptions);

        var scheme = new AuthenticationScheme(ApiKeyAuthenticationOptions.DefaultScheme, "X-API-KEY", typeof(ApiKeyAuthenticationHandler));
        var context = new DefaultHttpContext();
        context.Request.Headers[ApiKeyAuthenticationOptions.ApiKeyHeaderName] = "test-secret-key";

        await handler.InitializeAsync(scheme, context);

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.Equal("static-client", result.Principal?.Identity!.Name);
    }

    private static ApiKeyAuthenticationHandler CreateHandler(ApiKeyAuthenticationOptions apiOptions)
    {
        var optionsMonitorMock = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(apiOptions);
        optionsMonitorMock.Setup(x => x.CurrentValue).Returns(apiOptions);

        return new ApiKeyAuthenticationHandler(
            optionsMonitorMock.Object, NullLoggerFactory.Instance, UrlEncoder.Default);
    }
}
