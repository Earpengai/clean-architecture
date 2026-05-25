using System.Net;
using IntegrationTests.WebApi;

namespace IntegrationTests.WebApi;

[Collection(nameof(IntegrationTestCollection))]
public sealed class HealthCheckTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
