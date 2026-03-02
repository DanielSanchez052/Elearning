using ELearning.Domain.Entities;

namespace ELearning.Domain.Interfaces.Services;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken(); // TODO: queda para una fase posterior, no es necesario por ahora

}
