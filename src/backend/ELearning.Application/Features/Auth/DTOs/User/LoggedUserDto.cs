namespace ELearning.Application.Features.Auth.DTOs.User;

public sealed record LoggedUserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Country
);

