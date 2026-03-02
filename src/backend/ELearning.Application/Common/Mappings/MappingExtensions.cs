using ELearning.Domain.Entities;

namespace ELearning.Application.Common.Mappings;

public static class UserMappings
{
    public static object ToDto(this User user) => new { user.Id, user.FullName, user.Email, Role = user.Role.ToString(), user.CountryId };
}

public static class CourseMappings
{
    public static object ToDto(this Course course) => new { course.Id, course.Title, course.Description, course.ThumbnailUrl, course.IsActive, course.IsGlobal, course.TimeLimitMins };
}
