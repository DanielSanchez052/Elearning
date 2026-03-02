using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id");

        builder.Property(n => n.Type)
            .IsRequired()
            .HasColumnName("type")
            .HasConversion<string>();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("title");

        builder.Property(n => n.Message)
            .IsRequired()
            .HasColumnName("message");

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_read");

        builder.Property(n => n.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()")
            .HasColumnName("created_at");

        builder.HasIndex(n => n.UserId);

        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasFilter("is_read = FALSE");

        builder.HasIndex(n => n.CreatedAt);

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
