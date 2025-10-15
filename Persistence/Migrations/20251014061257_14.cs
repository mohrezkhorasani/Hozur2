using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class _14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MeetingTBL",
                schema: "GeneralInfo",
                newName: "MeetingTBL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "GeneralInfo");

            migrationBuilder.RenameTable(
                name: "MeetingTBL",
                newName: "MeetingTBL",
                newSchema: "GeneralInfo");
        }
    }
}
