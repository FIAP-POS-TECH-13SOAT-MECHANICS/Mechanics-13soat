namespace Mechanics.Application.Auth.Responses;

public record TokenResponse
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public required DateTime ExpirationDate { get; init; }
}
