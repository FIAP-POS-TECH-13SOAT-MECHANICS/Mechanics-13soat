using Mechanics.Application.Auth.Models.Request;
using Swashbuckle.AspNetCore.Filters;

namespace Mechanics.Api.SwaggerExamples.Auth;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Username = "usuario@teste.com",
            Password = "Senha123!",
        };
    }
}
