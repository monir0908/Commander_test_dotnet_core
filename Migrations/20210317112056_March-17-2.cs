using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class March172 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VClassId",
                table: "VClassInvitation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VClassId",
                table: "VClassInvitation");
        }
    }
}
