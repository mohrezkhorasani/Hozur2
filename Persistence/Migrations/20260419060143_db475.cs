using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db475 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRevokeTokenTBL_Token",
                table: "UserRevokeTokenTBL");

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "UserRevokeTokenTBL",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_UserRevokeTokenTBL_TokenHash",
                table: "UserRevokeTokenTBL",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRevokeTokenTBL_TokenHash",
                table: "UserRevokeTokenTBL");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "UserRevokeTokenTBL");

            migrationBuilder.CreateIndex(
                name: "IX_UserRevokeTokenTBL_Token",
                table: "UserRevokeTokenTBL",
                column: "Token",
                unique: true);
        }
    }
}
