using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class March17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VClassInvitation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<string>(type: "nvarchar(750)", maxLength: 750, nullable: true),
                    BatchId = table.Column<long>(type: "bigint", nullable: false),
                    HostId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ParticipantId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    InvitationDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VClassInvitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VClassInvitation_AspNetUsers_HostId",
                        column: x => x.HostId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VClassInvitation_AspNetUsers_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VClassInvitation_Batch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "Batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VClassInvitation_BatchId",
                table: "VClassInvitation",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_VClassInvitation_HostId",
                table: "VClassInvitation",
                column: "HostId");

            migrationBuilder.CreateIndex(
                name: "IX_VClassInvitation_ParticipantId",
                table: "VClassInvitation",
                column: "ParticipantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VClassInvitation");
        }
    }
}
