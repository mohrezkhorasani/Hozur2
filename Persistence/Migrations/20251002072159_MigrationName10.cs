using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "IDOwnerTBL",
                schema: "GeneralInfo",
                table: "RoomTBL",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "RoomTBLToUserTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 3"),
                    FriendlyID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    UUIDParticipatorTBL = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FriendlyIDRoomTBL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTBLToUserTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RoomTBLToUserTBL_RoomTBL_FriendlyID",
                        column: x => x.FriendlyID,
                        principalSchema: "GeneralInfo",
                        principalTable: "RoomTBL",
                        principalColumn: "FriendlyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomTBLToUserTBL_UserTBL_UUIDParticipatorTBL",
                        column: x => x.UUIDParticipatorTBL,
                        principalSchema: "GeneralInfo",
                        principalTable: "UserTBL",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBL_IDOwnerTBL",
                schema: "GeneralInfo",
                table: "RoomTBL",
                column: "IDOwnerTBL");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBLToUserTBL_FriendlyID",
                schema: "GeneralInfo",
                table: "RoomTBLToUserTBL",
                column: "FriendlyID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBLToUserTBL_UUIDParticipatorTBL",
                schema: "GeneralInfo",
                table: "RoomTBLToUserTBL",
                column: "UUIDParticipatorTBL");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomTBL_UserTBL_IDOwnerTBL",
                schema: "GeneralInfo",
                table: "RoomTBL",
                column: "IDOwnerTBL",
                principalSchema: "GeneralInfo",
                principalTable: "UserTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomTBL_UserTBL_IDOwnerTBL",
                schema: "GeneralInfo",
                table: "RoomTBL");

            migrationBuilder.DropTable(
                name: "RoomTBLToUserTBL",
                schema: "GeneralInfo");

            migrationBuilder.DropIndex(
                name: "IX_RoomTBL_IDOwnerTBL",
                schema: "GeneralInfo",
                table: "RoomTBL");

            migrationBuilder.DropColumn(
                name: "IDOwnerTBL",
                schema: "GeneralInfo",
                table: "RoomTBL");
        }
    }
}
