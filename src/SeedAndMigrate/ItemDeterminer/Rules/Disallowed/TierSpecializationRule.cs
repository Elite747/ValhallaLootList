// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed
{
    internal class TierSpecializationRule : SimpleRule
    {
        private static readonly Dictionary<uint, Specializations> _tierSpecs = new()
        {
            // Warrior
            // Tier 4
            [29019] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Warbringer Breastplate (Warrior)
            [29022] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Warbringer Greaves (Warrior)
            [29023] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Warbringer Shoulderplates (Warrior)
            [29012] = Specializations.ProtWarrior, // Warbringer Chestguard (Warrior)
            [29015] = Specializations.ProtWarrior, // Warbringer Legguards (Warrior)
            [29016] = Specializations.ProtWarrior, // Warbringer Shoulderguards (Warrior)

            // Tier 5
            [30120] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Destroyer Battle-Helm (Warrior)
            [30118] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Destroyer Breastplate (Warrior)
            [30119] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Destroyer Gauntlets (Warrior)
            [30121] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Destroyer Greaves (Warrior)
            [30122] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Destroyer Shoulderblades (Warrior)
            [30113] = Specializations.ProtWarrior, // Destroyer Chestguard (Warrior)
            [30115] = Specializations.ProtWarrior, // Destroyer Greathelm (Warrior)
            [30114] = Specializations.ProtWarrior, // Destroyer Handguards (Warrior)
            [30116] = Specializations.ProtWarrior, // Destroyer Legguards (Warrior)
            [30117] = Specializations.ProtWarrior, // Destroyer Shoulderguards (Warrior)

            // Tier 6
            [30972] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Battle-Helm (Warrior)
            [30975] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Breastplate (Warrior)
            [30969] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Gauntlets (Warrior)
            [30977] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Greaves (Warrior)
            [30979] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Shoulderblades (Warrior)
            [34546] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Belt (Warrior)
            [34441] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Bracers (Warrior)
            [34569] = Specializations.ArmsWarrior | Specializations.FuryWarrior, // Onslaught Treads (Warrior)
            [30976] = Specializations.ProtWarrior, // Onslaught Chestguard (Warrior)
            [30974] = Specializations.ProtWarrior, // Onslaught Greathelm (Warrior)
            [30970] = Specializations.ProtWarrior, // Onslaught Handguards (Warrior)
            [30978] = Specializations.ProtWarrior, // Onslaught Legguards (Warrior)
            [30980] = Specializations.ProtWarrior, // Onslaught Shoulderguards (Warrior)
            [34568] = Specializations.ProtWarrior, // Onslaught Boots (Warrior)
            [34547] = Specializations.ProtWarrior, // Onslaught Waistguard (Warrior)
            [34442] = Specializations.ProtWarrior, // Onslaught Wristguards (Warrior)

            // Paladin
            // Tier 4
            [29071] = Specializations.RetPaladin, // Justicar Breastplate (Paladin)
            [29074] = Specializations.RetPaladin, // Justicar Greaves (Paladin)
            [29075] = Specializations.RetPaladin, // Justicar Shoulderplates (Paladin)
            [29066] = Specializations.ProtPaladin, // Justicar Chestguard (Paladin)
            [29069] = Specializations.ProtPaladin, // Justicar Legguards (Paladin)
            [29070] = Specializations.ProtPaladin, // Justicar Shoulderguards (Paladin)
            [29062] = Specializations.HolyPaladin, // Justicar Chestpiece (Paladin)
            [29063] = Specializations.HolyPaladin, // Justicar Leggings (Paladin)
            [29064] = Specializations.HolyPaladin, // Justicar Pauldrons (Paladin)

            // Tier 5
            [30129] = Specializations.RetPaladin, // Crystalforge Breastplate (Paladin)
            [30130] = Specializations.RetPaladin, // Crystalforge Gauntlets (Paladin)
            [30132] = Specializations.RetPaladin, // Crystalforge Greaves (Paladin)
            [30133] = Specializations.RetPaladin, // Crystalforge Shoulderbraces (Paladin)
            [30131] = Specializations.RetPaladin, // Crystalforge War-Helm (Paladin)
            [30123] = Specializations.ProtPaladin, // Crystalforge Chestguard (Paladin)
            [30125] = Specializations.ProtPaladin, // Crystalforge Faceguard (Paladin)
            [30124] = Specializations.ProtPaladin, // Crystalforge Handguards (Paladin)
            [30126] = Specializations.ProtPaladin, // Crystalforge Legguards (Paladin)
            [30127] = Specializations.ProtPaladin, // Crystalforge Shoulderguards (Paladin)
            [30134] = Specializations.HolyPaladin, // Crystalforge Chestpiece (Paladin)
            [30135] = Specializations.HolyPaladin, // Crystalforge Gloves (Paladin)
            [30136] = Specializations.HolyPaladin, // Crystalforge Greathelm (Paladin)
            [30137] = Specializations.HolyPaladin, // Crystalforge Leggings (Paladin)
            [30138] = Specializations.HolyPaladin, // Crystalforge Pauldrons (Paladin)

            // Tier 6
            [30990] = Specializations.RetPaladin, // Lightbringer Breastplate (Paladin)
            [30982] = Specializations.RetPaladin, // Lightbringer Gauntlets (Paladin)
            [30993] = Specializations.RetPaladin, // Lightbringer Greaves (Paladin)
            [30997] = Specializations.RetPaladin, // Lightbringer Shoulderbraces (Paladin)
            [30989] = Specializations.RetPaladin, // Lightbringer War-Helm (Paladin)
            [34431] = Specializations.RetPaladin, // Lightbringer Bands (Paladin)
            [34561] = Specializations.RetPaladin, // Lightbringer Boots (Paladin)
            [34485] = Specializations.RetPaladin, // Lightbringer Girdle (Paladin)
            [30991] = Specializations.ProtPaladin, // Lightbringer Chestguard (Paladin)
            [30987] = Specializations.ProtPaladin, // Lightbringer Faceguard (Paladin)
            [30985] = Specializations.ProtPaladin, // Lightbringer Handguards (Paladin)
            [30995] = Specializations.ProtPaladin, // Lightbringer Legguards (Paladin)
            [30998] = Specializations.ProtPaladin, // Lightbringer Shoulderguards (Paladin)
            [34560] = Specializations.ProtPaladin, // Lightbringer Stompers (Paladin)
            [34488] = Specializations.ProtPaladin, // Lightbringer Waistguard (Paladin)
            [34433] = Specializations.ProtPaladin, // Lightbringer Wristguards (Paladin)
            [30992] = Specializations.HolyPaladin, // Lightbringer Chestpiece (Paladin)
            [30983] = Specializations.HolyPaladin, // Lightbringer Gloves (Paladin)
            [30988] = Specializations.HolyPaladin, // Lightbringer Greathelm (Paladin)
            [30994] = Specializations.HolyPaladin, // Lightbringer Leggings (Paladin)
            [30996] = Specializations.HolyPaladin, // Lightbringer Pauldrons (Paladin)
            [34487] = Specializations.HolyPaladin, // Lightbringer Belt (Paladin)
            [34432] = Specializations.HolyPaladin, // Lightbringer Bracers (Paladin)
            [34559] = Specializations.HolyPaladin, // Lightbringer Treads (Paladin)

            // Priest
            // Tier 4
            [29054] = SpecializationGroups.HealerPriest, // Light-Mantle of the Incarnate (Priest)
            [29050] = SpecializationGroups.HealerPriest, // Robes of the Incarnate (Priest)
            [29053] = SpecializationGroups.HealerPriest, // Trousers of the Incarnate (Priest)
            [29059] = Specializations.ShadowPriest, // Leggings of the Incarnate (Priest)
            [29056] = Specializations.ShadowPriest, // Shroud of the Incarnate (Priest)
            [29060] = Specializations.ShadowPriest, // Soul-Mantle of the Incarnate (Priest)

            // Tier 5
            [30153] = SpecializationGroups.HealerPriest, // Breeches of the Avatar (Priest)
            [30152] = SpecializationGroups.HealerPriest, // Cowl of the Avatar (Priest)
            [30151] = SpecializationGroups.HealerPriest, // Gloves of the Avatar (Priest)
            [30154] = SpecializationGroups.HealerPriest, // Mantle of the Avatar (Priest)
            [30150] = SpecializationGroups.HealerPriest, // Vestments of the Avatar (Priest)
            [30160] = Specializations.ShadowPriest, // Handguards of the Avatar (Priest)
            [30161] = Specializations.ShadowPriest, // Hood of the Avatar (Priest)
            [30162] = Specializations.ShadowPriest, // Leggings of the Avatar (Priest)
            [30159] = Specializations.ShadowPriest, // Shroud of the Avatar (Priest)
            [30163] = Specializations.ShadowPriest, // Wings of the Avatar (Priest)

            // Tier 6
            [31068] = SpecializationGroups.HealerPriest, // Breeches of Absolution (Priest)
            [31063] = SpecializationGroups.HealerPriest, // Cowl of Absolution (Priest)
            [31060] = SpecializationGroups.HealerPriest, // Gloves of Absolution (Priest)
            [31069] = SpecializationGroups.HealerPriest, // Mantle of Absolution (Priest)
            [31066] = SpecializationGroups.HealerPriest, // Vestments of Absolution (Priest)
            [34527] = SpecializationGroups.HealerPriest, // Belt of Absolution (Priest)
            [34562] = SpecializationGroups.HealerPriest, // Boots of Absolution (Priest)
            [34435] = SpecializationGroups.HealerPriest, // Cuffs of Absolution (Priest)
            [31061] = Specializations.ShadowPriest, // Handguards of Absolution (Priest)
            [31064] = Specializations.ShadowPriest, // Hood of Absolution (Priest)
            [31067] = Specializations.ShadowPriest, // Leggings of Absolution (Priest)
            [31070] = Specializations.ShadowPriest, // Shoulderpads of Absolution (Priest)
            [31065] = Specializations.ShadowPriest, // Shroud of Absolution (Priest)
            [34434] = Specializations.ShadowPriest, // Bracers of Absolution (Priest)
            [34528] = Specializations.ShadowPriest, // Cord of Absolution (Priest)
            [34563] = Specializations.ShadowPriest, // Treads of Absolution (Priest)

            // Shaman
            // Tier 4
            [29038] = Specializations.EnhanceShaman, // Cyclone Breastplate (Shaman)
            [29043] = Specializations.EnhanceShaman, // Cyclone Shoulderplates (Shaman)
            [29042] = Specializations.EnhanceShaman, // Cyclone War-Kilt (Shaman)
            [29033] = Specializations.EleShaman, // Cyclone Chestguard (Shaman)
            [29036] = Specializations.EleShaman, // Cyclone Legguards (Shaman)
            [29037] = Specializations.EleShaman, // Cyclone Shoulderguards (Shaman)
            [29029] = Specializations.RestoShaman, // Cyclone Hauberk (Shaman)
            [29030] = Specializations.RestoShaman, // Cyclone Kilt (Shaman)
            [29031] = Specializations.RestoShaman, // Cyclone Shoulderpads (Shaman)

            // Tier 5
            [30185] = Specializations.EnhanceShaman, // Cataclysm Chestplate (Shaman)
            [30189] = Specializations.EnhanceShaman, // Cataclysm Gauntlets (Shaman)
            [30190] = Specializations.EnhanceShaman, // Cataclysm Helm (Shaman)
            [30192] = Specializations.EnhanceShaman, // Cataclysm Legplates (Shaman)
            [30194] = Specializations.EnhanceShaman, // Cataclysm Shoulderplates (Shaman)
            [30169] = Specializations.EleShaman, // Cataclysm Chestpiece (Shaman)
            [30170] = Specializations.EleShaman, // Cataclysm Handgrips (Shaman)
            [30171] = Specializations.EleShaman, // Cataclysm Headpiece (Shaman)
            [30172] = Specializations.EleShaman, // Cataclysm Leggings (Shaman)
            [30173] = Specializations.EleShaman, // Cataclysm Shoulderpads (Shaman)
            [30164] = Specializations.RestoShaman, // Cataclysm Chestguard (Shaman)
            [30165] = Specializations.RestoShaman, // Cataclysm Gloves (Shaman)
            [30166] = Specializations.RestoShaman, // Cataclysm Headguard (Shaman)
            [30167] = Specializations.RestoShaman, // Cataclysm Legguards (Shaman)
            [30168] = Specializations.RestoShaman, // Cataclysm Shoulderguards (Shaman)

            // Tier 6
            [31015] = Specializations.EnhanceShaman, // Skyshatter Cover (Shaman)
            [31011] = Specializations.EnhanceShaman, // Skyshatter Grips (Shaman)
            [31021] = Specializations.EnhanceShaman, // Skyshatter Pants (Shaman)
            [31024] = Specializations.EnhanceShaman, // Skyshatter Pauldrons (Shaman)
            [31018] = Specializations.EnhanceShaman, // Skyshatter Tunic (Shaman)
            [34545] = Specializations.EnhanceShaman, // Skyshatter Girdle (Shaman)
            [34567] = Specializations.EnhanceShaman, // Skyshatter Greaves (Shaman)
            [34439] = Specializations.EnhanceShaman, // Skyshatter Wristguards (Shaman)
            [31017] = Specializations.EleShaman, // Skyshatter Breastplate (Shaman)
            [31008] = Specializations.EleShaman, // Skyshatter Gauntlets (Shaman)
            [31014] = Specializations.EleShaman, // Skyshatter Headguard (Shaman)
            [31020] = Specializations.EleShaman, // Skyshatter Legguards (Shaman)
            [31023] = Specializations.EleShaman, // Skyshatter Mantle (Shaman)
            [34437] = Specializations.EleShaman, // Skyshatter Bands (Shaman)
            [34542] = Specializations.EleShaman, // Skyshatter Cord (Shaman)
            [34566] = Specializations.EleShaman, // Skyshatter Treads (Shaman)
            [31016] = Specializations.RestoShaman, // Skyshatter Chestguard (Shaman)
            [31007] = Specializations.RestoShaman, // Skyshatter Gloves (Shaman)
            [31012] = Specializations.RestoShaman, // Skyshatter Helmet (Shaman)
            [31019] = Specializations.RestoShaman, // Skyshatter Leggings (Shaman)
            [31022] = Specializations.RestoShaman, // Skyshatter Shoulderpads (Shaman)
            [34543] = Specializations.RestoShaman, // Skyshatter Belt (Shaman)
            [34565] = Specializations.RestoShaman, // Skyshatter Boots (Shaman)
            [34438] = Specializations.RestoShaman, // Skyshatter Bracers (Shaman)

            // Druid
            // Tier 4
            [29096] = Specializations.BearDruid | Specializations.CatDruid, // Breastplate of Malorne (Druid)
            [29099] = Specializations.BearDruid | Specializations.CatDruid, // Greaves of Malorne (Druid)
            [29100] = Specializations.BearDruid | Specializations.CatDruid, // Mantle of Malorne (Druid)
            [29094] = Specializations.BalanceDruid, // Britches of Malorne (Druid)
            [29091] = Specializations.BalanceDruid, // Chestpiece of Malorne (Druid)
            [29095] = Specializations.BalanceDruid, // Pauldrons of Malorne (Druid)
            [29087] = Specializations.RestoDruid, // Chestguard of Malorne (Druid)
            [29088] = Specializations.RestoDruid, // Legguards of Malorne (Druid)
            [29089] = Specializations.RestoDruid, // Shoulderguards of Malorne (Druid)

            // Tier 5
            [30222] = Specializations.BearDruid | Specializations.CatDruid, // Nordrassil Chestplate (Druid)
            [30229] = Specializations.BearDruid | Specializations.CatDruid, // Nordrassil Feral-Kilt (Druid)
            [30230] = Specializations.BearDruid | Specializations.CatDruid, // Nordrassil Feral-Mantle (Druid)
            [30223] = Specializations.BearDruid | Specializations.CatDruid, // Nordrassil Handgrips (Druid)
            [30228] = Specializations.BearDruid | Specializations.CatDruid, // Nordrassil Headdress (Druid)
            [30231] = Specializations.BalanceDruid, // Nordrassil Chestpiece (Druid)
            [30232] = Specializations.BalanceDruid, // Nordrassil Gauntlets (Druid)
            [30233] = Specializations.BalanceDruid, // Nordrassil Headpiece (Druid)
            [30234] = Specializations.BalanceDruid, // Nordrassil Wrath-Kilt (Druid)
            [30235] = Specializations.BalanceDruid, // Nordrassil Wrath-Mantle (Druid)
            [30216] = Specializations.RestoDruid, // Nordrassil Chestguard (Druid)
            [30217] = Specializations.RestoDruid, // Nordrassil Gloves (Druid)
            [30219] = Specializations.RestoDruid, // Nordrassil Headguard (Druid)
            [30220] = Specializations.RestoDruid, // Nordrassil Life-Kilt (Druid)
            [30221] = Specializations.RestoDruid, // Nordrassil Life-Mantle (Druid)

            // Tier 6
            [31042] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Chestguard (Druid)
            [31039] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Cover (Druid)
            [31034] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Gauntlets (Druid)
            [31044] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Leggings (Druid)
            [31048] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Pauldrons (Druid)
            [34573] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Treads (Druid)
            [34556] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Waistguard (Druid)
            [34444] = Specializations.BearDruid | Specializations.CatDruid, // Thunderheart Wristguards (Druid)
            [31035] = Specializations.BalanceDruid, // Thunderheart Handguards (Druid)
            [31040] = Specializations.BalanceDruid, // Thunderheart Headguard (Druid)
            [31046] = Specializations.BalanceDruid, // Thunderheart Pants (Druid)
            [31049] = Specializations.BalanceDruid, // Thunderheart Shoulderpads (Druid)
            [31043] = Specializations.BalanceDruid, // Thunderheart Vest (Druid)
            [34446] = Specializations.BalanceDruid, // Thunderheart Bands (Druid)
            [34555] = Specializations.BalanceDruid, // Thunderheart Cord (Druid)
            [34572] = Specializations.BalanceDruid, // Thunderheart Footwraps (Druid)
            [31032] = Specializations.RestoDruid, // Thunderheart Gloves (Druid)
            [31037] = Specializations.RestoDruid, // Thunderheart Helmet (Druid)
            [31045] = Specializations.RestoDruid, // Thunderheart Legguards (Druid)
            [31047] = Specializations.RestoDruid, // Thunderheart Spaulders (Druid)
            [31041] = Specializations.RestoDruid, // Thunderheart Tunic (Druid)
            [34554] = Specializations.RestoDruid, // Thunderheart Belt (Druid)
            [34571] = Specializations.RestoDruid, // Thunderheart Boots (Druid)
            [34445] = Specializations.RestoDruid, // Thunderheart Bracers (Druid)
        };

        protected override string DisallowReason => "Tier gear is not for the chosen specialization.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => _tierSpecs.ContainsKey(item.Id);

        protected override bool IsAllowed(Item item, Specializations spec) => (_tierSpecs[item.Id] & spec) != 0;
    }
}
