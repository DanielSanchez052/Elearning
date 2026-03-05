using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_lessons_url",
                table: "lessons");

            migrationBuilder.AddCheckConstraint(
                name: "chk_lessons_url",
                table: "lessons",
                sql: "(type IN ('Video', 'Pdf') AND content_url IS NOT NULL) OR type = 'Quiz'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_lessons_url",
                table: "lessons");

            migrationBuilder.AddCheckConstraint(
                name: "chk_lessons_url",
                table: "lessons",
                sql: "(type IN ('video', 'pdf') AND content_url IS NOT NULL) OR type = 'quiz'");
        }
    }
}
