using Domain.Todos;
using SharedKernel;

namespace Application.Todos.Tree;

public sealed class TodoTreeResponse : ITreeNode<TodoTreeResponse>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid? ParentId { get; init; }
    public string Description { get; init; } = string.Empty;
    public Priority Priority { get; init; }
    public List<string> Labels { get; init; } = [];
    public bool IsCompleted { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<TodoTreeResponse> Children { get; } = [];
}
