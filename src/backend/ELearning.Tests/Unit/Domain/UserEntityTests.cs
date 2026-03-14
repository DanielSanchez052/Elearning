using ELearning.Domain.Entities;

namespace ELearning.Tests.Unit.Domain;


public class UserEntityTests
{
    [Fact]
    public void Create_NewUser_IsNotEmailVerified()
    {
        var user = User.Create("Test", "test@test.com", "hash", countryId: 1);

        Assert.False(user.IsEmailVerified);
    }

    [Fact]
    public void VerifyEmail_SetsVerifiedAndClearsToken()
    {
        var user = User.Create("Test", "test@test.com", "hash", countryId: 1);
        user.SetEmailVerifyToken("sometoken");

        user.VerifyEmail();

        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerifyToken);
    }

    [Fact]
    public void SetResetToken_SetsTokenAndExpiry()
    {
        var user = User.Create("Test", "test@test.com", "hash", countryId: 1);
        var expiry = DateTime.UtcNow.AddMinutes(15);

        user.SetResetToken("mytoken", expiry);

        Assert.Equal("mytoken", user.ResetToken);
        Assert.Equal(expiry, user.ResetTokenExpires);
    }

    [Fact]
    public void ClearResetToken_SetsTokenAndExpiryToNull()
    {
        var user = User.Create("Test", "test@test.com", "hash", countryId: 1);
        user.SetResetToken("mytoken", DateTime.UtcNow.AddMinutes(15));

        user.ClearResetToken();

        Assert.Null(user.ResetToken);
        Assert.Null(user.ResetTokenExpires);
    }

    [Fact]
    public void RecordLogin_IncrementsStreakAndSetsLastLoginAt()
    {
        var user = User.Create("Test", "test@test.com", "hash", countryId: 1);
        var streakBefore = user.LoginStreak;
        var before = DateTime.UtcNow;

        user.RecordLogin();

        Assert.Equal(streakBefore + 1, user.LoginStreak);
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= before);
    }

    [Fact]
    public void SetPasswordHash_UpdatesHash()
    {
        var user = User.Create("Test", "test@test.com", "old-hash", countryId: 1);

        user.SetPasswordHash("new-hash");

        Assert.Equal("new-hash", user.PasswordHash);
    }
}
