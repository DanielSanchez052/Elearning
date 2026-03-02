namespace ELearning.Application.Features.Auth.DTOs.User;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Country,
    int CountryId,
    DateTime CreatedAt,
    int LoginStreak
);
