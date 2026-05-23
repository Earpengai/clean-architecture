using Domain.Todos;

namespace Application.Todos.Kanban;

public sealed record TodoCardResponse
{
    public Guid Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public List<string> Labels { get; init; } = [];
    public Priority Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
