// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList;

public class CharacterOwnerRequirement : IAuthorizationRequirement
{
    public CharacterOwnerRequirement(bool allowAdmin)
    {
        AllowAdmin = allowAdmin;
    }

    public bool AllowAdmin { get; }
}
