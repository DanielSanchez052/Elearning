using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCountryRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_countries_countries_CountryId1",
                table: "course_countries");

            migrationBuilder.DropForeignKey(
                name: "FK_users_countries_CountryId1",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_CountryId1",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_course_countries_CountryId1",
                table: "course_countries");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CountryId1",
                table: "course_countries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CountryId1",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CountryId1",
                table: "course_countries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_CountryId1",
                table: "users",
                column: "CountryId1");

            migrationBuilder.CreateIndex(
                name: "IX_course_countries_CountryId1",
                table: "course_countries",
                column: "CountryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_course_countries_countries_CountryId1",
                table: "course_countries",
                column: "CountryId1",
                principalTable: "countries",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_countries_CountryId1",
                table: "users",
                column: "CountryId1",
                principalTable: "countries",
                principalColumn: "id");
        }
    }
}
