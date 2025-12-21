using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db450 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BBB_MeetID",
                table: "RoomTBL",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBL_BBB_MeetID",
                table: "RoomTBL",
                column: "BBB_MeetID",
                unique: true,
                filter: "[BBB_MeetID] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomTBL_BBB_MeetID",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "BBB_MeetID",
                table: "RoomTBL");
        }
    }
}
