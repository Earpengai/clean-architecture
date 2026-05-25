using IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using StackExchange.Redis;

namespace IntegrationTests.WebApi;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _dbFixture = new();

    public async Task InitializeAsync()
    {
        await _dbFixture.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbFixture.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbFixture.ConnectionString,
                ["Jwt:Secret"] = "this-is-a-test-secret-key-for-integration-tests-123456",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            });
        });

        builder.ConfigureServices(services =>
        {
            IConnectionMultiplexer redisMock = Substitute.For<IConnectionMultiplexer>();
            services.AddSingleton(redisMock);
        });
    }
}
