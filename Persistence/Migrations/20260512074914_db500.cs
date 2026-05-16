using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db500 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IDRoomTBLPlanTBL",
                table: "MeetingTBL",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL",
                column: "IDRoomTBLPlanTBL",
                unique: true,
                filter: "[IDRoomTBLPlanTBL] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_RoomTBLPlanTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL",
                column: "IDRoomTBLPlanTBL",
                principalTable: "RoomTBLPlanTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_RoomTBLPlanTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL");

            migrationBuilder.DropIndex(
                name: "IX_MeetingTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL");

            migrationBuilder.DropColumn(
                name: "IDRoomTBLPlanTBL",
                table: "MeetingTBL");
        }
    }
}
