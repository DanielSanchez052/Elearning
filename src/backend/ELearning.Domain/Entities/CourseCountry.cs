namespace ELearning.Domain.Entities;

public class CourseCountry
{
    public Guid CourseId { get; private set; }
    public int CountryId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    public Course Course { get; private set; } = null!;
    public Country Country { get; private set; } = null!;

    private CourseCountry() { }

    public static CourseCountry Create(Guid courseId, int countryId)
    {
        return new CourseCountry
        {
            CourseId = courseId,
            CountryId = countryId,
            AssignedAt = DateTime.UtcNow
        };
    }
}
