using Mechanics.Application.Auth.Models.Request;
using Mechanics.Application.Auth.Models.Response;

namespace Mechanics.Application.Auth;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
