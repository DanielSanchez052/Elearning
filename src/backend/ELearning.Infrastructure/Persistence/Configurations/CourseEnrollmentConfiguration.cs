using ELearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CourseEnrollmentConfiguration : IEntityTypeConfiguration<CourseEnrollment>
{
    public void Configure(EntityTypeBuilder<CourseEnrollment> builder)
    {
        builder.ToTable("course_enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(e => e.CourseId)
            .IsRequired()
            .HasColumnName("course_id");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasColumnName("status")
            .HasConversion<string>();

        builder.Property(e => e.EnrolledAt)
            .IsRequired()
            .HasColumnName("enrolled_at");

        builder.Property(e => e.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(e => e.DeadlineAt)
            .HasColumnName("deadline_at");

        // Un usuario no puede estar inscripto dos veces en el mismo curso
        builder.HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique()
            .HasDatabaseName("IX_CourseEnrollments_UserId_CourseId");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_CourseEnrollments_UserId");

        builder.HasIndex(e => e.CourseId)
            .HasDatabaseName("IX_CourseEnrollments_CourseId");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_CourseEnrollments_Status");

        builder.HasOne(e => e.User)
            .WithMany(u => u.Enrollments)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.LessonProgress)
            .WithOne(p => p.Enrollment)
            .HasForeignKey(p => p.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
