using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserTBLToMeetingTBLLogs",
                schema: "GeneralInfo",
                newName: "UserTBLToMeetingTBLLogs");

            migrationBuilder.RenameTable(
                name: "UserTBLToMeetingTBL",
                schema: "GeneralInfo",
                newName: "UserTBLToMeetingTBL");

            migrationBuilder.RenameTable(
                name: "UserTBL",
                schema: "GeneralInfo",
                newName: "UserTBL");

            migrationBuilder.RenameTable(
                name: "RoomTBLToUserTBL",
                schema: "GeneralInfo",
                newName: "RoomTBLToUserTBL");

            migrationBuilder.RenameTable(
                name: "RoomTBL",
                schema: "GeneralInfo",
                newName: "RoomTBL");

            migrationBuilder.RenameTable(
                name: "ConfigTBL",
                schema: "GeneralInfo",
                newName: "ConfigTBL");

            migrationBuilder.RenameTable(
                name: "CertificateTBL",
                schema: "GeneralInfo",
                newName: "CertificateTBL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UserTBLToMeetingTBLLogs",
                newName: "UserTBLToMeetingTBLLogs",
                newSchema: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "UserTBLToMeetingTBL",
                newName: "UserTBLToMeetingTBL",
                newSchema: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "UserTBL",
                newName: "UserTBL",
                newSchema: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "RoomTBLToUserTBL",
                newName: "RoomTBLToUserTBL",
                newSchema: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "RoomTBL",
                newName: "RoomTBL",
                newSchema: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "ConfigTBL",
                newName: "ConfigTBL",
                newSchema: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "CertificateTBL",
                newName: "CertificateTBL",
                newSchema: "GeneralInfo");
        }
    }
}
