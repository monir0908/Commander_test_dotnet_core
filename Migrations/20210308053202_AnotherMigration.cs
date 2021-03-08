using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class AnotherMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "ConferenceId",
                table: "ConferenceHistory",
                type: "float",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(750)",
                oldMaxLength: 750);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConferenceId",
                table: "ConferenceHistory",
                type: "nvarchar(750)",
                maxLength: 750,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}
