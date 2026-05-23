using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Application.Todos.Kanban;
using Domain.Todos;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Kanban : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("todos/kanban", async (
            IQueryHandler<GetTodosKanbanQuery, KanbanList<Priority, TodoCardResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTodosKanbanQuery();

            Result<KanbanList<Priority, TodoCardResponse>> result =
                await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
