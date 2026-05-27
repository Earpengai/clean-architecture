using IntegrationTests.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

        Environment.SetEnvironmentVariable("ConnectionStrings__Database", _dbFixture.ConnectionString);
        Environment.SetEnvironmentVariable("Jwt__Secret", "this-is-a-test-secret-key-for-integration-tests-123456");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "test-issuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "test-audience");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbFixture.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            IConnectionMultiplexer redisMock = Substitute.For<IConnectionMultiplexer>();
            services.AddSingleton(redisMock);
        });
    }
}
