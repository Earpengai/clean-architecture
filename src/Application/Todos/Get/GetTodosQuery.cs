using Application.Abstractions.Messaging;
using Application.Abstractions.Models;

namespace Application.Todos.Get;

public sealed record GetTodosQuery : IQuery<PaginatedList<TodoResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public List<SortParam> Sorts { get; init; } = [];
    public List<FilterParam> Filters { get; init; } = [];
}
