namespace SharedKernel;

public interface ITreeNode<T> where T : ITreeNode<T>
{
    Guid Id { get; }

    Guid? ParentId { get; }

    List<T> Children { get; }
}
