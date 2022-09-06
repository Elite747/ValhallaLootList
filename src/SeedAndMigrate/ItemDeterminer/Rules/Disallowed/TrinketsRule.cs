// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class TrinketsRule : Rule
{
    private readonly List<TrinketInfo> _trinkets = new()
    {
        new(EffectCategory.Healer, "Althor's Abacus", 50359, 50366),
        new(EffectCategory.Physical, "Bandit's Insignia", 40371),
        new(EffectCategory.Healer, "Bauble of True Blood", 50354, 50726),
        new(EffectCategory.Healer, "Binding Light", 47728, 47947),
        new(EffectCategory.Physical, "Blood of the Old God", 45522),
        new(EffectCategory.Caster, "Charred Twilight Scale", 54572, 54588),
        new(EffectCategory.Physical, "Comet's Trail", 45609),
        new(EffectCategory.Tank, "Corpse Tongue Coin", 50349, 50352),
        new(EffectCategory.Physical, "Dark Matter", 46038),
        new(EffectCategory.Physical, "Death's Verdict", 47115, 47131),
        new(EffectCategory.Physical, "Deathbringer's Will", 50362, 50363),
        new(EffectCategory.Tank, "Defender's Code", 40257),
        new(EffectCategory.Caster, "Dislodged Foreign Object", 50348, 50353),
        new(EffectCategory.Caster, "Dying Curse", 40255),
        new(EffectCategory.Caster, "Elemental Focus Stone", 45866),
        new(EffectCategory.CasterOrHealer, "Embrace of the Spider", 39229),
        new(EffectCategory.CasterOrHealer, "Energy Siphon", 45292),
        new(EffectCategory.Caster, "Extract of Necromantic Power", 40373),
        new(EffectCategory.CasterOrHealer, "Eye of the Broodmother", 45308),
        new(EffectCategory.Tank, "Fervor of the Frostborn", 47727, 47949),
        new(EffectCategory.Caster, "Flare of the Heavens", 45518),
        new(EffectCategory.Healer, "Forethought Talisman", 40258),
        new(EffectCategory.Tank, "Furnace Stone", 45313),
        new(EffectCategory.Physical, "Fury of the Five Flights", 40431),
        new(EffectCategory.Healer, "Glowing Twilight Scale", 54573, 54589),
        new(EffectCategory.Physical, "Grim Toll", 40256),
        new(EffectCategory.Tank, "Heart of Iron", 45158),
        new(EffectCategory.CasterOrHealer, "Illustration of the Dragon Soul", 40432),
        new(EffectCategory.Caster, "Living Flame", 45148),
        new(EffectCategory.Healer, "Living Ice Crystals", 40532),
        new(EffectCategory.Physical, "Loatheb's Shadow", 39257),
        new(EffectCategory.CasterOrHealer, "Majestic Dragon Figurine", 40430),
        //new(EffectCategory., "Mark of Norgannon", 40531),
        new(EffectCategory.CasterOrHealer, "Meteorite Crystal", 46051),
        new(EffectCategory.Physical, "Mjolnir Runestone", 45931),
        new(EffectCategory.Caster, "Muradin's Spyglass", 50340, 50345),
        new(EffectCategory.CasterOrHealer, "Pandora's Plea", 45490),
        new(EffectCategory.Tank, "Petrified Twilight Scale", 54571, 54591),
        new(EffectCategory.Caster, "Phylactery of the Nameless Lich", 50360, 50365),
        new(EffectCategory.Physical, "Pyrite Infuser", 45286),
        new(EffectCategory.Caster, "Reign of the Unliving", 47182, 47188),
        new(EffectCategory.Tank, "Repelling Charge", 39292),
        new(EffectCategory.Tank, "Royal Seal of King Llane", 46021),
        new(EffectCategory.Tank, "Rune of Repulsion", 40372),
        new(EffectCategory.Tank, "Satrina's Impeding Scarab", 47080, 47088),
        new(EffectCategory.CasterOrHealer, "Scale of Fates", 45466),
        new(EffectCategory.Physical, "Sharpened Twilight Scale", 54569, 54590),
        new(EffectCategory.CasterOrHealer, "Show of Faith", 45535),
        new(EffectCategory.CasterOrHealer, "Sif's Remembrance", 45929),
        new(EffectCategory.Tank, "Sindragosa's Flawless Fang", 50361, 50364),
        new(EffectCategory.CasterOrHealer, "Sliver of Pure Ice", 50339, 50346),
        new(EffectCategory.CasterOrHealer, "Solace of the Defeated", 47041, 47059),
        new(EffectCategory.CasterOrHealer, "Soul of the Dead", 40382),
        new(EffectCategory.CasterOrHealer, "Spark of Hope", 45703),
        new(EffectCategory.CasterOrHealer, "Spirit-World Glass", 39388),
        new(EffectCategory.Caster, "Talisman of Volatile Power", 47726, 47946),
        new(EffectCategory.Tank, "The General's Heart", 45507),
        new(EffectCategory.Melee, "Tiny Abomination in a Jar", 50351, 50706),
        new(EffectCategory.Tank, "Unidentifiable Organ", 50341, 50344),
        //new(EffectCategory., "Vanquished Clutches of Yogg-Saron", 46312),
        new(EffectCategory.Melee, "Victor's Call", 47725, 47948),
        new(EffectCategory.Physical, "Whispering Fanged Skull", 50342, 50343),
        new(EffectCategory.Physical, "Wrathstone", 45263),
    };

    protected override ItemDetermination MakeDetermination(Item item, Specializations spec)
    {
        if (_trinkets.Find(t => t.Id == item.Id || t.Id2 == item.Id) is { } trinket)
        {
            Specializations allowedSpecs = trinket.Category switch
            {
                EffectCategory.Healer => SpecializationGroups.Healer,
                EffectCategory.Tank => SpecializationGroups.Tank,
                EffectCategory.Caster => SpecializationGroups.CasterDps,
                EffectCategory.Physical => SpecializationGroups.PhysicalDps,
                EffectCategory.Melee => SpecializationGroups.MeleeDps,
                EffectCategory.CasterOrHealer => SpecializationGroups.CasterDps | SpecializationGroups.Healer,
                _ => throw new NotSupportedException()
            };

            if ((allowedSpecs & spec) != spec)
            {
                return new(spec, DeterminationLevel.Disallowed, $"{trinket.Name} has a {GetEffectDisplay(trinket.Category)} effect.");
            }
        }
        return new(spec, DeterminationLevel.Allowed, string.Empty);
    }

    protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.Trinket;

    private static string GetEffectDisplay(EffectCategory category) => category switch
    {
        EffectCategory.Healer => "healing",
        EffectCategory.Tank => "tanking",
        EffectCategory.Caster => "damaging spell",
        EffectCategory.Physical => "physical damage",
        EffectCategory.Melee => "melee damage",
        EffectCategory.CasterOrHealer => "spell",
        _ => throw new NotSupportedException()
    };

    private record TrinketInfo(EffectCategory Category, string Name, uint Id, uint? Id2 = null);

    private enum EffectCategory
    {
        Healer,
        Tank,
        Caster,
        Physical,
        Melee,
        CasterOrHealer
    }
}
