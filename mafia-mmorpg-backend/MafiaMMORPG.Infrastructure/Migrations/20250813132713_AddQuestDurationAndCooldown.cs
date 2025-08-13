using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MafiaMMORPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestDurationAndCooldown : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CooldownMinutes",
                table: "Quests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Quests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CooldownUntil",
                table: "PlayerQuests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CooldownMinutes",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "CooldownUntil",
                table: "PlayerQuests");
        }
    }
}
