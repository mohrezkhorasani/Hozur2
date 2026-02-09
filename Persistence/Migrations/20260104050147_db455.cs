using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db455 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomTBL_Title",
                table: "RoomTBL");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBL_Title",
                table: "RoomTBL",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomTBL_Title",
                table: "RoomTBL");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBL_Title",
                table: "RoomTBL",
                column: "Title",
                unique: true);
        }
    }
}
