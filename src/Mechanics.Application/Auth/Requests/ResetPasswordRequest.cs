namespace Mechanics.Application.Auth.Requests;

public class ResetPasswordRequest
{
    /// <summary>
    ///     O nome de usuário cadastrado.
    /// </summary>
    public required string UserName { get; init; }
}
