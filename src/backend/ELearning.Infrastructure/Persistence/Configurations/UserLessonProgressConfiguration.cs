using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class UserLessonProgressConfiguration : IEntityTypeConfiguration<UserLessonProgress>
{
    public void Configure(EntityTypeBuilder<UserLessonProgress> builder)
    {
        builder.ToTable("user_lesson_progress");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.EnrollmentId)
            .IsRequired()
            .HasColumnName("enrollment_id");

        builder.Property(p => p.LessonId)
            .IsRequired()
            .HasColumnName("lesson_id");

        builder.Property(p => p.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_completed");

        builder.Property(p => p.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(p => p.LastAccessedAt)
            .IsRequired()
            .HasColumnName("last_accessed_at");

        // Un enrollment no puede tener dos registros para la misma lección
        builder.HasIndex(p => new { p.EnrollmentId, p.LessonId })
            .IsUnique()
            .HasDatabaseName("IX_UserLessonProgress_EnrollmentId_LessonId");

        builder.HasIndex(p => p.LessonId)
            .HasDatabaseName("IX_UserLessonProgress_LessonId");

        builder.HasOne(p => p.Lesson)
            .WithMany(l => l.UserProgress)
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        // La relación con Enrollment está configurada desde CourseEnrollmentConfiguration
    }
}
