using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Todos;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Todos.Update;

internal sealed class UpdateTodoCommandHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : ICommandHandler<UpdateTodoCommand>
{
    public async Task<Result> Handle(UpdateTodoCommand command, CancellationToken cancellationToken)
    {
        TodoItem? todoItem = await context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == command.TodoItemId
                && t.TenantId == userContext.TenantId!.Value, cancellationToken);

        if (todoItem is null)
        {
            return Result.Failure(TodoItemErrors.NotFound(command.TodoItemId));
        }

        todoItem.Description = command.Description;

        if (command.ParentId is not null)
        {
            todoItem.ParentId = command.ParentId;
        }

        if (command.DueDate is not null)
        {
            todoItem.DueDate = command.DueDate;
        }

        if (command.Labels is not null)
        {
            todoItem.Labels = command.Labels;
        }

        if (command.Priority is not null)
        {
            todoItem.Priority = command.Priority.Value;
        }

        todoItem.Raise(new TodoItemUpdatedDomainEvent(todoItem.Id));

        await context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
