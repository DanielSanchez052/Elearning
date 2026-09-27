using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class UserBadgeConfiguration : IEntityTypeConfiguration<UserBadge>
{
    public void Configure(EntityTypeBuilder<UserBadge> builder)
    {
        builder.ToTable("user_badges");

        builder.HasKey(ub => ub.Id);

        builder.Property(ub => ub.Id)
            .HasColumnName("id");

        builder.Property(ub => ub.ObtainedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .HasColumnName("obtained_at");

        builder.Property(ub => ub.Metadata)
            .HasColumnType("jsonb")
            .HasColumnName("metadata");

        builder.Property(ub => ub.CourseId)
            .HasColumnName("course_id");

        builder.HasIndex(ub => ub.UserId);

        builder.HasIndex(ub => ub.BadgeId);

        builder.HasOne(ub => ub.User)
            .WithMany(u => u.Badges)
            .HasForeignKey(ub => ub.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ub => ub.Badge)
            .WithMany(b => b.UserBadges)
            .HasForeignKey(ub => ub.BadgeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Badges are never revoked; courses get deactivated, not deleted
        // (same rationale as CourseEnrollmentConfiguration's Course FK).
        builder.HasOne(ub => ub.Course)
            .WithMany()
            .HasForeignKey(ub => ub.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // A badge can be earned once per (user, badge, course) combination.
        // CourseId is null for course-agnostic badges (LoginFirst), so the
        // index must treat NULLs as equal to each other (Postgres 17
        // "NULLS NOT DISTINCT") or two LoginFirst rows for the same user
        // would both be allowed through.
        builder.HasIndex(ub => new { ub.UserId, ub.BadgeId, ub.CourseId })
            .IsUnique()
            .AreNullsDistinct(false);
    }
}
