using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Domain.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar perfis de acesso.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/auth/[controller]")]
[Authorize(Roles = RoleNames.Administrator)]
public class RolesController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Lista os perfis de acesso disponíveis.
    /// </summary>
    [HttpGet]
    [Produces("application/json", Type = typeof(GetRolesResponse))]
    [ProducesResponseType(typeof(GetRolesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetRolesRequest(), cancellationToken);
        return Ok(response);
    }
}
