// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Helpers;

public class CharacterNameAttribute : RegularExpressionAttribute
{
    public CharacterNameAttribute() : base(NameHelpers.CharacterNameRegex)
    {
        ErrorMessage = "Name must be 2 to 12 characters long, contain only letters, and have no more than two consecutive letters.";
    }
}
