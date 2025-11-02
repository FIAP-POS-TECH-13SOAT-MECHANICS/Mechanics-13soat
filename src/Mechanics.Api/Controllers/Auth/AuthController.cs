using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Application.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace Mechanics.Api.Controllers.Auth;

/// <summary>
///     Controller para gerenciar informações de login.
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "v1")]
[Route("api/auth")]
public class AuthController(AuthAppService service) : ControllerBase
{
    /// <summary>
    ///     Gera um token JWT para o usuário fornecido.
    /// </summary>
    /// <returns>Um <see cref="TokenResponse"/> contendo o token JWT.</returns>
    /// <response code="200">Usuário autenticado com sucesso.</response>
    /// <response code="401">Usuário ou senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [Consumes(typeof(LoginRequest), "application/json")]
    [Produces("application/json", Type = typeof(TokenResponse))]
    [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await service.Login(request, cancellationToken);
        return response is not null ? Ok(response) : Unauthorized();
    }

    /// <summary>
    ///     Gera um token JWT a partir de um refresh token.
    /// </summary>
    /// <returns>Um <see cref="TokenResponse"/> contendo o token JWT.</returns>
    /// <response code="200">Usuário autenticado com sucesso.</response>
    /// <response code="401">Token inválido.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [Consumes(typeof(RefreshTokenRequest), "application/json")]
    [Produces("application/json", Type = typeof(TokenResponse))]
    [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var response = await service.Refresh(request, cancellationToken);
        return response is not null ? Ok(response) : Unauthorized();
    }

    /// <summary>
    ///     Envia um código de criação de senha para o e-mail do usuário informado.
    /// </summary>
    /// <remarks>Por segurança é sempre retornado um código de sucesso, mesmo que o usuário informado seja inválido.</remarks>
    /// <response code="204">Resposta padrão.</response>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [Consumes(typeof(ResetPasswordRequest), "application/json")]
    [Produces("application/json", Type = typeof(ResetPasswordRequest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await service.ResetPassword(request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    ///     Cria uma senha para o usuário informado.
    /// </summary>
    /// <response code="204">Senha criada com sucesso.</response>
    /// <response code="401">Usuário ou código de criação de senha inválidos.</response>
    [AllowAnonymous]
    [HttpPost("create-password")]
    [Consumes(typeof(CreatePasswordRequest), "application/json")]
    [Produces("application/json", Type = typeof(CreatePasswordRequest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePassword(CreatePasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await service.CreatePassword(request, cancellationToken);
        return response is not null ? NoContent() : Unauthorized();
    }

    /// <summary>
    ///     Altera a senha do usuário atual.
    /// </summary>
    /// <response code="204">Senha alterada com sucesso.</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpPost("change-password")]
    [Consumes(typeof(ChangePasswordRequest), "application/json")]
    [Produces("application/json", Type = typeof(ChangePasswordRequest))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await service.ChangePassword(userId, request, cancellationToken);
        return NoContent();
    }
}
