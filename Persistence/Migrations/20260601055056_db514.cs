using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db514 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetPollQuizTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "525, 7"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 625),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsForAll = table.Column<bool>(type: "bit", nullable: false),
                    IDMeetingTBL = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetPollQuizTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetPollQuizTBL_MeetingTBL_IDMeetingTBL",
                        column: x => x.IDMeetingTBL,
                        principalTable: "MeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetPollQuizAnsTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "525, 7"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 625),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    IDQuizTBL = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetPollQuizAnsTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetPollQuizAnsTBL_MeetPollQuizTBL_IDQuizTBL",
                        column: x => x.IDQuizTBL,
                        principalTable: "MeetPollQuizTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetPollQuizAnsTBLToUserMeetTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "525, 7"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 625),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IDUserTBLToMeetingTBL = table.Column<long>(type: "bigint", nullable: true),
                    IDMeetPollQuizAnsTBL = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetPollQuizAnsTBLToUserMeetTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetPollQuizAnsTBLToUserMeetTBL_MeetPollQuizAnsTBL_IDMeetPollQuizAnsTBL",
                        column: x => x.IDMeetPollQuizAnsTBL,
                        principalTable: "MeetPollQuizAnsTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetPollQuizAnsTBLToUserMeetTBL_UserTBLToMeetingTBL_IDUserTBLToMeetingTBL",
                        column: x => x.IDUserTBLToMeetingTBL,
                        principalTable: "UserTBLToMeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetPollQuizAnsTBL_IDQuizTBL",
                table: "MeetPollQuizAnsTBL",
                column: "IDQuizTBL");

            migrationBuilder.CreateIndex(
                name: "IX_MeetPollQuizAnsTBLToUserMeetTBL_IDMeetPollQuizAnsTBL",
                table: "MeetPollQuizAnsTBLToUserMeetTBL",
                column: "IDMeetPollQuizAnsTBL");

            migrationBuilder.CreateIndex(
                name: "IX_MeetPollQuizAnsTBLToUserMeetTBL_IDUserTBLToMeetingTBL",
                table: "MeetPollQuizAnsTBLToUserMeetTBL",
                column: "IDUserTBLToMeetingTBL");

            migrationBuilder.CreateIndex(
                name: "IX_MeetPollQuizTBL_IDMeetingTBL",
                table: "MeetPollQuizTBL",
                column: "IDMeetingTBL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetPollQuizAnsTBLToUserMeetTBL");

            migrationBuilder.DropTable(
                name: "MeetPollQuizAnsTBL");

            migrationBuilder.DropTable(
                name: "MeetPollQuizTBL");
        }
    }
}
