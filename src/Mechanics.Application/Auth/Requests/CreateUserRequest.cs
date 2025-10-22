using System.ComponentModel.DataAnnotations;

namespace Mechanics.Application.Auth.Requests;

public class CreateUserRequest
{
    /// <summary>
    ///     Nome completo do usuário.
    /// </summary>
    public required string FullName { get; init; }

    /// <summary>
    ///     Login do usuário.
    /// </summary>
    public required string UserName { get; init; }

    /// <summary>
    ///     E-mail do usuário.
    /// </summary>
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>
    ///     Perfil de acesso associado ao usuário.
    /// </summary>
    public required Guid RoleId { get; init; }
}
