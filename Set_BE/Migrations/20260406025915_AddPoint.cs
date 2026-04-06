using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Set_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActionPoints",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedCode",
                table: "MovieCodes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionPoints",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NormalizedCode",
                table: "MovieCodes");
        }
    }
}
