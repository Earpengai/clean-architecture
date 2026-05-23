namespace Application.Abstractions.Models;

public sealed record FilterParam
{
    public required string Column { get; init; }

    public required FilterOperator Operator { get; init; }

    public string? Value { get; init; }

    public string? ValueTo { get; init; }
}
