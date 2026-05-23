using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Application.Todos.Tree;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Tree : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("todos/tree", async (
            IQueryHandler<GetTodosTreeQuery, TreeList<TodoTreeResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTodosTreeQuery();

            Result<TreeList<TodoTreeResponse>> result =
                await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
