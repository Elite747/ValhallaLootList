﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Helpers;

public class GuildNameAttribute : RegularExpressionAttribute
{
    public GuildNameAttribute() : base(NameHelpers.GuildNameRegex)
    {
        ErrorMessage = "Name must be 2 to 24 characters long, contain no numbers, and have no more than two consecutive letters.";
    }
}
