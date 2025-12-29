namespace Taxi_API.DTOs
{
    public record AuthRequest(string AuthSessionId, string Code, string? Name);
    public record RequestCodeRequest(string Phone);
    public record AuthResponse(string Token, string AuthSessionId);
}