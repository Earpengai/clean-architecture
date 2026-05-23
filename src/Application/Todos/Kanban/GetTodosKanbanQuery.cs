using Application.Abstractions.Messaging;
using Application.Abstractions.Models;
using Domain.Todos;

namespace Application.Todos.Kanban;

public sealed record GetTodosKanbanQuery : IQuery<KanbanList<Priority, TodoCardResponse>>;
