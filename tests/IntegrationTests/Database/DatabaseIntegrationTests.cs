using Domain.Todos;
using Infrastructure.Database;
using IntegrationTests.WebApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Database;

[Collection(nameof(IntegrationTestCollection))]
public sealed class DatabaseIntegrationTests(IntegrationTestWebAppFactory factory)
{
    [Fact]
    public async Task CanAddAndRetrieveTodoItem()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenantId = Guid.NewGuid();
        var todoItem = new TodoItem
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TenantId = tenantId,
            Description = "Integration test todo",
            Priority = Priority.Normal,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.TodoItems.Add(todoItem);
        await dbContext.SaveChangesAsync();

        TodoItem? retrieved = await dbContext.TodoItems
            .FirstOrDefaultAsync(t => t.Id == todoItem.Id);

        retrieved.ShouldNotBeNull();
        retrieved.Description.ShouldBe("Integration test todo");
    }
}
