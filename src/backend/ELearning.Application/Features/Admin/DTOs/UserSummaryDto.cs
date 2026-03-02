namespace ELearning.Application.Features.Admin.DTOs;

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
