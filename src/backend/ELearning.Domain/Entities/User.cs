using ELearning.Domain.Enums;

namespace ELearning.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public int CountryId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public string? EmailVerifyToken { get; private set; }
    public string? ResetToken { get; private set; }
    public DateTime? ResetTokenExpires { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int LoginStreak { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Country Country { get; private set; } = null!;
    public ICollection<Course> CreatedCourses { get; private set; } = new List<Course>();
    public ICollection<CourseEnrollment> Enrollments { get; private set; } = new List<CourseEnrollment>();
    public ICollection<UserBadge> Badges { get; private set; } = new List<UserBadge>();
    public ICollection<Notification> Notifications { get; private set; } = new List<Notification>();

    private User() { }

    public static User Create(string fullName, string email, string passwordHash, int countryId, UserRole role = UserRole.Student)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash,
            CountryId = countryId,
            Role = role,
            IsEmailVerified = false,
            LoginStreak = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        EmailVerifyToken = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEmailVerifyToken(string token)
    {
        EmailVerifyToken = token;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetResetToken(string token, DateTime expires)
    {
        ResetToken = token;
        ResetTokenExpires = expires;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearResetToken()
    {
        ResetToken = null;
        ResetTokenExpires = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        LoginStreak++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateFullName(string fullName)
    {
        FullName = fullName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCountry(int countryId)
    {
        CountryId = countryId;
        UpdatedAt = DateTime.UtcNow;
    }
}
