namespace Application.Common.Interfaces;

public interface IJwtService
{
    (string accessToken, string refreshToken, DateTime expiresAt) GenerateTokens(int userId, string email, IList<string> roles);
    Task RevokeRefreshTokensAsync(int userId);
}
