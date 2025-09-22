namespace Mechanics.Application.Options;

public class JwtOptions
{
    /// <summary>
    ///     Chave de assinatura do token JWT.
    /// </summary>
    public required string SecretKey { get; init; }

    /// <summary>
    ///     Validade do token em minutos.
    /// </summary>
    public required int AccessTokenLifetime { get; init; }
}
