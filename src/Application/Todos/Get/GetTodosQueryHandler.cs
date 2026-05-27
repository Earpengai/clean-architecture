using System.Linq.Expressions;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Domain.Todos;
using Application.Extensions;
using SharedKernel;

namespace Application.Todos.Get;

internal sealed class GetTodosQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTodosQuery, PaginatedList<TodoResponse>>
{
    private static readonly Dictionary<string, Expression<Func<TodoItem, string>>> SearchMap = new()
    {
        ["description"] = t => t.Description,
    };

    private static readonly Dictionary<string, Expression<Func<TodoItem, object>>> SortMap = new()
    {
        ["description"] = t => t.Description,
        ["dueDate"] = t => t.DueDate,
        ["createdAt"] = t => t.CreatedAt,
        ["priority"] = t => t.Priority,
        ["isCompleted"] = t => t.IsCompleted,
    };

    private static readonly Dictionary<string, Expression<Func<TodoItem, object>>> FilterMap = new()
    {
        ["description"] = t => t.Description,
        ["dueDate"] = t => t.DueDate,
        ["priority"] = t => t.Priority,
        ["isCompleted"] = t => t.IsCompleted,
        ["createdAt"] = t => t.CreatedAt,
    };

    public async Task<Result<PaginatedList<TodoResponse>>> Handle(
        GetTodosQuery query,
        CancellationToken cancellationToken)
    {
        PaginatedList<TodoResponse> result = await context.TodoItems
            .ApplyFilters(query.Filters, FilterMap)
            .ApplySearch(query.Search, SearchMap)
            .ApplySort(query.Sorts, SortMap, t => t.CreatedAt)
            .Select(t => new TodoResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                ParentId = t.ParentId,
                Description = t.Description,
                DueDate = t.DueDate,
                Labels = t.Labels,
                Priority = t.Priority,
                IsCompleted = t.IsCompleted,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
            })
            .ToPaginatedListAsync(query.Page, query.PageSize, cancellationToken);

        return result;
    }
}
