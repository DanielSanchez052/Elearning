using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizEvaluationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quiz_questions_lessons_LessonId",
                table: "quiz_questions");

            migrationBuilder.DropIndex(
                name: "IX_quiz_questions_LessonId",
                table: "quiz_questions");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "quiz_questions",
                newName: "lesson_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "lesson_id",
                table: "quiz_questions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "course_id",
                table: "quiz_questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_required",
                table: "quiz_questions",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "quiz_questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "user_quiz_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    attempted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_quiz_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_quiz_attempts_quiz_options_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "quiz_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_quiz_attempts_quiz_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_quiz_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_quiz_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_id = table.Column<Guid>(type: "uuid", nullable: true),
                    course_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    is_passed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_quiz_results", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_quiz_results_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_quiz_results_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_quiz_results_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_course_id",
                table: "quiz_questions",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_lesson_id",
                table: "quiz_questions",
                column: "lesson_id");

            migrationBuilder.AddCheckConstraint(
                name: "chk_qq_type",
                table: "quiz_questions",
                sql: "(type = 0 AND lesson_id IS NOT NULL AND course_id IS NULL) OR (type = 1 AND lesson_id IS NULL AND course_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_attempts_question_id",
                table: "user_quiz_attempts",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_attempts_selected_option_id",
                table: "user_quiz_attempts",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_attempts_user_id",
                table: "user_quiz_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_attempts_user_id_question_id_attempt_number",
                table: "user_quiz_attempts",
                columns: new[] { "user_id", "question_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_course_attempt",
                table: "user_quiz_results",
                columns: new[] { "user_id", "course_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_user_lesson_attempt",
                table: "user_quiz_results",
                columns: new[] { "user_id", "lesson_id", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_results_course_id",
                table: "user_quiz_results",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_results_lesson_id",
                table: "user_quiz_results",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_quiz_results_user_id",
                table: "user_quiz_results",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quiz_questions_courses_course_id",
                table: "quiz_questions",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quiz_questions_lessons_lesson_id",
                table: "quiz_questions",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quiz_questions_courses_course_id",
                table: "quiz_questions");

            migrationBuilder.DropForeignKey(
                name: "FK_quiz_questions_lessons_lesson_id",
                table: "quiz_questions");

            migrationBuilder.DropTable(
                name: "user_quiz_attempts");

            migrationBuilder.DropTable(
                name: "user_quiz_results");

            migrationBuilder.DropIndex(
                name: "IX_quiz_questions_course_id",
                table: "quiz_questions");

            migrationBuilder.DropIndex(
                name: "IX_quiz_questions_lesson_id",
                table: "quiz_questions");

            migrationBuilder.DropCheckConstraint(
                name: "chk_qq_type",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "course_id",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "is_required",
                table: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "quiz_questions");

            migrationBuilder.RenameColumn(
                name: "lesson_id",
                table: "quiz_questions",
                newName: "LessonId");

            migrationBuilder.AlterColumn<Guid>(
                name: "LessonId",
                table: "quiz_questions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_quiz_questions_LessonId",
                table: "quiz_questions",
                column: "LessonId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_quiz_questions_lessons_LessonId",
                table: "quiz_questions",
                column: "LessonId",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
