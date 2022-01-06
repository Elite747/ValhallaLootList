// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore.Migrations;

namespace ValhallaLootList.Server.Migrations;

public partial class ChangePassRelationship : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Characters_RaidTeams_TeamId",
            table: "Characters");

        migrationBuilder.DropForeignKey(
            name: "FK_Drops_Items_ItemId",
            table: "Drops");

        migrationBuilder.DropForeignKey(
            name: "FK_EncounterKills_Encounters_EncounterId",
            table: "EncounterKills");

        migrationBuilder.DropForeignKey(
            name: "FK_Encounters_Instances_InstanceId",
            table: "Encounters");

        migrationBuilder.DropForeignKey(
            name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
            table: "LootListEntries");

        migrationBuilder.DropForeignKey(
            name: "FK_RaidAttendees_Characters_CharacterId",
            table: "RaidAttendees");

        migrationBuilder.DropForeignKey(
            name: "FK_Raids_RaidTeams_RaidTeamId",
            table: "Raids");

        migrationBuilder.AddColumn<long>(
            name: "LootListEntryId",
            table: "DropPasses",
            type: "bigint",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_DropPasses_LootListEntryId",
            table: "DropPasses",
            column: "LootListEntryId");

        migrationBuilder.AddForeignKey(
            name: "FK_Characters_RaidTeams_TeamId",
            table: "Characters",
            column: "TeamId",
            principalTable: "RaidTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_DropPasses_LootListEntries_LootListEntryId",
            table: "DropPasses",
            column: "LootListEntryId",
            principalTable: "LootListEntries",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Drops_Items_ItemId",
            table: "Drops",
            column: "ItemId",
            principalTable: "Items",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_EncounterKills_Encounters_EncounterId",
            table: "EncounterKills",
            column: "EncounterId",
            principalTable: "Encounters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Encounters_Instances_InstanceId",
            table: "Encounters",
            column: "InstanceId",
            principalTable: "Instances",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
            table: "LootListEntries",
            columns: new[] { "LootListCharacterId", "LootListPhase" },
            principalTable: "CharacterLootLists",
            principalColumns: new[] { "CharacterId", "Phase" },
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_RaidAttendees_Characters_CharacterId",
            table: "RaidAttendees",
            column: "CharacterId",
            principalTable: "Characters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Raids_RaidTeams_RaidTeamId",
            table: "Raids",
            column: "RaidTeamId",
            principalTable: "RaidTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.Sql(@"update dp
set dp.LootListEntryId = topEnt.Id
from DropPasses dp
inner join Drops d on dp.DropId = d.Id
inner join (
	select ent.Id, ent.CharacterId, ent.ItemId
	from (
		select lle.Id, CharacterId = lle.LootListCharacterId, ItemId = COALESCE(item.RewardFromId, lle.ItemId), lle.[Rank], Height = ROW_NUMBER() over (partition by lle.LootListCharacterId order by lle.[Rank] desc)
		from LootListEntries lle
		inner join Items item on item.Id = lle.ItemId
		where lle.DropId is null
	) ent
	where ent.Height = 1
) topEnt on topEnt.ItemId = d.ItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Characters_RaidTeams_TeamId",
            table: "Characters");

        migrationBuilder.DropForeignKey(
            name: "FK_DropPasses_LootListEntries_LootListEntryId",
            table: "DropPasses");

        migrationBuilder.DropForeignKey(
            name: "FK_Drops_Items_ItemId",
            table: "Drops");

        migrationBuilder.DropForeignKey(
            name: "FK_EncounterKills_Encounters_EncounterId",
            table: "EncounterKills");

        migrationBuilder.DropForeignKey(
            name: "FK_Encounters_Instances_InstanceId",
            table: "Encounters");

        migrationBuilder.DropForeignKey(
            name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
            table: "LootListEntries");

        migrationBuilder.DropForeignKey(
            name: "FK_RaidAttendees_Characters_CharacterId",
            table: "RaidAttendees");

        migrationBuilder.DropForeignKey(
            name: "FK_Raids_RaidTeams_RaidTeamId",
            table: "Raids");

        migrationBuilder.DropIndex(
            name: "IX_DropPasses_LootListEntryId",
            table: "DropPasses");

        migrationBuilder.DropColumn(
            name: "LootListEntryId",
            table: "DropPasses");

        migrationBuilder.AddForeignKey(
            name: "FK_Characters_RaidTeams_TeamId",
            table: "Characters",
            column: "TeamId",
            principalTable: "RaidTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Drops_Items_ItemId",
            table: "Drops",
            column: "ItemId",
            principalTable: "Items",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_EncounterKills_Encounters_EncounterId",
            table: "EncounterKills",
            column: "EncounterId",
            principalTable: "Encounters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Encounters_Instances_InstanceId",
            table: "Encounters",
            column: "InstanceId",
            principalTable: "Instances",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_LootListEntries_CharacterLootLists_LootListCharacterId_LootListPhase",
            table: "LootListEntries",
            columns: new[] { "LootListCharacterId", "LootListPhase" },
            principalTable: "CharacterLootLists",
            principalColumns: new[] { "CharacterId", "Phase" },
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_RaidAttendees_Characters_CharacterId",
            table: "RaidAttendees",
            column: "CharacterId",
            principalTable: "Characters",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Raids_RaidTeams_RaidTeamId",
            table: "Raids",
            column: "RaidTeamId",
            principalTable: "RaidTeams",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
