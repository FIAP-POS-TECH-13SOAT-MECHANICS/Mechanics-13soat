namespace Mechanics.Application.Options;

public class JwtOptions
{
    /// <summary>
    ///     Chave privada do token JWT.
    /// </summary>
    public required string PrivateKey { get; init; }

    /// <summary>
    ///     Chave pública do token JWT.
    /// </summary>
    public required string PublicKey { get; init; }

    /// <summary>
    ///     Validade do token em minutos.
    /// </summary>
    public required int AccessTokenLifetime { get; init; }

    /// <summary>
    ///     Validade do token em minutos.
    /// </summary>
    public required int RefreshTokenLifetime { get; init; }
}
