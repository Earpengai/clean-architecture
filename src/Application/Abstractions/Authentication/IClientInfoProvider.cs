namespace Application.Abstractions.Authentication;

public interface IClientInfoProvider
{
    string? IpAddress { get; }
    string? UserAgent { get; }
}
