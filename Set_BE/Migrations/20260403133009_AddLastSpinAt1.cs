using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Set_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSpinAt1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSpinAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSpinAt",
                table: "Users");
        }
    }
}
