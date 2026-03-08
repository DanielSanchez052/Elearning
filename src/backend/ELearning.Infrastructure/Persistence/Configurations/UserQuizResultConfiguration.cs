using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class UserQuizResultConfiguration : IEntityTypeConfiguration<UserQuizResult>
{
    public void Configure(EntityTypeBuilder<UserQuizResult> builder)
    {
        builder.ToTable("user_quiz_results");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.UserId)
            .HasColumnName("user_id");

        builder.Property(u => u.LessonId)
            .HasColumnName("lesson_id");

        builder.Property(u => u.CourseId)
            .HasColumnName("course_id");

        builder.Property(u => u.AttemptNumber)
            .HasColumnName("attempt_number");

        builder.Property(u => u.Score)
            .HasColumnName("score")
            .HasPrecision(5, 2);

        builder.Property(u => u.IsPassed)
            .HasColumnName("is_passed");

        builder.Property(u => u.CompletedAt)
            .HasColumnName("completed_at");

        // Relaciones
        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Lesson)
            .WithMany()
            .HasForeignKey(u => u.LessonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(u => u.Course)
            .WithMany()
            .HasForeignKey(u => u.CourseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Índices
        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => new { u.UserId, u.LessonId, u.AttemptNumber })
            .IsUnique()
            .HasName("idx_user_lesson_attempt");

        builder.HasIndex(u => new { u.UserId, u.CourseId, u.AttemptNumber })
            .IsUnique()
            .HasName("idx_user_course_attempt");
    }
}
