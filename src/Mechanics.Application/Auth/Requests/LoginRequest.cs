namespace Mechanics.Application.Auth.Requests;

public class LoginRequest
{
    /// <summary>
    ///     Nome de usuário.
    /// </summary>
    /// <example>administrator</example>
    public required string UserName { get; init; }

    /// <summary>
    ///     Senha da conta.
    /// </summary>
    /// <example>5eCre+Key</example>
    public required string Password { get; init; }
}
