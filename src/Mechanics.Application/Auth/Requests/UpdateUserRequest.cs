namespace Mechanics.Application.Auth.Requests;

public class UpdateUserRequest
{
    /// <summary>
    ///     Nome completo do usuário.
    /// </summary>
    public string? FullName { get; init; }

    /// <summary>
    ///     Login do usuário.
    /// </summary>
    public string? UserName { get; init; }

    /// <summary>
    ///     Perfil de acesso associado ao usuário.
    /// </summary>
    public Guid? RoleId { get; init; }
}
