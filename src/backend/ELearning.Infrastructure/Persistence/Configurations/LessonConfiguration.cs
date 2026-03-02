using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("title");

        builder.Property(l => l.Type)
            .IsRequired()
            .HasColumnName("type")
            .HasConversion<string>();

        builder.Property(l => l.ContentUrl)
            .HasColumnName("content_url");

        builder.Property(l => l.OrderIndex)
            .IsRequired()
            .HasColumnName("order_index");

        builder.Property(l => l.IsRequired)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("is_required");

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.HasIndex(l => l.CourseId);

        builder.HasIndex(l => new { l.CourseId, l.OrderIndex })
            .IsUnique();

        builder.HasOne(l => l.Course)
            .WithMany(c => c.Lessons)
            .HasForeignKey(l => l.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasCheckConstraint("chk_lessons_url", 
            "(type IN ('video', 'pdf') AND content_url IS NOT NULL) OR type = 'quiz'");
    }
}
