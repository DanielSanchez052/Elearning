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

        builder.HasIndex(ub => new { ub.UserId, ub.BadgeId })
            .IsUnique();
    }
}
