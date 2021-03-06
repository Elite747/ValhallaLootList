﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class LootListSubmissionDto : IValidatableObject
    {
        public Specializations MainSpec { get; set; }

        public Specializations OffSpec { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MainSpec == Specializations.None)
            {
                yield return new ValidationResult("The Main Spec field is required.", new[] { nameof(MainSpec) });
            }
            else if (MainSpec == OffSpec)
            {
                yield return new ValidationResult("Off Spec cannot be the same as Main Spec.", new[] { nameof(OffSpec) });
            }
        }
    }
}
