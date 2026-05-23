using SharedKernel;

namespace Application.Abstractions.Models;

public sealed record SortParam
{
    public string Column { get; set; } = string.Empty;

    public SortDirection Direction { get; set; }
}
