using Mechanics.Application.Auth.Responses;
using MediatR;

namespace Mechanics.Application.Auth.Requests;

public class LoginRequest : IRequest<LoginResponse?>
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
