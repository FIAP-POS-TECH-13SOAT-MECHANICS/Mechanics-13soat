using Mechanics.Application.Auth.Responses;
using Mechanics.Domain.Auth;

namespace Mechanics.Application.Utils.TokenGenerator;

public interface IJwtTokenHandler
{
    Guid? GetUserId(string token);
    Task<bool> ValidateRefreshToken(string refreshToken, string securityStamp);
    TokenResponse CreateTokenResponse(User user);
}
