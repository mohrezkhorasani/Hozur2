using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db141 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetCommentTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "525, 7"),
                    MeetID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 101),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 609),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDUserTBLToMeeting = table.Column<long>(type: "bigint", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetCommentTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetCommentTBL_UserTBLToMeetingTBL_IDUserTBLToMeeting",
                        column: x => x.IDUserTBLToMeeting,
                        principalTable: "UserTBLToMeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetQuestionTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "525, 7"),
                    MeetID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 611),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDUserTBLToMeeting = table.Column<long>(type: "bigint", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnswerAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EitaUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelegramUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetQuestionTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetQuestionTBL_UserTBLToMeetingTBL_IDUserTBLToMeeting",
                        column: x => x.IDUserTBLToMeeting,
                        principalTable: "UserTBLToMeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetCommentTBL_IDUserTBLToMeeting",
                table: "MeetCommentTBL",
                column: "IDUserTBLToMeeting");

            migrationBuilder.CreateIndex(
                name: "IX_MeetQuestionTBL_IDUserTBLToMeeting",
                table: "MeetQuestionTBL",
                column: "IDUserTBLToMeeting");

            migrationBuilder.CreateIndex(
                name: "IX_MeetQuestionTBL_PhoneNumber",
                table: "MeetQuestionTBL",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetCommentTBL");

            migrationBuilder.DropTable(
                name: "MeetQuestionTBL");
        }
    }
}
