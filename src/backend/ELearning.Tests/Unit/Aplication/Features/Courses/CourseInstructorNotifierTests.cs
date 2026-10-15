using ELearning.Application.Features.Courses.Services;
using ELearning.Application.Features.Notifications.Messages;
using ELearning.Application.Features.Notifications.Services;
using ELearning.Domain.Entities;
using Moq;

namespace ELearning.Tests.Unit.Aplication.Features.Courses;

public class CourseInstructorNotifierTests
{
    private readonly Mock<INotificationPublisher> _publisherMock = new();
    private readonly CourseInstructorNotifier _notifier;
    private readonly Course _course = Course.Create("Excel Avanzado", null, null, Guid.NewGuid(), isGlobal: false);

    public CourseInstructorNotifierTests() =>
        _notifier = new CourseInstructorNotifier(_publisherMock.Object);

    private void VerifyPublished<TMessage>(Guid recipientId, Times times) where TMessage : NotificationMessage =>
        _publisherMock.Verify(p => p.PublishAsync(
            recipientId, It.Is<TMessage>(m => m.ReferenceId == _course.Id), It.IsAny<CancellationToken>()), times);

    [Fact]
    public async Task InstructorReplaced_NotifiesTheOneLeavingAndTheOneArriving()
    {
        var previous = Guid.NewGuid();
        var next = Guid.NewGuid();

        await _notifier.InstructorChangedAsync(_course, previous, next);

        VerifyPublished<CourseUnassignedMessage>(previous, Times.Once());
        VerifyPublished<CourseAssignedMessage>(next, Times.Once());
        _publisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task FirstAssignment_OnlyNotifiesTheNewInstructor()
    {
        var next = Guid.NewGuid();

        await _notifier.InstructorChangedAsync(_course, null, next);

        VerifyPublished<CourseAssignedMessage>(next, Times.Once());
        _publisherMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InstructorRemoved_OnlyNotifiesThePreviousInstructor()
    {
        var previous = Guid.NewGuid();

        await _notifier.InstructorChangedAsync(_course, previous, null);

        VerifyPublished<CourseUnassignedMessage>(previous, Times.Once());
        _publisherMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NoChange_NotifiesNobody(bool hasInstructor)
    {
        var instructorId = hasInstructor ? Guid.NewGuid() : (Guid?)null;

        await _notifier.InstructorChangedAsync(_course, instructorId, instructorId);

        _publisherMock.VerifyNoOtherCalls();
    }
}
