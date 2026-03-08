namespace ELearning.Application.Features.Users.DTOs;

public sealed record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Country,
    int CountryId,
    bool IsEmailVerified,
    DateTime CreatedAt
);
