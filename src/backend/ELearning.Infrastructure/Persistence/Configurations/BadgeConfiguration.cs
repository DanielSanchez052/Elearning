using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ELearning.Domain.Entities;
using ELearning.Domain.Enums;

namespace ELearning.Infrastructure.Persistence.Configurations;

public class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.ToTable("badges");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnName("id");

        builder.Property(b => b.Code)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("code");

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(b => b.Description)
            .HasColumnName("description");

        builder.Property(b => b.IconUrl)
            .HasColumnName("icon_url");

        builder.HasIndex(b => b.Code)
            .IsUnique();

        // Locked design: exactly 3 badges (LoginFirst, CourseDone, Speedster).
        // Fixed Ids so seeded rows are stable across environments/migrations.
        builder.HasData(
            new
            {
                Id = 1,
                Code = BadgeCode.LoginFirst.ToString(),
                Name = "Primer Inicio de Sesión",
                Description = "Iniciaste sesión en la plataforma por primera vez.",
                IconUrl = (string?)null
            },
            new
            {
                Id = 2,
                Code = BadgeCode.CourseDone.ToString(),
                Name = "Curso Completado",
                Description = "Completaste todas las lecciones requeridas de un curso.",
                IconUrl = (string?)null
            },
            new
            {
                Id = 3,
                Code = BadgeCode.Speedster.ToString(),
                Name = "Velocista",
                Description = "Aprobaste el examen final de un curso en menos de 10 minutos, en tu primer intento.",
                IconUrl = (string?)null
            }
        );
    }
}
