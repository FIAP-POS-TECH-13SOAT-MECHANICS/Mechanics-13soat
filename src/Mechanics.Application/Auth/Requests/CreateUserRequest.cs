using Mechanics.Application.Utils;
using MediatR;

namespace Mechanics.Application.Auth.Requests;

public class CreateUserRequest : IRequest<CreateItemResponse>
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
    ///     Perfil de acesso associado ao usuário.
    /// </summary>
    public required Guid RoleId { get; init; }
}
