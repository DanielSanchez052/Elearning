using ELearning.Domain.Entities;

namespace ELearning.Tests.Unit.Domain;

public class ExamSessionEntityTests
{
    [Fact]
    public void Start_SetsIdentityFieldsAndStartedAtNow()
    {
        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var session = ExamSession.Start(userId, courseId, 2);

        var after = DateTime.UtcNow;
        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(courseId, session.CourseId);
        Assert.Equal(2, session.AttemptNumber);
        Assert.InRange(session.StartedAt, before, after);
    }

    [Fact]
    public void Start_NewSession_IsOpenAndNotSubmitted()
    {
        var session = ExamSession.Start(Guid.NewGuid(), Guid.NewGuid(), 1);

        Assert.True(session.IsOpen);
        Assert.Null(session.SubmittedAt);
    }

    [Fact]
    public void MarkSubmitted_SetsSubmittedAtAndClosesSession()
    {
        var session = ExamSession.Start(Guid.NewGuid(), Guid.NewGuid(), 1);
        var before = DateTime.UtcNow;

        session.MarkSubmitted();

        Assert.False(session.IsOpen);
        Assert.NotNull(session.SubmittedAt);
        Assert.InRange(session.SubmittedAt!.Value, before, DateTime.UtcNow);
    }

    [Fact]
    public void MarkSubmitted_CalledTwice_KeepsFirstSubmittedAt()
    {
        var session = ExamSession.Start(Guid.NewGuid(), Guid.NewGuid(), 1);
        session.MarkSubmitted();
        var firstSubmittedAt = session.SubmittedAt;

        Thread.Sleep(15);
        session.MarkSubmitted();

        Assert.Equal(firstSubmittedAt, session.SubmittedAt);
        Assert.False(session.IsOpen);
    }

    [Fact]
    public void MarkSubmitted_DoesNotChangeStartedAt()
    {
        var session = ExamSession.Start(Guid.NewGuid(), Guid.NewGuid(), 1);
        var startedAt = session.StartedAt;

        session.MarkSubmitted();

        Assert.Equal(startedAt, session.StartedAt);
    }
}
