using Mechanics.Application.Auth.Responses;
using MediatR;

namespace Mechanics.Application.Auth.Requests;

public class LoginRequest : IRequest<LoginResponse?>
{
    /// <summary>
    ///     Nome de usuário.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    ///     Senha da conta.
    /// </summary>
    public required string Password { get; init; }
}
