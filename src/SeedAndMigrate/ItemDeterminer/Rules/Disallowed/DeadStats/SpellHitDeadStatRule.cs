// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class SpellHitDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~(SpecializationGroups.CasterDps | Specializations.ProtPaladin);

    protected override int GetStat(Item item) => item.SpellHit;

    protected override string GetStatDisplayName() => "Spell Hit";
}
