using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db511 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MeetingReservationTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "598569, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IDRoomTBLPlanTBL = table.Column<long>(type: "bigint", nullable: false),
                    IDUserTBL = table.Column<long>(type: "bigint", nullable: true),
                    TokenJWT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserNickName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingReservationTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MeetingReservationTBL_RoomTBLPlanTBL_IDRoomTBLPlanTBL",
                        column: x => x.IDRoomTBLPlanTBL,
                        principalTable: "RoomTBLPlanTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingReservationTBL_UserTBL_IDUserTBL",
                        column: x => x.IDUserTBL,
                        principalTable: "UserTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReservationTBL_IDRoomTBLPlanTBL",
                table: "MeetingReservationTBL",
                column: "IDRoomTBLPlanTBL");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReservationTBL_IDUserTBL",
                table: "MeetingReservationTBL",
                column: "IDUserTBL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingReservationTBL");
        }
    }
}
