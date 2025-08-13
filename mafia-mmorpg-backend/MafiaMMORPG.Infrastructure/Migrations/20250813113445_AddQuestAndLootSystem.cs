using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MafiaMMORPG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestAndLootSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerInventories_Items_ItemId",
                table: "PlayerInventories");

            migrationBuilder.DropIndex(
                name: "IX_PlayerInventories_PlayerId_ItemId",
                table: "PlayerInventories");

            migrationBuilder.AddColumn<int>(
                name: "RequiredLevel",
                table: "Quests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "PlayerInventories",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ItemDefinitionId",
                table: "PlayerInventories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ItemDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slot = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<string>(type: "text", nullable: false),
                    ItemLevel = table.Column<int>(type: "integer", nullable: false),
                    RequiredLevel = table.Column<int>(type: "integer", nullable: false),
                    BaseK = table.Column<int>(type: "integer", nullable: true),
                    BaseG = table.Column<int>(type: "integer", nullable: true),
                    BaseZ = table.Column<int>(type: "integer", nullable: true),
                    BaseH = table.Column<int>(type: "integer", nullable: true),
                    AffixJson = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_ItemDefinitionId",
                table: "PlayerInventories",
                column: "ItemDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_PlayerId_ItemDefinitionId",
                table: "PlayerInventories",
                columns: new[] { "PlayerId", "ItemDefinitionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerInventories_ItemDefinitions_ItemDefinitionId",
                table: "PlayerInventories",
                column: "ItemDefinitionId",
                principalTable: "ItemDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerInventories_Items_ItemId",
                table: "PlayerInventories",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerInventories_ItemDefinitions_ItemDefinitionId",
                table: "PlayerInventories");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerInventories_Items_ItemId",
                table: "PlayerInventories");

            migrationBuilder.DropTable(
                name: "ItemDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_PlayerInventories_ItemDefinitionId",
                table: "PlayerInventories");

            migrationBuilder.DropIndex(
                name: "IX_PlayerInventories_PlayerId_ItemDefinitionId",
                table: "PlayerInventories");

            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "ItemDefinitionId",
                table: "PlayerInventories");

            migrationBuilder.AlterColumn<Guid>(
                name: "ItemId",
                table: "PlayerInventories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventories_PlayerId_ItemId",
                table: "PlayerInventories",
                columns: new[] { "PlayerId", "ItemId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerInventories_Items_ItemId",
                table: "PlayerInventories",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
