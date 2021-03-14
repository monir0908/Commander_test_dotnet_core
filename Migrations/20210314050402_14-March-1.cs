using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _14March1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VClass",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: true),
                    HostId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BatchId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScheduleDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VClass_AspNetUsers_HostId",
                        column: x => x.HostId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VClass_Batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VClassDetail",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VClassId = table.Column<long>(type: "bigint", nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: true),
                    HostId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ConnectionId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    JoinTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeaveTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VClassDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VClassDetail_AspNetUsers_HostId",
                        column: x => x.HostId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VClassDetail_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VClass_BatchId",
                table: "VClass",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_VClass_HostId",
                table: "VClass",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_VClassDetail_HostId",
                table: "VClassDetail",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_VClassDetail_ParticipantId",
                table: "VClassDetail",
                column: "ParticipantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VClass");

            migrationBuilder.DropTable(
                name: "VClassDetail");
        }
    }
}
