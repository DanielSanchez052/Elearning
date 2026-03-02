using ELearning.Application.Features.Auth.DTOs.User;

namespace ELearning.Application.Features.Auth.DTOs.AuthResponse;

public sealed record LoginResponseDto(
    string AccessToken,
    DateTime ExpiresAt,
    LoggedUserDto User
);

