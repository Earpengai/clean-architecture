using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Domain.Todos;
using Application.Extensions;
using SharedKernel;

namespace Application.Todos.Kanban;

internal sealed class GetTodosKanbanQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetTodosKanbanQuery, KanbanList<Priority, TodoCardResponse>>
{
    public async Task<Result<KanbanList<Priority, TodoCardResponse>>> Handle(
        GetTodosKanbanQuery query,
        CancellationToken cancellationToken)
    {
        KanbanList<Priority, TodoCardResponse> board = await context.TodoItems
            .ToKanbanListAsync(
                columnKeySelector: t => t.Priority,
                itemSelector: t => new TodoCardResponse
                {
                    Id = t.Id,
                    Description = t.Description,
                    Labels = t.Labels,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                },
                columnLabelSelector: priority => priority switch
                {
                    Priority.Top => "Top Priority",
                    Priority.High => "High",
                    Priority.Medium => "Medium",
                    Priority.Low => "Low",
                    Priority.Normal => "Backlog",
                    _ => priority.ToString()
                },
                columnOrderSelector: priority => priority switch
                {
                    Priority.Top => 0,
                    Priority.High => 1,
                    Priority.Medium => 2,
                    Priority.Low => 3,
                    Priority.Normal => 4,
                    _ => 99
                },
                cancellationToken);

        return board;
    }
}
