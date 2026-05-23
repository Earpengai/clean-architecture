using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Application.Todos.Get;
using Microsoft.AspNetCore.Http;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("todos", async (
            HttpContext httpContext,
            IQueryHandler<GetTodosQuery, PaginatedList<TodoResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            GetTodosQuery query = BuildQuery(httpContext.Request.Query);

            Result<PaginatedList<TodoResponse>> result =
                await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }

    private static GetTodosQuery BuildQuery(IQueryCollection queryParams)
    {
        int page = int.TryParse(queryParams["page"], out int parsedPage) && parsedPage > 0 ? parsedPage : 1;

        int pageSize = int.TryParse(queryParams["pageSize"], out int parsedSize) && parsedSize > 0 ? parsedSize : 20;

        string? search = queryParams["search"].FirstOrDefault();

        return new GetTodosQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            Sorts = ParseSorts(queryParams),
            Filters = ParseFilters(queryParams),
        };
    }

    private static List<SortParam> ParseSorts(IQueryCollection queryParams)
    {
        List<SortParam> sorts = [];
        int index = 0;

        while (true)
        {
            string? column = queryParams[$"sorts[{index}][column]"].FirstOrDefault();
            if (column is null)
            {
                break;
            }

            string? directionStr = queryParams[$"sorts[{index}][direction]"].FirstOrDefault();

            if (Enum.TryParse<SortDirection>(directionStr, ignoreCase: true, out SortDirection direction))
            {
                sorts.Add(new SortParam { Column = column, Direction = direction });
            }

            index++;
        }

        return sorts;
    }

    private static List<FilterParam> ParseFilters(IQueryCollection queryParams)
    {
        List<FilterParam> filters = [];
        int index = 0;

        while (true)
        {
            string? column = queryParams[$"filters[{index}][column]"].FirstOrDefault();
            if (column is null)
            {
                break;
            }

            string? operatorStr = queryParams[$"filters[{index}][operator]"].FirstOrDefault();
            string? value = queryParams[$"filters[{index}][value]"].FirstOrDefault();
            string? valueTo = queryParams[$"filters[{index}][valueTo]"].FirstOrDefault();

            if (Enum.TryParse<FilterOperator>(operatorStr, ignoreCase: true, out FilterOperator filterOp))
            {
                filters.Add(new FilterParam
                {
                    Column = column,
                    Operator = filterOp,
                    Value = value,
                    ValueTo = valueTo,
                });
            }

            index++;
        }

        return filters;
    }
}
