using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _13March : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "Conference",
                newName: "HasJoined");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasJoined",
                table: "Conference",
                newName: "ConnectionId");
        }
    }
}
