using Application.Todos.Update;
using Domain.Todos;
using FluentValidation.TestHelper;

namespace Application.UnitTests.Todos.Update;

public sealed class UpdateTodoCommandValidatorTests
{
    private readonly UpdateTodoCommandValidator _validator = new();

    [Fact]
    public void ShouldHaveError_WhenTodoItemIdIsEmpty()
    {
        var command = new UpdateTodoCommand(Guid.Empty, "Description", null, null, null, null);

        TestValidationResult<UpdateTodoCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.TodoItemId);
    }

    [Fact]
    public void ShouldHaveError_WhenDescriptionIsEmpty()
    {
        var command = new UpdateTodoCommand(Guid.NewGuid(), "", null, null, null, null);

        TestValidationResult<UpdateTodoCommand> result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description);
    }

    [Fact]
    public void ShouldHaveError_WhenPriorityIsOutOfRange()
    {
        var command = new UpdateTodoCommand(Guid.NewGuid(), "Description", null, null, null, (Priority)(-1));

        TestValidationResult<UpdateTodoCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void ShouldNotHaveError_WhenAllFieldsValid()
    {
        var command = new UpdateTodoCommand(Guid.NewGuid(), "Valid description", null, null, null, null);

        TestValidationResult<UpdateTodoCommand> result = _validator.TestValidate(command);

        result.IsValid.ShouldBeTrue();
    }
}
