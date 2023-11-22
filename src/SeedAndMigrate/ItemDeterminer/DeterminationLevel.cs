// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer;

internal enum DeterminationLevel
{
    Allowed = 0,
    ManualReview = 1,
    Disallowed = 2,
    Unequippable = 3
}
