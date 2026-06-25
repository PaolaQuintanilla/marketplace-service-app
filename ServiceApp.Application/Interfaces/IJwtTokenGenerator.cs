using ServiceApp.Domain.Entities;

namespace ServiceApp.Application.Interfaces;

public record TokenResult(string Token, DateTime ExpiresAt);

public interface IJwtTokenGenerator
{
    TokenResult GenerateToken(User user);
}
