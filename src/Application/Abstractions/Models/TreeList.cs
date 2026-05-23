using SharedKernel;

namespace Application.Abstractions.Models;

public sealed record TreeList<T> where T : ITreeNode<T>
{
    public required List<T> Roots { get; init; }
}
