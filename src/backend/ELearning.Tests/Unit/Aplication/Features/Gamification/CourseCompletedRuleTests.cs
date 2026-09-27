using ELearning.Application.Features.Gamification.Rules;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class CourseCompletedRuleTests
{
    [Fact]
    public void Evaluate_CompletedEnrollment_ProposesCourseDoneScopedToItsCourse()
    {
        var rule = new CourseCompletedRule();
        var enrollment = CourseEnrollment.Create(Guid.NewGuid(), Guid.NewGuid());

        var proposals = rule.Evaluate(enrollment);

        var proposal = Assert.Single(proposals);
        Assert.Equal(BadgeCode.CourseDone, proposal.Code);
        Assert.Equal(enrollment.CourseId, proposal.CourseId);
    }
}
