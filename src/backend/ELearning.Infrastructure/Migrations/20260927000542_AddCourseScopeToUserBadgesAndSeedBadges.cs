using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseScopeToUserBadgesAndSeedBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_badges_UserId_BadgeId",
                table: "user_badges");

            migrationBuilder.AddColumn<Guid>(
                name: "course_id",
                table: "user_badges",
                type: "uuid",
                nullable: true);

            migrationBuilder.InsertData(
                table: "badges",
                columns: new[] { "id", "code", "description", "icon_url", "name" },
                values: new object[,]
                {
                    { 1, "LoginFirst", "Iniciaste sesión en la plataforma por primera vez.", null, "Primer Inicio de Sesión" },
                    { 2, "CourseDone", "Completaste todas las lecciones requeridas de un curso.", null, "Curso Completado" },
                    { 3, "Speedster", "Aprobaste el examen final de un curso en menos de 10 minutos, en tu primer intento.", null, "Velocista" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_badges_course_id",
                table: "user_badges",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_badges_UserId_BadgeId_course_id",
                table: "user_badges",
                columns: new[] { "UserId", "BadgeId", "course_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddForeignKey(
                name: "FK_user_badges_courses_course_id",
                table: "user_badges",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_badges_courses_course_id",
                table: "user_badges");

            migrationBuilder.DropIndex(
                name: "IX_user_badges_course_id",
                table: "user_badges");

            migrationBuilder.DropIndex(
                name: "IX_user_badges_UserId_BadgeId_course_id",
                table: "user_badges");

            migrationBuilder.DeleteData(
                table: "badges",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "badges",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "badges",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "course_id",
                table: "user_badges");

            migrationBuilder.CreateIndex(
                name: "IX_user_badges_UserId_BadgeId",
                table: "user_badges",
                columns: new[] { "UserId", "BadgeId" },
                unique: true);
        }
    }
}
