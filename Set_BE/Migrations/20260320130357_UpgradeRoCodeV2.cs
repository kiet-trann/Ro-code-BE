using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Set_BE.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeRoCodeV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorName",
                table: "MovieCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MovieCodes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "MovieCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActorName",
                table: "MovieCodes");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "MovieCodes");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "MovieCodes");
        }
    }
}
