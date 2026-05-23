using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Application.Extensions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Todos.Tree;

internal sealed class GetTodosTreeQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTodosTreeQuery, TreeList<TodoTreeResponse>>
{
    public async Task<Result<TreeList<TodoTreeResponse>>> Handle(
        GetTodosTreeQuery query,
        CancellationToken cancellationToken)
    {
        TreeList<TodoTreeResponse> tree = await context.TodoItems
            .AsNoTracking()
            .Where(t => t.TenantId == userContext.TenantId!.Value)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TodoTreeResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                ParentId = t.ParentId,
                Description = t.Description,
                Priority = t.Priority,
                Labels = t.Labels,
                IsCompleted = t.IsCompleted,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
            })
            .ToTreeListAsync(cancellationToken);

        return tree;
    }
}
