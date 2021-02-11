// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.FactualLimitations
{
    internal class CanEquipClassRestrictedRule : SimpleRule
    {
        protected override string DisallowReason => "Class does not match the item's class restrictions.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

        protected override bool AppliesTo(Item item) => item.UsableClasses.HasValue;

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            Debug.Assert(item.UsableClasses.HasValue);
            Specializations restrictedSpecs = Specializations.None;

            foreach (Classes c in Enum.GetValues(typeof(Classes)))
            {
                if (c != Classes.None && (item.UsableClasses.Value & c) == c)
                {
                    restrictedSpecs |= GetClassSpecializations(c);
                }
            }

            return (spec & restrictedSpecs) == spec;
        }

        private static Specializations GetClassSpecializations(Classes playerClass)
        {
            return playerClass switch
            {
                Classes.Warrior => Specializations.Warrior,
                Classes.Paladin => Specializations.Paladin,
                Classes.Hunter => Specializations.Hunter,
                Classes.Rogue => Specializations.Rogue,
                Classes.Priest => Specializations.Priest,
                Classes.Shaman => Specializations.Shaman,
                Classes.Mage => Specializations.Mage,
                Classes.Warlock => Specializations.Warlock,
                Classes.Druid => Specializations.Druid,
                _ => throw new ArgumentOutOfRangeException(nameof(playerClass), "Parameter must be a single defined playable class."),
            };
        }
    }
}
