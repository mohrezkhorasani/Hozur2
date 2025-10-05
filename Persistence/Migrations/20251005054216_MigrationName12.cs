using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeUnique",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBL_CodeUnique",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                column: "CodeUnique",
                unique: true,
                filter: "[CodeUnique] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserTBLToMeetingTBL_CodeUnique",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL");

            migrationBuilder.DropColumn(
                name: "CodeUnique",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL");
        }
    }
}
