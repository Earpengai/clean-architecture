using Application.Abstractions.Messaging;
using Domain.Todos;

namespace Application.Todos.Update;

public sealed record UpdateTodoCommand(
    Guid TodoItemId,
    string Description,
    Guid? ParentId,
    DateTime? DueDate,
    List<string>? Labels,
    Priority? Priority) : ICommand;
