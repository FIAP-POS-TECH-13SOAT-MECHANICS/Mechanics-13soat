namespace Mechanics.Application.Auth.Requests;

public class CreatePasswordRequest
{
    /// <summary>
    ///     O login do usuário.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    ///     O código para criar uma nova senha, enviado por e-mail.
    /// </summary>
    public required string PasswordCreationCode { get; init; }

    /// <summary>
    ///     A nova senha.
    /// </summary>
    public required string Password { get; init; }
}
