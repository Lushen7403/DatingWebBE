namespace MatchLoveWeb.Services.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(int userId, string role);
    }
}
