# Paginated List

Server-side pagination, global search, column sorting, and column filtering — all chained directly on `IQueryable<T>` so the heavy lifting stays in SQL.

---

## Models

### `PaginatedList<T>`

Namespace: `Application.Abstractions.Models`

```csharp
public sealed record PaginatedList<T>
{
    public required List<T> Items { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
```

Computed properties (`TotalPages`, `HasPreviousPage`, `HasNextPage`) map directly to React pagination component props.

### `SortParam`

```csharp
public sealed record SortParam
{
    public required string Column { get; init; }       // DTO column name from the API
    public required SortDirection Direction { get; init; }
}
```

### `FilterParam`

```csharp
public sealed record FilterParam
{
    public required string Column { get; init; }      // DTO column name from the API
    public required FilterOperator Operator { get; init; }
    public string? Value { get; init; }               // primary value
    public string? ValueTo { get; init; }             // secondary value (for Between only)
}
```

### `SortDirection`

Namespace: `SharedKernel`

```csharp
public enum SortDirection { Asc = 0, Desc = 1 }
```

---

## Filter Operators

| Operator | Value | ValueTo | SQL Equivalent | Notes |
|----------|-------|---------|----------------|-------|
| `Equals` | required | — | `column = @value` | `Value: null` maps to `IS NULL` |
| `NotEquals` | required | — | `column != @value` | |
| `Contains` | required | — | `column LIKE '%@value%'` | string columns only |
| `StartsWith` | required | — | `column LIKE '@value%'` | string columns only |
| `EndsWith` | required | — | `column LIKE '%@value'` | string columns only |
| `GreaterThan` | required | — | `column > @value` | numeric/date columns |
| `GreaterThanOrEqual` | required | — | `column >= @value` | numeric/date columns |
| `LessThan` | required | — | `column < @value` | numeric/date columns |
| `LessThanOrEqual` | required | — | `column <= @value` | numeric/date columns |
| `Between` | required | required | `column >= @value AND column <= @valueTo` | inclusive range |
| `IsNull` | ignored | — | `column IS NULL` | |
| `IsNotNull` | ignored | — | `column IS NOT NULL` | |

---

## Extension Methods

All methods are in `Infrastructure.Extensions.PaginatedQueryExtensions`.

### `ApplySearch`

```csharp
public static IQueryable<T> ApplySearch<T>(
    this IQueryable<T> query,
    string? searchTerm,
    IReadOnlyDictionary<string, Expression<Func<T, string>>> searchMap)
```

OR-connects `EF.Functions.Like` across every column in the `searchMap`. Wildcards `%`, `_`, and `\` in the search term are escaped automatically. Returns the query unchanged if `searchTerm` is null/empty or the map is empty.

### `ApplySort`

```csharp
public static IQueryable<T> ApplySort<T>(
    this IQueryable<T> query,
    IReadOnlyList<SortParam> sorts,
    IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
    Expression<Func<T, object>>? defaultSort = null,
    SortDirection defaultDirection = SortDirection.Desc)
```

Builds an `OrderBy` / `ThenBy` chain. The first sort column uses `OrderBy`; all subsequent use `ThenBy` (stable multi-column sort). Falls back to `defaultSort` when no user sorts are applied.

### `ApplyFilters`

```csharp
public static IQueryable<T> ApplyFilters<T>(
    this IQueryable<T> query,
    IReadOnlyList<FilterParam> filters,
    IReadOnlyDictionary<string, Expression<Func<T, object>>> filterMap)
```

AND-connects all filters into a `WHERE` clause. Each filter is applied as a separate `.Where()` call. Filters whose column is not in the map are silently skipped.

### `ToPaginatedListAsync`

```csharp
public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
    this IQueryable<T> query,
    int page,
    int pageSize,
    CancellationToken cancellationToken)
```

Executes `COUNT(*)` for total, then `Skip((page-1)*pageSize).Take(pageSize)` for the page. Returns a `PaginatedList<T>` with all computed properties populated.

---

## Map Conventions

Each extension method accepts a dictionary that maps **DTO column names** (strings from the API) to **EF expression trees** on the source entity.

```csharp
// The T in all maps refers to the EF entity, not the response DTO.
// All expressions run at the SQL level on the database table.

