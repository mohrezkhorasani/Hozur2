using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTBLToMeetingTBLLogs",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Explain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserTBLToMeetingTBLID = table.Column<long>(type: "bigint", nullable: false),
                    IDUserTBLToMeetingTBL = table.Column<long>(type: "bigint", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTBLToMeetingTBLLogs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBL_UserTBLToMeetingTBLID",
                        column: x => x.UserTBLToMeetingTBLID,
                        principalSchema: "GeneralInfo",
                        principalTable: "UserTBLToMeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs",
                column: "UserTBLToMeetingTBLID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTBLToMeetingTBLLogs");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL");
        }
    }
}
