// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class LibramsRule : Rule
{
    protected override ItemDetermination MakeDetermination(Item item, Specializations spec)
    {
        return item.Id switch
        {
            45510u when spec != Specializations.RetPaladin => new(spec, DeterminationLevel.Disallowed, "Libram of Discord is meant for retribution paladins."),
            40191u when spec != Specializations.RetPaladin => new(spec, DeterminationLevel.Disallowed, "Libram of Radiance is meant for retribution paladins."),
            40337u when spec != Specializations.ProtPaladin => new(spec, DeterminationLevel.Disallowed, "Libram of Resurgence is meant for protection paladins."),
            45436u when spec != Specializations.HolyPaladin => new(spec, DeterminationLevel.Disallowed, "Libram of the Resolute is meant for holy paladins."),
            45145u when spec != Specializations.ProtPaladin => new(spec, DeterminationLevel.Disallowed, "Libram of the Sacred Shield is meant for protection paladins."),
            40268u when spec != Specializations.HolyPaladin => new(spec, DeterminationLevel.Disallowed, "Libram of Tolerance is meant for holy paladins."),
            _ => new(spec, DeterminationLevel.Allowed, string.Empty)
        };
    }

    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.Paladin;
    }

    protected override bool AppliesTo(Item item)
    {
        return item.Type == ItemType.Libram;
    }
}
