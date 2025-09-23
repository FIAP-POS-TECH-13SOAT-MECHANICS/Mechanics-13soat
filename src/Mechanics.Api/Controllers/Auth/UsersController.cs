using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Generic;
using Mechanics.Domain.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar cadastros de usuários.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/auth/[controller]")]
[Authorize(Roles = RoleNames.Administrator)]
public class UsersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Cria um novo usuário.
    /// </summary>
    /// <response code="201">Registro cadastrado.</response>
    /// <response code="400">Registro inválido.</response>
    [HttpPost]
    [Consumes(typeof(CreateUserRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(request, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = response.CreatedId }, response);
    }

    /// <summary>
    ///     Busca um usuário.
    /// </summary>
    /// <response code="200">Registro encontrado.</response>
    /// <response code="404">Registro não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Produces("application/json", Type = typeof(GetUserResponse))]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await mediator.Send(new GetUserRequest { Id = id }, cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }
}
