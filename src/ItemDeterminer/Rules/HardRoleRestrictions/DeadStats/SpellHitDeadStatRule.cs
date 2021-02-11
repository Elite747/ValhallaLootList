// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.DeadStats
{
    internal class SpellHitDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.All & ~Specializations.CasterDps;

        protected override int GetStat(Item item) => item.SpellHit;

        protected override string GetStatDisplayName() => "Spell Hit";
    }
}
