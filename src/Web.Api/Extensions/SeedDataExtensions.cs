using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Api.Extensions;

public static class SeedDataExtensions
{
    public static void SeedData(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        DataSeeder seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

        seeder.SeedAsync().GetAwaiter().GetResult();
    }
}
