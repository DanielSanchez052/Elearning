namespace ELearning.Application.Features.Countries.Dtos;

public sealed record CountryDto(
    int Id,
    string Code,
    string Name,
    bool IsActive
);