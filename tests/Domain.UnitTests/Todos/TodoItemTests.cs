using Domain.Todos;
using SharedKernel;

namespace Domain.UnitTests.Todos;

public sealed class TodoItemTests
{
    [Fact]
    public void ShouldInheritFromEntity()
    {
        typeof(Entity).IsAssignableFrom(typeof(TodoItem)).ShouldBeTrue();
    }

    [Fact]
    public void Labels_ShouldDefaultToEmptyList()
    {
        TodoItem todoItem = new();

        todoItem.Labels.ShouldNotBeNull();
        todoItem.Labels.Count.ShouldBe(0);
    }

    [Fact]
    public void IsCompleted_ShouldDefaultToFalse()
    {
        TodoItem todoItem = new();

        todoItem.IsCompleted.ShouldBeFalse();
    }
}
