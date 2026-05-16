using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db510 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelTBLToRoleTBL_RoleTBL_RoleTBLID",
                schema: "Ticket",
                table: "AccessLevelTBLToRoleTBL");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTBLToRoleTBL_UserTBL_UserID",
                schema: "Ticket",
                table: "UserTBLToRoleTBL");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelTBLToRoleTBL_RoleTBL_RoleTBLID",
                schema: "Ticket",
                table: "AccessLevelTBLToRoleTBL",
                column: "RoleTBLID",
                principalSchema: "Ticket",
                principalTable: "RoleTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTBLToRoleTBL_UserTBL_UserID",
                schema: "Ticket",
                table: "UserTBLToRoleTBL",
                column: "UserID",
                principalTable: "UserTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AccessLevelTBLToRoleTBL_RoleTBL_RoleTBLID",
                schema: "Ticket",
                table: "AccessLevelTBLToRoleTBL");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTBLToRoleTBL_UserTBL_UserID",
                schema: "Ticket",
                table: "UserTBLToRoleTBL");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessLevelTBLToRoleTBL_RoleTBL_RoleTBLID",
                schema: "Ticket",
                table: "AccessLevelTBLToRoleTBL",
                column: "RoleTBLID",
                principalSchema: "Ticket",
                principalTable: "RoleTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTBLToRoleTBL_UserTBL_UserID",
                schema: "Ticket",
                table: "UserTBLToRoleTBL",
                column: "UserID",
                principalTable: "UserTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
