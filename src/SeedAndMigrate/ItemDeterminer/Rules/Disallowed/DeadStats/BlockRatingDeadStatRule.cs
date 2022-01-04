// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class BlockRatingDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~(Specializations.ProtPaladin | Specializations.ProtWarrior);

    protected override int GetStat(Item item) => item.BlockRating;

    protected override string GetStatDisplayName() => "Block Rating";
}
