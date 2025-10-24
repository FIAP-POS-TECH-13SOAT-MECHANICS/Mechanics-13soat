using Mechanics.Api;
using Mechanics.Application.Auth.Requests;
using Mechanics.Application.Auth.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
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
                new LoginRequest { UserName = roleName, Password = "5eCre+Key" });

            var content = await response.Content.ReadFromJsonAsync<TokenResponse>();
            _tokens[roleName] = content!.AccessToken;
            return content.AccessToken;
        }
    }
}
