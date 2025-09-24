namespace Mechanics.Application.Auth.Requests;

public class LoginRequest
{
    /// <summary>
    ///     Nome de usuário.
    /// </summary>
    /// <example>admin</example>
    public required string Username { get; init; }

    /// <summary>
    ///     Senha da conta.
    /// </summary>
    /// <example>12345</example>
    public required string Password { get; init; }
}
