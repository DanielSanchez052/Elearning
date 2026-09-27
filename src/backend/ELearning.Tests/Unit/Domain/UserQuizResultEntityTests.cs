using ELearning.Domain.Entities;

namespace ELearning.Tests.Unit.Domain;

public class UserQuizResultEntityTests
{
    [Fact]
    public void Create_WithoutStartedAt_StartedAtAndDurationAreNull()
    {
        var result = UserQuizResult.Create(Guid.NewGuid(), null, Guid.NewGuid(), 1, 100m, 70m);

        Assert.Null(result.StartedAt);
        Assert.Null(result.Duration);
    }

    [Fact]
    public void Create_WithStartedAt_StoresStartedAt()
    {
        var startedAt = DateTime.UtcNow.AddMinutes(-5);

        var result = UserQuizResult.Create(Guid.NewGuid(), null, Guid.NewGuid(), 1, 100m, 70m, startedAt);

        Assert.Equal(startedAt, result.StartedAt);
    }

    [Fact]
    public void Duration_WithStartedAt_IsCompletedAtMinusStartedAt()
    {
        var startedAt = DateTime.UtcNow.AddMinutes(-5);

        var result = UserQuizResult.Create(Guid.NewGuid(), null, Guid.NewGuid(), 1, 100m, 70m, startedAt);

        Assert.NotNull(result.Duration);
        Assert.Equal(result.CompletedAt - startedAt, result.Duration!.Value);
        Assert.InRange(result.Duration.Value, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ExistingSixArgumentOverload_StillComputesIsPassed()
    {
        var passed = UserQuizResult.Create(Guid.NewGuid(), Guid.NewGuid(), null, 1, 70m, 70m);
        var failed = UserQuizResult.Create(Guid.NewGuid(), Guid.NewGuid(), null, 1, 69m, 70m);

        Assert.True(passed.IsPassed);
        Assert.False(failed.IsPassed);
        Assert.Null(passed.StartedAt);
    }
}
