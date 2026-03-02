namespace ELearning.Domain.Entities;

public class Country
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<CourseCountry> CourseCountries { get; private set; } = new List<CourseCountry>();

    private Country() { }

    public static Country Create(string code, string name)
    {
        return new Country
        {
            Code = code,
            Name = name,
            IsActive = true
        };
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void UpdateCode(string code)
    {
        Code = code;
    }
}
