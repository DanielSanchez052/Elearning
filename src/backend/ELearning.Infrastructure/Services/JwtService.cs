using ELearning.Domain.Entities;
using ELearning.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ELearning.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresInMinutes;

    public JwtService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");

        _secretKey = jwt["SecretKey"] ?? throw new InvalidOperationException("Jwt:SecretKey no está configurado.");
        _issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no está configurado.");
        _audience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience no está configurado.");
        _expiresInMinutes = int.Parse(jwt["ExpiresInMinutes"] ?? "60");
    }

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_expiresInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name,  user.FullName),
            new Claim(ClaimTypes.Role,               user.Role.ToString().ToLowerInvariant()),
            new Claim("country_id",                  user.CountryId.ToString()),
            // Jti permite invalidar tokens individuales en el futuro si se implementa
            // una blacklist en Redis
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        throw new NotImplementedException();
    }
}
