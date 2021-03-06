﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class EyeOfMagtheridonRule : SimpleRule
    {
        protected override string DisallowReason => "Eye of Magtheridon has an effect that procs when spells are resisted, which is not useful to healers.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 28789u;

        protected override Specializations ApplicableSpecs() => SpecializationGroups.Healer;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
