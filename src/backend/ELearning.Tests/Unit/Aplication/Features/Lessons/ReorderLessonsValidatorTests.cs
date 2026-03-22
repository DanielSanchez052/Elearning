using ELearning.Application.Features.Lessons.Commands;
using ELearning.Application.Features.Lessons.Validators;

namespace ELearning.Tests.Unit.Aplication.Features.Lessons;

public class ReorderLessonsValidatorTests
{
    private readonly ReorderLessonsValidator _validator = new();

    private static ReorderLessonsCommand Valid() =>
        new(
            CourseId: Guid.NewGuid(),
            Orders: [
                new LessonOrderItem(Guid.NewGuid(), 1),
                new LessonOrderItem(Guid.NewGuid(), 2)
            ],
            RequesterId: Guid.NewGuid(),
            RequesterRole: "instructor");

    [Fact]
    public void Validate_EmptyCourseId_HasCourseIdError()
    {
        var result = _validator.Validate(Valid() with { CourseId = Guid.Empty });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderLessonsCommand.CourseId));
    }

    [Fact]
    public void Validate_NullOrders_HasOrdersError()
    {
        var result = _validator.Validate(Valid() with { Orders = null! });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderLessonsCommand.Orders));
    }

    [Fact]
    public void Validate_EmptyOrders_HasOrdersError()
    {
        var result = _validator.Validate(Valid() with { Orders = [] });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderLessonsCommand.Orders));
    }

    [Fact]
    public void Validate_OrderIndexBelowOne_HasOrdersError()
    {
        var result = _validator.Validate(Valid() with
        {
            Orders = [new LessonOrderItem(Guid.NewGuid(), 0)]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderLessonsCommand.Orders));
    }

    [Fact]
    public void Validate_DuplicateOrderIndexes_HasOrdersError()
    {
        var result = _validator.Validate(Valid() with
        {
            Orders = [
                new LessonOrderItem(Guid.NewGuid(), 1),
                new LessonOrderItem(Guid.NewGuid(), 1)
            ]
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderLessonsCommand.Orders));
    }

    [Fact]
    public void Validate_ValidCommand_IsValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
