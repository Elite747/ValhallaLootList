// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class ShadowPowerDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.All & ~(Specializations.ShadowPriest | Specializations.Warlock);

        protected override int GetStat(Item item)
        {
            return item.Id switch
            {
                32590u => 53, // Nethervoid Cloak
                30050u => 59, // Boots of the Shifting Nightmare
                _ => 0
            };
        }

        protected override string GetStatDisplayName() => "Shadow Power";
    }
}
