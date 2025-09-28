using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                schema: "GeneralInfo",
                table: "MeetingTBL");

            migrationBuilder.AddColumn<string>(
                name: "UserNickName",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNickName",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
