using Mechanics.Api.SwaggerExamples.Auth;
using Mechanics.Application.Auth;
using Mechanics.Application.Auth.Models.Request;
using Mechanics.Application.Auth.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Net;

namespace Mechanics.Api.Controllers
{
    /// <summary>
    /// Controller responsible for authentication operations such as user login.
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token upon successful login.
        /// </summary>
        /// <param name="request">The login request containing username and password.</param>
        /// <returns>Returns a JWT token if authentication is successful; otherwise, returns an error.</returns>
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authenticates a user and generates a JWT token upon successful login.",
            Description = "This endpoint verifies the user's credentials. If valid, it returns a signed JWT token that can be used to authorize further requests. " +
                          "If the credentials are invalid, a 400 or 404 error is returned depending on the nature of the failure."
        )]
        [ProducesResponseType(typeof(LoginResponse), (int)HttpStatusCode.OK)]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequestExample))]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
    }
}
