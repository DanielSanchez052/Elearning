using ELearning.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace ELearning.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(string.Empty, password);

    public bool Verify(string hash, string password) =>
        _hasher.VerifyHashedPassword(string.Empty, hash, password)
            != PasswordVerificationResult.Failed;
}
