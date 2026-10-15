using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "instructor_id",
                table: "courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_instructor_id",
                table: "courses",
                column: "instructor_id");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_users_instructor_id",
                table: "courses",
                column: "instructor_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Hasta ahora el instructor de un curso era su creador: los cursos creados por
            // instructores quedan asignados a ellos para que no pierdan el acceso. Los creados
            // por admins y superadmins quedan sin instructor.
            migrationBuilder.Sql("""
                UPDATE courses
                SET instructor_id = "CreatedBy"
                WHERE "CreatedBy" IN (SELECT id FROM users WHERE role = 'Instructor');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_users_instructor_id",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_instructor_id",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "instructor_id",
                table: "courses");
        }
    }
}
