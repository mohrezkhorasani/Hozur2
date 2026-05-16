using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db502 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_RoomTBLPlanTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_RoomTBL_RoomFriendlyID",
                table: "MeetingTBL");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_UserTBL_IDCreator",
                table: "MeetingTBL");

            migrationBuilder.DropIndex(
                name: "IX_MeetingTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL",
                column: "IDRoomTBLPlanTBL");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_RoomTBLPlanTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL",
                column: "IDRoomTBLPlanTBL",
                principalTable: "RoomTBLPlanTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_RoomTBL_RoomFriendlyID",
                table: "MeetingTBL",
                column: "RoomFriendlyID",
                principalTable: "RoomTBL",
                principalColumn: "FriendlyID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_UserTBL_IDCreator",
                table: "MeetingTBL",
                column: "IDCreator",
                principalTable: "UserTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_RoomTBLPlanTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_RoomTBL_RoomFriendlyID",
                table: "MeetingTBL");

            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_UserTBL_IDCreator",
                table: "MeetingTBL");

            migrationBuilder.DropIndex(
                name: "IX_MeetingTBL_IDRoomTBLPlanTBL",
                table: "MeetingTBL");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_RoomTBL_RoomFriendlyID",
                table: "MeetingTBL",
                column: "RoomFriendlyID",
                principalTable: "RoomTBL",
                principalColumn: "FriendlyID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_UserTBL_IDCreator",
                table: "MeetingTBL",
                column: "IDCreator",
                principalTable: "UserTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
