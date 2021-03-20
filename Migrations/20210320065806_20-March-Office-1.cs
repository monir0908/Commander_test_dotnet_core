using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _20MarchOffice1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectIdId",
                table: "ProjectBatchHost");

            migrationBuilder.AlterColumn<long>(
                name: "ProjectId",
                table: "ProjectBatchHost",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ProjectId",
                table: "ProjectBatchHost",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ProjectIdId",
                table: "ProjectBatchHost",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
