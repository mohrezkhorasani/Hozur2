using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db499 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTBLToMeetingTBL_UUID_MeetID",
                table: "UserTBLToMeetingTBL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBL_UUID_MeetID_Type",
                table: "UserTBLToMeetingTBL",
                columns: new[] { "UUID", "MeetID", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTBLToMeetingTBL_UUID_MeetID_Type",
                table: "UserTBLToMeetingTBL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBL_UUID_MeetID",
                table: "UserTBLToMeetingTBL",
                columns: new[] { "UUID", "MeetID" },
                unique: true);
        }
    }
}
