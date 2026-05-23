using FluentValidation;

namespace Application.Todos.Update;

public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoCommandValidator()
    {
        RuleFor(c => c.TodoItemId).NotEmpty();
        RuleFor(c => c.Description).NotEmpty().MaximumLength(255);
        RuleFor(c => c.Priority!.Value).IsInEnum().When(c => c.Priority.HasValue);
    }
}
