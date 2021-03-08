using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class MyThirdMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SocketId",
                table: "ConferenceHistory",
                newName: "ConnectionId");

            migrationBuilder.AddColumn<string>(
                name: "ConnectionId",
                table: "Conference",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConnectionId",
                table: "Conference");

            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "ConferenceHistory",
                newName: "SocketId");
        }
    }
}
