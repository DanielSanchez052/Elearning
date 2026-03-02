using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
{
    public void Configure(EntityTypeBuilder<QuizOption> builder)
    {
        builder.ToTable("quiz_options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id");

        builder.Property(o => o.OptionText)
            .IsRequired()
            .HasColumnName("option_text");

        builder.Property(o => o.IsCorrect)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_correct");

        builder.Property(o => o.OrderIndex)
            .IsRequired()
            .HasColumnName("order_index");

        builder.HasIndex(o => o.QuestionId);

        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
