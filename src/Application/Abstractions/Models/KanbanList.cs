namespace Application.Abstractions.Models;

public sealed record KanbanList<TColumn, TItem>
{
    public required List<KanbanColumn<TColumn, TItem>> Columns { get; init; }
}

public sealed record KanbanColumn<TColumn, TItem>
{
    public required TColumn Key { get; init; }

    public required string Label { get; init; }

    public required int Order { get; init; }

    public required List<TItem> Items { get; init; }
}
