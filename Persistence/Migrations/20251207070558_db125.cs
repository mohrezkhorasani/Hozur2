using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db125 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Ticket");

            migrationBuilder.RenameTable(
                name: "ConfigTBL",
                newName: "ConfigTBL",
                newSchema: "Ticket");

            migrationBuilder.AddColumn<string>(
                name: "ModeratorCode",
                table: "RoomTBL",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentationPath",
                table: "RoomTBL",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecordRoom",
                table: "RoomTBL",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiredAdminAcceptToView",
                table: "RoomTBL",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequiredLoginToView",
                table: "RoomTBL",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ViewerCode",
                table: "RoomTBL",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "Ticket",
                table: "ConfigTBL",
                type: "int",
                nullable: false,
                defaultValue: 101,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AccessLevelTBL",
                schema: "Ticket",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 101),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    DateUpdate = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Route = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelTBL", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RoleTBL",
                schema: "Ticket",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 101),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleTBL", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AccessLevelTBLToRoleTBL",
                schema: "Ticket",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 101),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    DateUpdate = table.Column<long>(type: "bigint", nullable: false),
                    RoleTBLID = table.Column<long>(type: "bigint", nullable: false),
                    IDRoleTBL = table.Column<long>(type: "bigint", nullable: false),
                    IDAccessLevelTBL = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessLevelTBLToRoleTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AccessLevelTBLToRoleTBL_AccessLevelTBL_IDAccessLevelTBL",
                        column: x => x.IDAccessLevelTBL,
                        principalSchema: "Ticket",
                        principalTable: "AccessLevelTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccessLevelTBLToRoleTBL_RoleTBL_RoleTBLID",
                        column: x => x.RoleTBLID,
                        principalSchema: "Ticket",
                        principalTable: "RoleTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTBLToRoleTBL",
                schema: "Ticket",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 101),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IDUserTBL = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    IDRoleTBL = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTBLToRoleTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserTBLToRoleTBL_RoleTBL_IDRoleTBL",
                        column: x => x.IDRoleTBL,
                        principalSchema: "Ticket",
                        principalTable: "RoleTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTBLToRoleTBL_UserTBL_UserID",
                        column: x => x.UserID,
                        principalTable: "UserTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelTBLToRoleTBL_IDAccessLevelTBL",
                schema: "Ticket",
                table: "AccessLevelTBLToRoleTBL",
                column: "IDAccessLevelTBL");

            migrationBuilder.CreateIndex(
                name: "IX_AccessLevelTBLToRoleTBL_RoleTBLID",
                schema: "Ticket",
                table: "AccessLevelTBLToRoleTBL",
                column: "RoleTBLID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToRoleTBL_IDRoleTBL",
                schema: "Ticket",
                table: "UserTBLToRoleTBL",
                column: "IDRoleTBL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToRoleTBL_UserID",
                schema: "Ticket",
                table: "UserTBLToRoleTBL",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessLevelTBLToRoleTBL",
                schema: "Ticket");

            migrationBuilder.DropTable(
                name: "UserTBLToRoleTBL",
                schema: "Ticket");

            migrationBuilder.DropTable(
                name: "AccessLevelTBL",
                schema: "Ticket");

            migrationBuilder.DropTable(
                name: "RoleTBL",
                schema: "Ticket");

            migrationBuilder.DropColumn(
                name: "ModeratorCode",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "PresentationPath",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "RecordRoom",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "RequiredAdminAcceptToView",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "RequiredLoginToView",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "ViewerCode",
                table: "RoomTBL");

            migrationBuilder.RenameTable(
                name: "ConfigTBL",
                schema: "Ticket",
                newName: "ConfigTBL");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ConfigTBL",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 101);
        }
    }
}
