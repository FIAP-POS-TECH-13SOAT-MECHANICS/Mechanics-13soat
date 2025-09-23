using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar informações de login.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Gera um token JWT para o usuário fornecido.
    /// </summary>
    /// <returns>Um <see cref="LoginResponse"/> contendo o token JWT.</returns>
    /// <remarks>A senha deve ser 12345.</remarks>
    /// <response code="200">Usuário autenticado com sucesso.</response>
    /// <response code="401">Usuário ou senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes(typeof(LoginRequest), "application/json")]
    [Produces("application/json", Type = typeof(LoginResponse))]
    [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        var response = await mediator.Send(request);
        return response is not null ? Ok(response) : Unauthorized();
    }
}
