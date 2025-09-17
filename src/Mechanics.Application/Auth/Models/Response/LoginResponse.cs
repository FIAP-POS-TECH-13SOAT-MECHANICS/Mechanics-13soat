namespace Mechanics.Application.Auth.Models.Response
{
    public record LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
