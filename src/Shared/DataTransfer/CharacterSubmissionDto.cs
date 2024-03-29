﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.DataTransfer;

public class CharacterSubmissionDto : IValidatableObject
{
    [Required, CharacterName]
    public string? Name { get; set; }

    [Required]
    public PlayerRace? Race { get; set; }

    public Classes Class { get; set; }

    public bool SenderIsOwner { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Class == Classes.None)
        {
            yield return new ValidationResult("The Class field is required.", new[] { nameof(Class) });
        }
    }
}
