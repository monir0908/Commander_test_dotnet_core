using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _10MarMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ConferenceId",
                table: "ConferenceHistory",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "ConferenceId",
                table: "ConferenceHistory",
                type: "float",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
