using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
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
[AllowAnonymous]
public class LoginController(IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Gera um token JWT para o usuário fornecido.
    /// </summary>
    /// <returns>Um <see cref="LoginResponse"/> contendo o token JWT.</returns>
    /// <remarks>A senha deve ser 12345.</remarks>
    [HttpPost]
    [Consumes(typeof(LoginRequest), "application/json")]
    [Produces("application/json", Type = typeof(LoginResponse))]
    [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        var response = await mediator.Send(request);
        return response is not null ? Ok(response) : Unauthorized();
    }
}
