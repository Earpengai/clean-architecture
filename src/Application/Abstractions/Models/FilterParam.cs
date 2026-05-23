namespace Application.Abstractions.Models;

public sealed record FilterParam
{
    public string Column { get; set; } = string.Empty;

    public FilterOperator Operator { get; set; }

    public string? Value { get; set; }

    public string? ValueTo { get; set; }
}
