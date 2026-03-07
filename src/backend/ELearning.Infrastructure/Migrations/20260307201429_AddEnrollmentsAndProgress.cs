using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentsAndProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_enrollments_courses_CourseId",
                table: "course_enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_course_enrollments_users_UserId",
                table: "course_enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_user_lesson_progress_course_enrollments_EnrollmentId",
                table: "user_lesson_progress");

            migrationBuilder.DropForeignKey(
                name: "FK_user_lesson_progress_lessons_LessonId",
                table: "user_lesson_progress");

            migrationBuilder.DropIndex(
                name: "IX_user_lesson_progress_EnrollmentId",
                table: "user_lesson_progress");

            migrationBuilder.DropCheckConstraint(
                name: "chk_ulp_score",
                table: "user_lesson_progress");

            migrationBuilder.DropIndex(
                name: "IX_course_enrollments_completed_at",
                table: "course_enrollments");

            migrationBuilder.DropColumn(
                name: "attempts_used",
                table: "user_lesson_progress");

            migrationBuilder.DropColumn(
                name: "quiz_score",
                table: "user_lesson_progress");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "user_lesson_progress",
                newName: "lesson_id");

            migrationBuilder.RenameColumn(
                name: "EnrollmentId",
                table: "user_lesson_progress",
                newName: "enrollment_id");

            migrationBuilder.RenameIndex(
                name: "IX_user_lesson_progress_LessonId",
                table: "user_lesson_progress",
                newName: "IX_UserLessonProgress_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_user_lesson_progress_EnrollmentId_LessonId",
                table: "user_lesson_progress",
                newName: "IX_UserLessonProgress_EnrollmentId_LessonId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "course_enrollments",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "course_enrollments",
                newName: "course_id");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "course_enrollments",
                newName: "deadline_at");

            migrationBuilder.RenameIndex(
                name: "IX_course_enrollments_UserId_CourseId",
                table: "course_enrollments",
                newName: "IX_CourseEnrollments_UserId_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_course_enrollments_UserId",
                table: "course_enrollments",
                newName: "IX_CourseEnrollments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_course_enrollments_CourseId",
                table: "course_enrollments",
                newName: "IX_CourseEnrollments_CourseId");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_accessed_at",
                table: "user_lesson_progress",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "enrolled_at",
                table: "course_enrollments",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "course_enrollments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_Status",
                table: "course_enrollments",
                column: "status");

            migrationBuilder.AddForeignKey(
                name: "FK_course_enrollments_courses_course_id",
                table: "course_enrollments",
                column: "course_id",
                principalTable: "courses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_course_enrollments_users_user_id",
                table: "course_enrollments",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_lesson_progress_course_enrollments_enrollment_id",
                table: "user_lesson_progress",
                column: "enrollment_id",
                principalTable: "course_enrollments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_lesson_progress_lessons_lesson_id",
                table: "user_lesson_progress",
                column: "lesson_id",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_enrollments_courses_course_id",
                table: "course_enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_course_enrollments_users_user_id",
                table: "course_enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_user_lesson_progress_course_enrollments_enrollment_id",
                table: "user_lesson_progress");

            migrationBuilder.DropForeignKey(
                name: "FK_user_lesson_progress_lessons_lesson_id",
                table: "user_lesson_progress");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_Status",
                table: "course_enrollments");

            migrationBuilder.DropColumn(
                name: "last_accessed_at",
                table: "user_lesson_progress");

            migrationBuilder.DropColumn(
                name: "status",
                table: "course_enrollments");

            migrationBuilder.RenameColumn(
                name: "lesson_id",
                table: "user_lesson_progress",
                newName: "LessonId");

            migrationBuilder.RenameColumn(
                name: "enrollment_id",
                table: "user_lesson_progress",
                newName: "EnrollmentId");

            migrationBuilder.RenameIndex(
                name: "IX_UserLessonProgress_LessonId",
                table: "user_lesson_progress",
                newName: "IX_user_lesson_progress_LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_UserLessonProgress_EnrollmentId_LessonId",
                table: "user_lesson_progress",
                newName: "IX_user_lesson_progress_EnrollmentId_LessonId");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "course_enrollments",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "course_id",
                table: "course_enrollments",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "deadline_at",
                table: "course_enrollments",
                newName: "started_at");

            migrationBuilder.RenameIndex(
                name: "IX_CourseEnrollments_UserId_CourseId",
                table: "course_enrollments",
                newName: "IX_course_enrollments_UserId_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseEnrollments_UserId",
                table: "course_enrollments",
                newName: "IX_course_enrollments_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseEnrollments_CourseId",
                table: "course_enrollments",
                newName: "IX_course_enrollments_CourseId");

            migrationBuilder.AddColumn<int>(
                name: "attempts_used",
                table: "user_lesson_progress",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "quiz_score",
                table: "user_lesson_progress",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "enrolled_at",
                table: "course_enrollments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_user_lesson_progress_EnrollmentId",
                table: "user_lesson_progress",
                column: "EnrollmentId");

            migrationBuilder.AddCheckConstraint(
                name: "chk_ulp_score",
                table: "user_lesson_progress",
                sql: "quiz_score IS NULL OR (quiz_score BETWEEN 0 AND 100)");

            migrationBuilder.CreateIndex(
                name: "IX_course_enrollments_completed_at",
                table: "course_enrollments",
                column: "completed_at",
                filter: "completed_at IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_course_enrollments_courses_CourseId",
                table: "course_enrollments",
                column: "CourseId",
                principalTable: "courses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_course_enrollments_users_UserId",
                table: "course_enrollments",
                column: "UserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_lesson_progress_course_enrollments_EnrollmentId",
                table: "user_lesson_progress",
                column: "EnrollmentId",
                principalTable: "course_enrollments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_lesson_progress_lessons_LessonId",
                table: "user_lesson_progress",
                column: "LessonId",
                principalTable: "lessons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
