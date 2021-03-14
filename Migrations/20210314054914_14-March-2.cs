using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _14March2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "BatchId",
                table: "VClassDetail",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_VClassDetail_BatchId",
                table: "VClassDetail",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_VClassDetail_Batch_BatchId",
                table: "VClassDetail",
                column: "BatchId",
                principalTable: "Batch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VClassDetail_Batch_BatchId",
                table: "VClassDetail");

            migrationBuilder.DropIndex(
                name: "IX_VClassDetail_BatchId",
                table: "VClassDetail");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "VClassDetail");
        }
    }
}
