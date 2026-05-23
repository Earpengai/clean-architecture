namespace Application.Abstractions.Models;

public enum FilterOperator
{
    Equals = 0,
    NotEquals = 1,
    Contains = 2,
    StartsWith = 3,
    EndsWith = 4,
    GreaterThan = 5,
    GreaterThanOrEqual = 6,
    LessThan = 7,
    LessThanOrEqual = 8,
    Between = 9,
    IsNull = 10,
    IsNotNull = 11
}
