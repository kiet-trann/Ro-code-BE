using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Set_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddLegacyTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LegacyTitles",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyTitles",
                table: "Users");
        }
    }
}
