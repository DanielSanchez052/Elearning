using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("full_name");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasColumnName("password_hash");

        builder.Property(u => u.Role)
            .IsRequired()
            .HasColumnName("role")
            .HasConversion<string>();

        builder.Property(u => u.IsEmailVerified)
            .HasColumnName("is_email_verified");

        builder.Property(u => u.EmailVerifyToken)
            .HasColumnName("email_verify_token");

        builder.Property(u => u.ResetToken)
            .HasColumnName("reset_token");

        builder.Property(u => u.ResetTokenExpires)
            .HasColumnName("reset_token_expires");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.LoginStreak)
            .HasColumnName("login_streak");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.CountryId);

        builder.HasIndex(u => u.Role);

        builder.HasOne(u => u.Country)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
