using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Generic;
using Mechanics.Domain.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Mechanics.Api.Controllers;

/// <summary>
///     Controller para realizar login.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.Administrator)]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Gera um token JWT para o usuário fornecido.
    /// </summary>
    /// <returns>Um <see cref="LoginResponse"/> contendo o token JWT.</returns>
    /// <remarks>A senha deve ser 12345.</remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes(typeof(LoginRequest), "application/json")]
    [Produces("application/json", Type = typeof(LoginResponse))]
    [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        var response = await mediator.Send(request);
        return response is not null ? Ok(response) : Unauthorized();
    }

    /// <summary>
    ///     Lista os perfis de acesso disponíveis.
    /// </summary>
    [HttpGet("roles")]
    [Produces("application/json", Type = typeof(GetRolesResponse))]
    [ProducesResponseType(typeof(GetRolesResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetRolesAsync()
    {
        var response = await mediator.Send(new GetRolesRequest());
        return Ok(response);
    }

    /// <summary>
    ///     Cria um novo usuário.
    /// </summary>
    [HttpPost("user")]
    [Consumes(typeof(CreateUserRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreateItemResponse))]
    [ProducesResponseType(typeof(CreateItemResponse), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest request)
    {
        var response = await mediator.Send(request);
        return StatusCode((int)HttpStatusCode.Created, response);
    }

    /// <summary>
    ///     Busca um usuário.
    /// </summary>
    [HttpPost("user/{id:guid}")]
    [Produces("application/json", Type = typeof(GetUserResponse))]
    [ProducesResponseType(typeof(GetUserResponse), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetUserAsync(Guid id)
    {
        var response = await mediator.Send(new GetUserRequest { Id = id });
        if (response is null)
            return NotFound();

        return Ok(response);
    }
}
