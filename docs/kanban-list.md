# Kanban List

Groups flat query results into columns for kanban boards, drag-and-drop UIs, or any column-based layout. The extension method materializes data first, then groups in memory — suitable for manageable datasets like a user's tasks or a project board.

---

## Models

### `KanbanList<TColumn, TItem>`

Namespace: `Application.Abstractions.Models`

```csharp
public sealed record KanbanList<TColumn, TItem>
{
    public required List<KanbanColumn<TColumn, TItem>> Columns { get; init; }
}
```

### `KanbanColumn<TColumn, TItem>`

```csharp
public sealed record KanbanColumn<TColumn, TItem>
{
    public required TColumn Key { get; init; }      // the grouping key (enum, string, etc.)
    public required string Label { get; init; }      // display label for the column header
    public required int Order { get; init; }          // column sort position
    public required List<TItem> Items { get; init; } // items in this column
}
```

### Design rationale

`TColumn` and `TItem` are independent generics so the column key can be a different type than the items. A common pattern: `TColumn` is an enum (e.g. `Priority`), `TItem` is a lightweight card DTO (e.g. `TodoCardResponse`). The columns are ordered by `Order`.

This maps directly to React kanban libraries like `@hello-pangea/dnd`, `react-beautiful-dnd`, or `react-kanban`:

```json
{
  "columns": [
    {
      "key": "Top",
      "label": "Top Priority",
      "order": 0,
      "items": [ { "id": "...", "title": "Fix login bug", "labels": ["urgent"] } ]
    },
    {
      "key": "Medium",
      "label": "Medium",
      "order": 2,
      "items": [ { "id": "...", "title": "Update README", "labels": [] } ]
    }
  ]
}
```

---

## Extension Method

### `ToKanbanListAsync`

```csharp
public static async Task<KanbanList<TColumn, TItem>> ToKanbanListAsync<T, TColumn, TItem>(
    this IQueryable<T> query,
    Func<T, TColumn> columnKeySelector,
    Func<T, TItem> itemSelector,
    Func<TColumn, string> columnLabelSelector,
    Func<TColumn, int> columnOrderSelector,
    CancellationToken cancellationToken)
```

| Parameter | Purpose |
|-----------|---------|
| `columnKeySelector` | Groups source entities by this key |
| `itemSelector` | Maps each source entity to the card/item DTO |
| `columnLabelSelector` | Produces the display label for each column |
| `columnOrderSelector` | Assigns a sort position to each column |

All four selectors are `Func` delegates — they run on in-memory data after `ToListAsync()` materializes the query. Columns are returned sorted ascending by `Order`.

Columns whose key yields zero items after grouping **do not appear** in the result. To show empty columns, prepopulate the `KanbanList` with all expected columns and merge the items in.

---

## Full Handler Example

### Card DTO

```csharp
public sealed record TodoCardResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<string> Labels { get; init; } = [];
    public Priority Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
```

### Query

```csharp
public sealed record GetTodosKanbanQuery : IQuery<KanbanList<Priority, TodoCardResponse>>
{
    // optional: add filters like UserId via IUserContext in handler
}
```

### Handler

```csharp
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Domain.Todos;
using Infrastructure.Extensions;
using SharedKernel;

internal sealed class GetTodosKanbanQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTodosKanbanQuery, KanbanList<Priority, TodoCardResponse>>
{
    public async Task<Result<KanbanList<Priority, TodoCardResponse>>> Handle(
        GetTodosKanbanQuery query,
        CancellationToken cancellationToken)
    {
        KanbanList<Priority, TodoCardResponse> board = await context.TodoItems
            .Where(t => t.UserId == userContext.UserId
                        && t.TenantId == userContext.TenantId!.Value)
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
```

### Endpoint

```csharp
internal sealed class GetTodosKanban : IEndpoint
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
```

### Progress-based Kanban (by status)

You can group by any property or derived value. A common use case is progress status columns:

```csharp
.ToKanbanListAsync(
    columnKeySelector: t => t.IsCompleted
        ? "Done"
        : t.CreatedAt < DateTime.UtcNow.AddDays(-7) ? "Overdue"
        : t.DueDate is not null ? "In Progress"
        : "Backlog",
    itemSelector: t => new TodoCardResponse { ... },
    columnLabelSelector: key => key,
    columnOrderSelector: key => key switch
    {
        "Backlog" => 0,
        "In Progress" => 1,
        "Overdue" => 2,
        "Done" => 3,
        _ => 99
    },
    cancellationToken);
```

---

## Gotchas

- **In-memory execution**: the entire query result is materialized via `ToListAsync()` before grouping. For datasets with thousands of rows, consider applying `ApplyFilters` first or limiting with `.Take(N)`.
- **Empty columns**: columns whose key produces zero items are absent from the result. To show empty columns in the UI, prepopulate a full column set and merge the server response.
- **Func delegates, not Expression**: the selectors are `Func`, not `Expression<Func>`. They run on in-memory objects, not in SQL. This means you can use complex C# logic in selectors — but the initial query must be optimized via `.Where()`, `.ApplyFilters()`, or `.ApplySearch()` before calling `ToKanbanListAsync`.
- **Column order**: columns are sorted by `columnOrderSelector` ascending. If multiple columns share the same order value, their relative position is non-deterministic.
