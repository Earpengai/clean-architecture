using Domain.Todos;
using Domain.Users;
using Infrastructure.Database;
using IntegrationTests.WebApi;
using Microsoft.AspNetCore.Identity;
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
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var testUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser@test.com",
            Email = "testuser@test.com",
            FirstName = "Test",
            LastName = "User",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await userManager.CreateAsync(testUser, "TestPass123!");

        var tenantId = Guid.NewGuid();
        var todoItem = new TodoItem
        {
            Id = Guid.NewGuid(),
            UserId = testUser.Id,
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
