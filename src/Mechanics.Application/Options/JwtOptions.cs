namespace Mechanics.Application.Options;

public class JwtOptions
{
    public required string SecretKey { get; init; }
    public required int AccessTokenLifetime { get; init; }
}
