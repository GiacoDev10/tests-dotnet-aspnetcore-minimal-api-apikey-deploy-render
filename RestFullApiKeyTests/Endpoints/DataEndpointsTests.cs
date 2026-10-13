using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestFullApiKey.Options;
using System.Net;
using System.Net.Http.Json;

namespace RestFullApiKeyTests.Endpoints
{
    public class DataEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private const string TestApiKey = "sk_test_jH/kiox1NerTVsknMeYRA2KOp2H9apLW47yN/ATymE0=";
        private readonly HttpClient _client;

        public DataEndpointsTests(WebApplicationFactory<Program> factory)
        {
            var configuresFactory = factory.WithWebHostBuilder(b =>
            {
                b.ConfigureAppConfiguration((_, cfg) =>

                    cfg.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["SecretKey"] = TestApiKey
                    }));

                // Backup in case ValidateOnStart has already captured the value.
                b.ConfigureTestServices(services =>
                services.Configure<ApiKeyAuthenticationOptions>(
                    ApiKeyAuthenticationOptions.DefaultScheme,
                    o => o.SecretKey = TestApiKey));
            });

            _client = configuresFactory.CreateClient();
            _client.DefaultRequestHeaders.Add(ApiKeyAuthenticationOptions.ApiKeyHeaderName, TestApiKey);
            _client.DefaultRequestHeaders.Add("X-Forwarded-For", "0.0.0.0");
        }

        [Fact]
        public async Task PostData_Success_Return200()
        {
            var response = await _client.PostAsJsonAsync(
                "/data", new { message = "Hi!", turnstileToken = "1x00000000000000000000AA" },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("{\"message\":null}")]
        [InlineData("{}")]
        [InlineData("{\"message\":\"\"}")]
        public async Task PostData_Invalid_Return400WithErrorMessage(string json)
        {
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/data", content, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("message", body, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("errors", body, StringComparison.OrdinalIgnoreCase);
        }
    }
}
