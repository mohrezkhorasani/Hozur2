using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class db1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "GeneralInfo");

            migrationBuilder.CreateTable(
                name: "ConfigTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "CONVERT(varchar, SYSDATETIME(), 121)"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigTBL", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigTBL",
                schema: "GeneralInfo");
        }
    }
}
