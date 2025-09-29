using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenJWT",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "IDCreator",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "JWT",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTBL_Username",
                schema: "GeneralInfo",
                table: "UserTBL",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTBL_IDCreator",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                column: "IDCreator");

            migrationBuilder.AddForeignKey(
                name: "FK_MeetingTBL_UserTBL_IDCreator",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                column: "IDCreator",
                principalSchema: "GeneralInfo",
                principalTable: "UserTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeetingTBL_UserTBL_IDCreator",
                schema: "GeneralInfo",
                table: "MeetingTBL");

            migrationBuilder.DropIndex(
                name: "IX_UserTBL_Username",
                schema: "GeneralInfo",
                table: "UserTBL");

            migrationBuilder.DropIndex(
                name: "IX_MeetingTBL_IDCreator",
                schema: "GeneralInfo",
                table: "MeetingTBL");

            migrationBuilder.DropColumn(
                name: "TokenJWT",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL");

            migrationBuilder.DropColumn(
                name: "IDCreator",
                schema: "GeneralInfo",
                table: "MeetingTBL");

            migrationBuilder.DropColumn(
                name: "JWT",
                schema: "GeneralInfo",
                table: "MeetingTBL");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
