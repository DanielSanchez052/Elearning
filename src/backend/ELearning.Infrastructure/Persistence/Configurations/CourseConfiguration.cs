using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("title");

        builder.Property(c => c.Description)
            .HasColumnName("description");

        builder.Property(c => c.ThumbnailUrl)
            .HasColumnName("thumbnail_url");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_active");

        builder.Property(c => c.IsGlobal)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_global");

        builder.Property(c => c.TimeLimitMins)
            .HasColumnName("time_limit_mins");

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasIndex(c => c.CreatedBy);

        builder.HasIndex(c => c.IsActive);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany(u => u.CreatedCourses)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint("chk_courses_time_limit", 
            "time_limit_mins IS NULL OR time_limit_mins > 0");
    }
}
