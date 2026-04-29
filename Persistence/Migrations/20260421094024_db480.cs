using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db480 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTBLToUserTBL",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "11111111, 4"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    IDFollower = table.Column<long>(type: "bigint", nullable: false),
                    IDFollowing = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTBLToUserTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserTBLToUserTBL_UserTBL_IDFollower",
                        column: x => x.IDFollower,
                        principalTable: "UserTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTBLToUserTBL_UserTBL_IDFollowing",
                        column: x => x.IDFollowing,
                        principalTable: "UserTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToUserTBL_IDFollower",
                table: "UserTBLToUserTBL",
                column: "IDFollower");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToUserTBL_IDFollowing",
                table: "UserTBLToUserTBL",
                column: "IDFollowing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTBLToUserTBL");
        }
    }
}
