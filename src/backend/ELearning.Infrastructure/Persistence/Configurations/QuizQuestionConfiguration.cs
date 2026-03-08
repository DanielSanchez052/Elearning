using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id");

        builder.Property(q => q.Type)
            .IsRequired()
            .HasColumnName("type")
            .HasConversion<int>()
            .HasDefaultValue(QuizType.PerLesson);

        builder.Property(q => q.CourseId)
            .HasColumnName("course_id");

        builder.Property(q => q.IsRequired)
            .IsRequired()
            .HasColumnName("is_required")
            .HasDefaultValue(true);

        // Existentes
        builder.Property(q => q.LessonId)
            .HasColumnName("lesson_id");

        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasColumnName("question_text");

        builder.Property(q => q.PassScore)
            .IsRequired()
            .HasPrecision(5, 2)
            .HasDefaultValue(60.00m)
            .HasColumnName("pass_score");

        builder.Property(q => q.MaxAttempts)
            .IsRequired()
            .HasDefaultValue(3)
            .HasColumnName("max_attempts");

        builder.Property(q => q.OrderIndex)
            .IsRequired()
            .HasDefaultValue(1)
            .HasColumnName("order_index");

        // Índices
        builder.HasIndex(q => q.LessonId);
        builder.HasIndex(q => q.CourseId);

        // Relaciones
        builder.HasOne(q => q.Lesson)
            .WithMany(l => l.QuizQuestions)
            .HasForeignKey(q => q.LessonId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(q => q.Course)
            .WithMany()
            .HasForeignKey(q => q.CourseId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // Restricciones
        builder.HasCheckConstraint("chk_qq_pass_score", "pass_score BETWEEN 0 AND 100");
        builder.HasCheckConstraint("chk_qq_attempts", "max_attempts > 0");
        builder.HasCheckConstraint("chk_qq_type", 
            "(type = 0 AND lesson_id IS NOT NULL AND course_id IS NULL) OR " +
            "(type = 1 AND lesson_id IS NULL AND course_id IS NOT NULL)");
    }
}
