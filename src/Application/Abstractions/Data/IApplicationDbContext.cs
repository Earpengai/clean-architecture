using Domain.Tenants;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Membership> Memberships { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
