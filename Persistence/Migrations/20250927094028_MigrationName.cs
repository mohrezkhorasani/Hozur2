using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MigrationName : Migration
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
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigTBL", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RoomTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTBL", x => x.ID);
                    table.UniqueConstraint("AK_RoomTBL_FriendlyID", x => x.FriendlyID);
                });

            migrationBuilder.CreateTable(
                name: "UserTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllowedGUID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Firstname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UUID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExtId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPList = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTBL", x => x.ID);
                    table.UniqueConstraint("AK_UserTBL_UUID", x => x.UUID);
                });

            migrationBuilder.CreateTable(
                name: "MeetingTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeetID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JalaliDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomFriendlyID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IDTeacher = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingTBL", x => x.ID);
                    table.UniqueConstraint("AK_MeetingTBL_MeetID", x => x.MeetID);
                    table.ForeignKey(
                        name: "FK_MeetingTBL_RoomTBL_RoomFriendlyID",
                        column: x => x.RoomFriendlyID,
                        principalSchema: "GeneralInfo",
                        principalTable: "RoomTBL",
                        principalColumn: "FriendlyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingTBL_UserTBL_IDTeacher",
                        column: x => x.IDTeacher,
                        principalSchema: "GeneralInfo",
                        principalTable: "UserTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTBLToMeetingTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    MeetID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UUID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsCertificated = table.Column<bool>(type: "bit", nullable: false),
                    TimeSpendSeconds = table.Column<long>(type: "bigint", nullable: false),
                    CertificateFileURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IPList = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomTBLID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTBLToMeetingTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserTBLToMeetingTBL_MeetingTBL_MeetID",
                        column: x => x.MeetID,
                        principalSchema: "GeneralInfo",
                        principalTable: "MeetingTBL",
                        principalColumn: "MeetID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserTBLToMeetingTBL_RoomTBL_RoomTBLID",
                        column: x => x.RoomTBLID,
                        principalSchema: "GeneralInfo",
                        principalTable: "RoomTBL",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_UserTBLToMeetingTBL_UserTBL_UUID",
                        column: x => x.UUID,
                        principalSchema: "GeneralInfo",
                        principalTable: "UserTBL",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CertificateTBL",
                schema: "GeneralInfo",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Explain = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    DateInsert = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IDUserTBLToMeetingTBL = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Validation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateTBL", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CertificateTBL_UserTBLToMeetingTBL_IDUserTBLToMeetingTBL",
                        column: x => x.IDUserTBLToMeetingTBL,
                        principalSchema: "GeneralInfo",
                        principalTable: "UserTBLToMeetingTBL",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateTBL_IDUserTBLToMeetingTBL",
                schema: "GeneralInfo",
                table: "CertificateTBL",
                column: "IDUserTBLToMeetingTBL",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTBL_IDTeacher",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                column: "IDTeacher");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTBL_MeetID",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                column: "MeetID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeetingTBL_RoomFriendlyID",
                schema: "GeneralInfo",
                table: "MeetingTBL",
                column: "RoomFriendlyID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBL_FriendlyID",
                schema: "GeneralInfo",
                table: "RoomTBL",
                column: "FriendlyID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomTBL_Title",
                schema: "GeneralInfo",
                table: "RoomTBL",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTBL_Email",
                schema: "GeneralInfo",
                table: "UserTBL",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBL_NationalCode",
                schema: "GeneralInfo",
                table: "UserTBL",
                column: "NationalCode",
                unique: true,
                filter: "[NationalCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBL_PhoneNumber",
                schema: "GeneralInfo",
                table: "UserTBL",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBL_UUID",
                schema: "GeneralInfo",
                table: "UserTBL",
                column: "UUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBL_MeetID",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                column: "MeetID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBL_RoomTBLID",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                column: "RoomTBLID");

            migrationBuilder.CreateIndex(
                name: "IX_UserTBLToMeetingTBL_UUID_MeetID",
                schema: "GeneralInfo",
                table: "UserTBLToMeetingTBL",
                columns: new[] { "UUID", "MeetID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateTBL",
                schema: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "ConfigTBL",
                schema: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "UserTBLToMeetingTBL",
                schema: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "MeetingTBL",
                schema: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "RoomTBL",
                schema: "GeneralInfo");

            migrationBuilder.DropTable(
                name: "UserTBL",
                schema: "GeneralInfo");
        }
    }
}
