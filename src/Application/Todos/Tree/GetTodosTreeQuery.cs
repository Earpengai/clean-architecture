using Application.Abstractions.Messaging;
using Application.Abstractions.Models;

namespace Application.Todos.Tree;

public sealed record GetTodosTreeQuery : IQuery<TreeList<TodoTreeResponse>>;