IReadOnlyDictionary<string, Expression<Func<TodoItem, string>>> searchMap;
IReadOnlyDictionary<string, Expression<Func<TodoItem, object>>> sortMap;
IReadOnlyDictionary<string, Expression<Func<TodoItem, object>>> filterMap;
```

**Joined fields** work natively — EF Core translates navigation property accessors into SQL JOINs:

```csharp
["userName"] = t => t.User.FirstName        // EF generates INNER JOIN users
["tenantName"] = t => t.Tenant.Name          // EF generates INNER JOIN tenants
```

**Non-DB computed fields** (e.g. `FullName`, `HasOverdue`) — omit them from the maps. The handler applies those after materialization, or the frontend computes them client-side. Columns not in the map are silently skipped; no exceptions.

**Consistent naming** — the key in the map must match the column name sent from the frontend (typically the JSON property name on the response DTO):

```csharp
// Frontend sends: { column: "description", direction: "Asc" }
// Map must match:
["description"] = t => t.Description    // ✓
["Description"] = t => t.Description    // ✗ won't match
```

---

## Pipeline Order

The recommended chain order:

```csharp
query
    .Where(basePredicate)           // Step 1: tenant scoping, ownership, hard filters
    .ApplyFilters(filters, map)    // Step 2: user column filters (AND logic)
    .ApplySearch(searchTerm, map)  // Step 3: global search (OR across columns)
    .ApplySort(sorts, map, t => t.CreatedAt)  // Step 4: dynamic ordering
    .ToPaginatedListAsync(page, pageSize, ct); // Step 5: count + page
```

Search after filters means the user can narrow results via column filters first, then text-search within that subset.

---

## Full Handler Example

### Query

```csharp
public sealed record GetTodosQuery : IQuery<PaginatedList<TodoResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public List<SortParam> Sorts { get; init; } = [];
    public List<FilterParam> Filters { get; init; } = [];
}
```

### Handler

```csharp
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Infrastructure.Extensions;
using SharedKernel;

internal sealed class GetTodosQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
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
            .Where(t => t.UserId == userContext.UserId
                        && t.TenantId == userContext.TenantId!.Value)
            .ApplyFilters(query.Filters, FilterMap)
            .ApplySearch(query.Search, SearchMap)
            .ApplySort(query.Sorts, SortMap, t => t.CreatedAt)
            .Select(t => new TodoResponse
            {
                Id = t.Id,
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
```

### Endpoint

```csharp
internal sealed class GetTodos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("todos", async (
            [AsParameters] GetTodosQuery query,
            IQueryHandler<GetTodosQuery, PaginatedList<TodoResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<PaginatedList<TodoResponse>> result =
                await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Todos)
        .RequireAuthorization();
    }
}
```

The frontend sends query string parameters:

```
GET /api/v1/todos?page=1&pageSize=20&search=groceries&sorts[0][column]=dueDate&sorts[0][direction]=Asc&filters[0][column]=isCompleted&filters[0][operator]=Equals&filters[0][value]=false
```

---

## Gotchas

- **Search escaping**: `%`, `_`, and `\` in the search term are escaped automatically with `\` (PostgreSQL default escape character). No manual sanitization needed.
- **Null Equals**: `{ Operator: Equals, Value: null }` is treated as `IS NULL`. Use `IsNull` operator for clarity.
- **AND vs OR**: Filters are ANDed together (`a > 1 AND b = 'x'`). Search conditions are ORed (`a LIKE '%x%' OR b LIKE '%x%'`). The combined WHERE is: `(base filters) AND (column filters) AND (search_a OR search_b)`.
- **String operators on non-string columns**: `Contains`, `StartsWith`, `EndsWith` are silently skipped if the column type is not `string`.
- **Between requires both values**: if `Value` or `ValueTo` fails to parse, the filter is silently skipped.
- **No validation of page/pageSize**: the handler should validate (e.g., clamp page to >= 1, cap pageSize to 100). The extension will execute `Skip(negative)` if page = 0, producing unexpected results.
- **Count and page are two separate queries**: totals may shift between requests if data changes concurrently.
