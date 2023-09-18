// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class SigilsRule : Rule
{
    protected override ItemDetermination MakeDetermination(Item item, Specializations spec)
    {
        return item.Id switch
        {
            45144u when spec is not Specializations.BloodDeathKnightTank and not Specializations.FrostDeathKnightTank and not Specializations.UnholyDeathKnightTank => new(spec, DeterminationLevel.Disallowed, "Sigil of Deflection is meant for tanking death knights."),
            _ => new(spec, DeterminationLevel.Allowed, string.Empty)
        };
    }

    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.DeathKnight;
    }

    protected override bool AppliesTo(Item item)
    {
        return item.Type == ItemType.Sigil;
    }
}
