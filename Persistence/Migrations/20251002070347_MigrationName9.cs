using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBL_UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs");

            migrationBuilder.DropColumn(
                name: "UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs");

            migrationBuilder.RenameTable(
                name: "UserTBLToMeetingTBLLogs",
                newName: "UserTBLToMeetingTBLLogs",
                newSchema: "GeneralInfo");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1111111, 2")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1111111, 7")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "RoomTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "25555, 57")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "598569, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "CertificateTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "111111111111, 777")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Explain",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateInsert",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1111112, 2")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBLLogs_IDUserTBLToMeetingTBL",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                column: "IDUserTBLToMeetingTBL");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBL_IDUserTBLToMeetingTBL",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs",
                column: "IDUserTBLToMeetingTBL",
                principalSchema: "GeneralInfo",
                principalTable: "UserTBLToMeetingTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBL_IDUserTBLToMeetingTBL",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserTBLToMeetingTBLLogs_IDUserTBLToMeetingTBL",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBLLogs");

            migrationBuilder.RenameTable(
                name: "UserTBLToMeetingTBLLogs",
                schema: "GeneralInfo",
                newName: "UserTBLToMeetingTBLLogs");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1111111, 2");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "UserTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1111111, 7");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "RoomTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "25555, 57");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "598569, 1");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                schema: "GeneralInfo",
                table: "CertificateTBL",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "111111111111, 777");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "UserTBLToMeetingTBLLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "UserTBLToMeetingTBLLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Explain",
                table: "UserTBLToMeetingTBLLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3000)",
                oldMaxLength: 3000,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateInsert",
                table: "UserTBLToMeetingTBLLogs",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSDATETIME()");

            migrationBuilder.AlterColumn<long>(
                name: "ID",
                table: "UserTBLToMeetingTBLLogs",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1111112, 2");

            migrationBuilder.AddColumn<long>(
                name: "UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs",
                column: "UserTBLToMeetingTBLID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTBLToMeetingTBLLogs_UserTBLToMeetingTBL_UserTBLToMeetingTBLID",
                table: "UserTBLToMeetingTBLLogs",
                column: "UserTBLToMeetingTBLID",
                principalSchema: "GeneralInfo",
                principalTable: "UserTBLToMeetingTBL",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
