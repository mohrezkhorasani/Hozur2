using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db554 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTBLToMeetingTBLMuiltiJoinTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "11111111, 4"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UserMeetID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IPList = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDUserTBLToMeetingTBL = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTBLToMeetingTBLMuiltiJoinTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserTBLToMeetingTBLMuiltiJoinTBL_UserTBLToMeetingTBL_IDUserTBLToMeetingTBL",
                        column: x => x.IDUserTBLToMeetingTBL,
                        principalTable: "UserTBLToMeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBLMuiltiJoinTBL_IDUserTBLToMeetingTBL",
                table: "UserTBLToMeetingTBLMuiltiJoinTBL",
                column: "IDUserTBLToMeetingTBL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTBLToMeetingTBLMuiltiJoinTBL");
        }
    }
}
