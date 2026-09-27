using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
{
    public void Configure(EntityTypeBuilder<ExamSession> builder)
    {
        builder.ToTable("exam_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.UserId)
            .HasColumnName("user_id");

        builder.Property(s => s.CourseId)
            .HasColumnName("course_id");

        builder.Property(s => s.AttemptNumber)
            .HasColumnName("attempt_number");

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at");

        builder.Property(s => s.SubmittedAt)
            .HasColumnName("submitted_at");

        // Relaciones (sin navegaciones en la entidad)
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Course>()
            .WithMany()
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(s => new { s.UserId, s.CourseId, s.AttemptNumber })
            .IsUnique()
            .HasDatabaseName("idx_exam_session_user_course_attempt");

        // Como máximo una sesión abierta (sin enviar) por estudiante y curso.
        builder.HasIndex(s => new { s.UserId, s.CourseId })
            .IsUnique()
            .HasFilter("submitted_at IS NULL")
            .HasDatabaseName("idx_exam_session_one_open_per_user_course");
    }
}
