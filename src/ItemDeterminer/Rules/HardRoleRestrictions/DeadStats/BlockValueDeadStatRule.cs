// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.DeadStats
{
    internal class BlockValueDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.All & ~(Specializations.ProtPaladin | Specializations.ProtWarrior);

        protected override int GetStat(Item item) => item.BlockValue;

        protected override string GetStatDisplayName() => "Block";
    }
}
