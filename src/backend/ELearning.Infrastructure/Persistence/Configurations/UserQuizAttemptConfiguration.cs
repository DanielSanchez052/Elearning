using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class UserQuizAttemptConfiguration : IEntityTypeConfiguration<UserQuizAttempt>
{
    public void Configure(EntityTypeBuilder<UserQuizAttempt> builder)
    {
        builder.ToTable("user_quiz_attempts");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.UserId)
            .HasColumnName("user_id");

        builder.Property(u => u.QuestionId)
            .HasColumnName("question_id");

        builder.Property(u => u.SelectedOptionId)
            .HasColumnName("selected_option_id");

        builder.Property(u => u.AttemptNumber)
            .HasColumnName("attempt_number");

        builder.Property(u => u.AttemptedAt)
            .HasColumnName("attempted_at");

        // Relaciones
        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Question)
            .WithMany()
            .HasForeignKey(u => u.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.SelectedOption)
            .WithMany()
            .HasForeignKey(u => u.SelectedOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => u.QuestionId);
        builder.HasIndex(u => new { u.UserId, u.QuestionId, u.AttemptNumber })
            .IsUnique();
    }
}
