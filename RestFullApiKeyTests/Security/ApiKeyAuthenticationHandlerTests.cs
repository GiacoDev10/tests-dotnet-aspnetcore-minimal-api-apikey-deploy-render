using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RestFullApiKey.Options;
using RestFullApiKey.Security;
using System;
using System.Collections.Generic;
using System.Text;
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

    private static ApiKeyAuthenticationHandler CreateHandler(ApiKeyAuthenticationOptions apiOptions)
    {
        var optionsMonitorMock = new Mock<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(apiOptions);
        optionsMonitorMock.Setup(x => x.CurrentValue).Returns(apiOptions);

        return new ApiKeyAuthenticationHandler(
            optionsMonitorMock.Object, NullLoggerFactory.Instance, UrlEncoder.Default);
    }
}
