namespace ELearning.Application.Common.Abstractions;

public sealed record PagedResult<T>(
 IReadOnlyList<T> Items,
 int TotalCount,
 int Page,
 int PageSize,
 int TotalPages
);
