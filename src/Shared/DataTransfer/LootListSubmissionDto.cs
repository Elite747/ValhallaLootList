// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class LootListSubmissionDto : IValidatableObject
{
    public long CharacterId { get; set; }

    public byte Phase { get; set; }

    public byte Size { get; set; }

    public Specializations MainSpec { get; set; }

    public Specializations OffSpec { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MainSpec == Specializations.None)
        {
            yield return new ValidationResult("The Main-spec field is required.", new[] { nameof(MainSpec) });
        }
        else if (MainSpec == OffSpec)
        {
            yield return new ValidationResult("Off-spec cannot be the same as Main-spec.", new[] { nameof(OffSpec) });
        }

        if (CharacterId == 0)
        {
            yield return new ValidationResult("The Character field is required.", new[] { nameof(CharacterId) });
        }

        if (Phase == 0)
        {
            yield return new ValidationResult("The Phase field is required.", new[] { nameof(Phase) });
        }

        if (Size == 0)
        {
            yield return new ValidationResult("The Size field is required.", new[] { nameof(Size) });
        }
        else if (Size is not (10 or 25))
        {
            yield return new ValidationResult("The Size field is not valid.", new[] { nameof(Size) });
        }
    }
}
