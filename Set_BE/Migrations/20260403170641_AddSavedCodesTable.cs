using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Set_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddSavedCodesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SavedCodes",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MovieCodeId = table.Column<int>(type: "integer", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedCodes", x => new { x.UserId, x.MovieCodeId });
                    table.ForeignKey(
                        name: "FK_SavedCodes_MovieCodes_MovieCodeId",
                        column: x => x.MovieCodeId,
                        principalTable: "MovieCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedCodes_MovieCodeId",
                table: "SavedCodes",
                column: "MovieCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedCodes");
        }
    }
}
