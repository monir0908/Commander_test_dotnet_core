using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _13March7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasJoined",
                table: "Conference",
                newName: "HasJoinedByParticipant");

            migrationBuilder.AddColumn<bool>(
                name: "HasJoinedByHost",
                table: "Conference",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasJoinedByHost",
                table: "Conference");

            migrationBuilder.RenameColumn(
                name: "HasJoinedByParticipant",
                table: "Conference",
                newName: "HasJoined");
        }
    }
}
