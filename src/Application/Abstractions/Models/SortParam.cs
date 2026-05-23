using SharedKernel;

namespace Application.Abstractions.Models;

public sealed record SortParam
{
    public required string Column { get; init; }

    public required SortDirection Direction { get; init; }
}
