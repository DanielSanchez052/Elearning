using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Domain;

public class CourseEnrollmentEntityTests
{
    private static CourseEnrollment CreateEnrollmentWithCompletedLesson(out Guid lessonId)
    {
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), Guid.NewGuid());
        lessonId = Guid.NewGuid();
        var progress = UserLessonProgress.Create(enrollment.Id, lessonId);
        progress.MarkComplete();
        enrollment.LessonProgress.Add(progress);
        return enrollment;
    }

    [Fact]
    public void TryComplete_AllRequiredLessonsDone_CompletesAndReturnsTrue()
    {
        var enrollment = CreateEnrollmentWithCompletedLesson(out var lessonId);

        var completed = enrollment.TryComplete([lessonId]);

        Assert.True(completed);
        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
        Assert.NotNull(enrollment.CompletedAt);
    }

    [Fact]
    public void TryComplete_PendingRequiredLesson_ReturnsFalseAndStaysActive()
    {
        var enrollment = CreateEnrollmentWithCompletedLesson(out var lessonId);

        var completed = enrollment.TryComplete([lessonId, Guid.NewGuid()]);

        Assert.False(completed);
        Assert.True(enrollment.IsActive);
        Assert.Null(enrollment.CompletedAt);
    }

    [Fact]
    public void TryComplete_AlreadyCompleted_ReturnsFalseAndKeepsOriginalCompletedAt()
    {
        var enrollment = CreateEnrollmentWithCompletedLesson(out var lessonId);
        Assert.True(enrollment.TryComplete([lessonId]));
        var originalCompletedAt = enrollment.CompletedAt;

        Thread.Sleep(15);
        var secondCall = enrollment.TryComplete([lessonId]);

        Assert.False(secondCall);
        Assert.Equal(EnrollmentStatus.Completed, enrollment.Status);
        Assert.Equal(originalCompletedAt, enrollment.CompletedAt);
    }
}
