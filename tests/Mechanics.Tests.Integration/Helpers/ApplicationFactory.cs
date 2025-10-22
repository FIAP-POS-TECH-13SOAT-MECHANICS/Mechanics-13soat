using Mechanics.Api;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Mechanics.Domain.Auth;
using Mechanics.Infra.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Mechanics.Tests.Integration.Helpers;

public class ApplicationFactory : WebApplicationFactory<Program>
{
    private readonly ConcurrentDictionary<string, string?> _tokens = new();

    public async Task<HttpClient> GetAuthenticatedClient(string roleName)
    {
        var authenticatedClient = CreateClient();
        var token = _tokens.GetOrAdd(roleName, await GetToken());
        authenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return authenticatedClient;

        async Task<string> GetToken()
        {
            var client = CreateClient();
            var response = await client.PostAsJsonAsync("api/auth/login",
                new LoginRequest { UserName = roleName, Password = "12345" });

            var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
            _tokens[roleName] = content!.AccessToken;
            return content.AccessToken;
        }
    }
}

public static class ApplicationFactoryExtensions
{
    public static async Task AddTestUsers(this ApplicationFactory factory, CancellationToken cancellationToken)
    {
        using var scopedProvider = factory.Services.CreateScope();
        var context = scopedProvider.ServiceProvider.GetRequiredService<AppDbContext>();

        var roles = await context.Roles.ToListAsync(cancellationToken: cancellationToken);

        context.Users.AddRange(
            roles.Select(role => new User
            {
                FullName = role.Name,
                UserName = role.Name.ToLower(),
                RoleId = role.Id,
                Email = $"{role.Name.ToLower()}@mechanics.com",
                PasswordHash = "",
                SecurityStamp = role.Name,
            }));

        await context.SaveChangesAsync(cancellationToken);
    }
}
