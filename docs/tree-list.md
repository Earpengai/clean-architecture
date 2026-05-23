# Tree List

Builds hierarchical data structures from flat database records. Suitable for nested categories, organizational charts, threaded comments, or any parent-child relationship stored in a single table.

---

## Models

### `ITreeNode<T>`

Namespace: `SharedKernel`

```csharp
public interface ITreeNode<T> where T : ITreeNode<T>
{
    Guid Id { get; }
    Guid? ParentId { get; }
    List<T> Children { get; }
}
```

The `Children` property is a `List<T>` backed by a get-only property. The tree-building algorithm populates children by calling `parent.Children.Add(child)` on the list instance — no setter needed.

### `TreeList<T>`

Namespace: `Application.Abstractions.Models`

```csharp
public sealed record TreeList<T> where T : ITreeNode<T>
{
    public required List<T> Roots { get; init; }
}
```

Only root nodes (items with no parent or whose parent is not in the dataset) appear in `Roots`. Descendants are nested inside their parents' `Children` list.

**Example JSON shape:**

```json
{
  "roots": [
    {
      "id": "guid-1",
      "parentId": null,
      "name": "Work",
      "children": [
        {
          "id": "guid-2",
          "parentId": "guid-1",
          "name": "Project Alpha",
          "children": [
            {
              "id": "guid-3",
              "parentId": "guid-2",
              "name": "Task 1",
              "children": []
            }
          ]
        }
      ]
    },
    {
      "id": "guid-4",
      "parentId": null,
      "name": "Personal",
      "children": []
    }
  ]
}
```

---

## DTO Setup

Your response DTO must implement `ITreeNode<T>`:

```csharp
public sealed class CategoryDto : ITreeNode<CategoryDto>
{
    public Guid Id { get; init; }
    public Guid? ParentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int SortOrder { get; init; }

    // Get-only property exposing the backing list.
    // The tree builder calls .Add() on this list instance.
    public List<CategoryDto> Children { get; } = [];
}
```

Key points:
- `Id` and `ParentId` must come from the database query (`.Select(...)`).
- `Children` is a get-only property with a field initializer `= []`. The property returns the same `List<CategoryDto>` instance, and `.Add(child)` mutates it.
- Your query must init `Children` to an empty list — the tree builder only adds to it.

---

## Extension Method

### `ToTreeListAsync`

```csharp
public static async Task<TreeList<T>> ToTreeListAsync<T>(
    this IQueryable<T> query,
    CancellationToken cancellationToken) where T : ITreeNode<T>
```

**Constraints**: `T` must implement `ITreeNode<T>`. The method uses `Id` and `ParentId` from the interface to build parent-child links.

**Algorithm**:
1. Materialize all items via `ToListAsync()`.
2. Build a dictionary: `Id → T`.
3. For each item:
   - If `ParentId` is non-null and the parent exists in the dictionary → add to `parent.Children`.
   - Otherwise → add to `Roots`.

**Orphans**: items whose `ParentId` references an ID not present in the dataset become root nodes. This prevents data loss when partial trees are fetched.

**Circular references**: not detected. Ensure your data has no cycles (parent referencing itself, or A → B → A). Use `.AsNoTracking()` in the query since the materialized entities are for read-only display.

---

## Full Handler Example

### Query

```csharp
public sealed record GetCategoriesTreeQuery : IQuery<TreeList<CategoryDto>>;
```

### Handler

```csharp
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Infrastructure.Extensions;
using SharedKernel;

internal sealed class GetCategoriesTreeQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetCategoriesTreeQuery, TreeList<CategoryDto>>
{
    public async Task<Result<TreeList<CategoryDto>>> Handle(
        GetCategoriesTreeQuery query,
        CancellationToken cancellationToken)
    {
        TreeList<CategoryDto> tree = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                ParentId = c.ParentId,
                Name = c.Name,
                SortOrder = c.SortOrder,
                // Children is auto-initialized to [] by the DTO definition
            })
            .ToTreeListAsync(cancellationToken);

        return tree;
    }
}
```

### Entity

```csharp
public sealed class Category : Entity
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public Guid TenantId { get; set; }
}
```

### Endpoint

```csharp
internal sealed class GetCategoriesTree : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("categories/tree", async (
            IQueryHandler<GetCategoriesTreeQuery, TreeList<CategoryDto>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetCategoriesTreeQuery();

            Result<TreeList<CategoryDto>> result =
                await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Tenants)
        .RequireAuthorization();
    }
}
```

---

## Combining Tree + Kanban + Pagination

These three list types compose with each other and with the filter/search/sort extensions. For example, a paginated tree:

```csharp
PaginatedList<CategoryDto> paginated = await context.Categories
    .AsNoTracking()
    .Where(c => c.TenantId == tenantId)
    .ApplySearch(query.Search, SearchMap)
    .ApplySort(query.Sorts, SortMap, c => c.SortOrder)
    .Select(c => new CategoryDto { ... })
    .ToPaginatedListAsync(query.Page, query.PageSize, cancellationToken);

// Apply tree building on the paginated items in memory
TreeList<CategoryDto> tree = new List<CategoryDto>(paginated.Items)
    .AsQueryable()
    .ToTreeListAsync(cancellationToken)  // works, but re-materializes unnecessarily
```

For better ergonomics, apply tree building directly on the materialized list:

```csharp
List<CategoryDto> items = paginated.Items;
Dictionary<Guid, CategoryDto> lookup = items.ToDictionary(i => i.Id);
List<CategoryDto> roots = [];
foreach (CategoryDto item in items)
{
    if (item.ParentId is { } pid && lookup.TryGetValue(pid, out CategoryDto? parent))
        parent.Children.Add(item);
    else
        roots.Add(item);
}
```

---

## Gotchas

- **In-memory execution**: the entire query result is materialized. Use `.Where()`, `.Take()`, or `ApplyFilters()` / `ApplySearch()` to limit the result set before calling `ToTreeListAsync`.
- **Children property**: must be `List<T>` with a getter that exposes the same instance. Record-based DTOs with `{ get; } = []` work perfectly.
- **Orphans become roots**: items referencing a missing parent are placed at root level. This is intentional — a partial tree fetch should not lose data.
- **Circular references**: not detected. If `A.ParentId = B.Id` and `B.ParentId = A.Id`, the algorithm processes one as a child of the other based on enumeration order, and the second becomes a root. Avoid cycles in your data.
- **Use `.AsNoTracking()`**: since the materialized entities are for display only, disable change tracking to reduce memory overhead.
- **Depth-first nesting**: children are nested arbitrarily deep with no depth limit. If your data has extreme depth, the JSON response may be large. The frontend should lazy-load deeply nested branches if needed.
