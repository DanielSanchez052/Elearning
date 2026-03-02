using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasColumnName("id");

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

        builder.HasIndex(q => q.LessonId);

        builder.HasOne(q => q.Lesson)
            .WithOne(l => l.QuizQuestion)
            .HasForeignKey<QuizQuestion>(q => q.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasCheckConstraint("chk_qq_pass_score", "pass_score BETWEEN 0 AND 100");
        builder.HasCheckConstraint("chk_qq_attempts", "max_attempts > 0");
    }
}
