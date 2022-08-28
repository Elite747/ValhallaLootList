// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class TotemsRule : Rule
{
    protected override ItemDetermination MakeDetermination(Item item, Specializations spec)
    {
        return item.Id switch
        {
            45114u when spec != Specializations.RestoShaman => new(spec, DeterminationLevel.Disallowed, "Steamcaller's Totem is meant for restoration shamans."),
            45255u when spec != Specializations.EleShaman => new(spec, DeterminationLevel.Disallowed, "Thunderfall Totem is meant for elemental shamans."),
            40322u when spec != Specializations.EnhanceShaman => new(spec, DeterminationLevel.Disallowed, "Totem of Dueling is meant for enhancement shamans."),
            40267u when spec != Specializations.EleShaman => new(spec, DeterminationLevel.Disallowed, "Totem of Hex is meant for elemental shamans."),
            39728u when spec != Specializations.RestoShaman => new(spec, DeterminationLevel.Disallowed, "Totem of Misery is meant for restoration shamans."),
            45169u when spec != Specializations.EnhanceShaman => new(spec, DeterminationLevel.Disallowed, "Totem of the Dancing Flame is meant for enhancement shamans."),
            _ => new(spec, DeterminationLevel.Allowed, string.Empty)
        };
    }

    protected override Specializations ApplicableSpecs() => SpecializationGroups.Shaman;

    protected override bool AppliesTo(Item item) => item.Type == ItemType.Totem;
}
