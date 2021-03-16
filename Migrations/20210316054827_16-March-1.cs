using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Commander.Migrations
{
    public partial class _16March1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ActualCallDuration",
                table: "VClass",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EmptySlotDuration",
                table: "VClass",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HostCallDuration",
                table: "VClass",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParticipantJoined",
                table: "VClass",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ParticipantsCallDuration",
                table: "VClass",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UniqueParticipantCounts",
                table: "VClass",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualCallDuration",
                table: "VClass");

            migrationBuilder.DropColumn(
                name: "EmptySlotDuration",
                table: "VClass");

            migrationBuilder.DropColumn(
                name: "HostCallDuration",
                table: "VClass");

            migrationBuilder.DropColumn(
                name: "ParticipantJoined",
                table: "VClass");

            migrationBuilder.DropColumn(
                name: "ParticipantsCallDuration",
                table: "VClass");

            migrationBuilder.DropColumn(
                name: "UniqueParticipantCounts",
                table: "VClass");
        }
    }
}
