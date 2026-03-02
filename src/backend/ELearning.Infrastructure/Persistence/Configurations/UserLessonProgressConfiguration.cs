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

        builder.Property(p => p.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_completed");

        builder.Property(p => p.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(p => p.QuizScore)
            .HasPrecision(5, 2)
            .HasColumnName("quiz_score");

        builder.Property(p => p.AttemptsUsed)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnName("attempts_used");

        builder.HasIndex(p => p.EnrollmentId);

        builder.HasIndex(p => p.LessonId);

        builder.HasOne(p => p.Enrollment)
            .WithMany(e => e.LessonProgress)
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Lesson)
            .WithMany(l => l.UserProgress)
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.EnrollmentId, p.LessonId })
            .IsUnique();

        builder.HasCheckConstraint("chk_ulp_score", 
            "quiz_score IS NULL OR (quiz_score BETWEEN 0 AND 100)");
    }
}
