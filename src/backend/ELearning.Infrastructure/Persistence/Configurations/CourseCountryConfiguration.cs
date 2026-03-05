using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class CourseCountryConfiguration : IEntityTypeConfiguration<CourseCountry>
{
    public void Configure(EntityTypeBuilder<CourseCountry> builder)
    {
        builder.ToTable("course_countries");

        builder.HasKey(cc => new { cc.CourseId, cc.CountryId });

        builder.Property(cc => cc.CourseId)
            .HasColumnName("course_id");

        builder.Property(cc => cc.CountryId)
            .HasColumnName("country_id");

        builder.Property(cc => cc.AssignedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .HasColumnName("assigned_at");

        builder.HasOne(cc => cc.Course)
            .WithMany(c => c.CourseCountries)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Country)
            .WithMany(c => c.CourseCountries)
            .HasForeignKey(cc => cc.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cc => cc.CountryId);
    }
}
