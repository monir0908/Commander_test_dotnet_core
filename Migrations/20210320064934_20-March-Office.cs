using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _20MarchOffice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BatchId",
                table: "ProjectBatchHost",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ProjectId",
                table: "ProjectBatchHost",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProjectIdId",
                table: "ProjectBatchHost",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBatchHost_BatchId",
                table: "ProjectBatchHost",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBatchHost_ProjectId",
                table: "ProjectBatchHost",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBatchHost_Batch_BatchId",
                table: "ProjectBatchHost",
                column: "BatchId",
                principalTable: "Batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectBatchHost_Project_ProjectId",
                table: "ProjectBatchHost",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBatchHost_Batch_BatchId",
                table: "ProjectBatchHost");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectBatchHost_Project_ProjectId",
                table: "ProjectBatchHost");

            migrationBuilder.DropIndex(
                name: "IX_ProjectBatchHost_BatchId",
                table: "ProjectBatchHost");

            migrationBuilder.DropIndex(
                name: "IX_ProjectBatchHost_ProjectId",
                table: "ProjectBatchHost");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "ProjectBatchHost");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ProjectBatchHost");

            migrationBuilder.DropColumn(
                name: "ProjectIdId",
                table: "ProjectBatchHost");
        }
    }
}
