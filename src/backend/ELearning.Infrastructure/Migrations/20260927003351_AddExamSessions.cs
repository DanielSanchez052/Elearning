using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "user_quiz_results",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "exam_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_sessions_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_exam_session_one_open_per_user_course",
                table: "exam_sessions",
                columns: new[] { "user_id", "course_id" },
                unique: true,
                filter: "submitted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "idx_exam_session_user_course_attempt",
                table: "exam_sessions",
                columns: new[] { "user_id", "course_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_sessions_course_id",
                table: "exam_sessions",
                column: "course_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_sessions");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "user_quiz_results");
        }
    }
}
