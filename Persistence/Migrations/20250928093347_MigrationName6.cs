using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordHash",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "GeneralInfo",
                table: "UserTBL");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                schema: "GeneralInfo",
                table: "UserTBL");

            migrationBuilder.DropColumn(
                name: "Username",
                schema: "GeneralInfo",
                table: "UserTBL");
        }
    }
}
