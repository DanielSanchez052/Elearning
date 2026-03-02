namespace ELearning.Domain.Entities;

public class Badge
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? IconUrl { get; private set; }

    public ICollection<UserBadge> UserBadges { get; private set; } = new List<UserBadge>();

    private Badge() { }

    public static Badge Create(string code, string name, string? description = null, string? iconUrl = null)
    {
        return new Badge
        {
            Code = code,
            Name = name,
            Description = description,
            IconUrl = iconUrl
        };
    }

    public void UpdateInfo(string name, string? description, string? iconUrl)
    {
        Name = name;
        Description = description;
        IconUrl = iconUrl;
    }
}
