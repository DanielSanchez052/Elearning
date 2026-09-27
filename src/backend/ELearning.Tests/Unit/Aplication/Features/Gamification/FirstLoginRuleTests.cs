using ELearning.Application.Features.Gamification.Rules;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Tests.Unit.Aplication.Features.Gamification;

public class FirstLoginRuleTests
{
    [Fact]
    public void Evaluate_AnyUser_ProposesLoginFirstUnconditionally()
    {
        var rule = new FirstLoginRule();
        var user = User.Create("Test", "test@test.com", "hash", countryId: 1);

        var proposals = rule.Evaluate(user);

        var proposal = Assert.Single(proposals);
        Assert.Equal(BadgeCode.LoginFirst, proposal.Code);
        Assert.Null(proposal.CourseId);
    }
}
