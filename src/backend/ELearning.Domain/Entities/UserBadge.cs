namespace ELearning.Domain.Entities;

public class UserBadge
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public int BadgeId { get; private set; }
    public Guid? CourseId { get; private set; }
    public DateTime ObtainedAt { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }

    public User User { get; private set; } = null!;
    public Badge Badge { get; private set; } = null!;
    public Course? Course { get; private set; }

    private UserBadge() { }

    public static UserBadge Create(Guid userId, int badgeId, Guid? courseId = null, Dictionary<string, object>? metadata = null)
    {
        return new UserBadge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BadgeId = badgeId,
            CourseId = courseId,
            ObtainedAt = DateTime.UtcNow,
            Metadata = metadata
        };
    }

    public void AddMetadata(string key, object value)
    {
        Metadata ??= new Dictionary<string, object>();
        Metadata[key] = value;
    }
}
