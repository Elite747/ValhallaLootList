// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class DonationSubmissionDto
{
    public long CharacterId { get; set; }

    [Range(1, 12)]
    public int TargetMonth { get; set; }

    [Range(1, 9999)]
    public int TargetYear { get; set; }

    [Range(1, 9999)]
    public int Amount { get; set; }

    public string? Unit { get; set; }
}

public class DonationImportRecord
{
    public string CharacterName { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    [Range(1, 9999)]
    public int Amount { get; set; }
}

public class DonationImportDto
{
    public List<DonationImportRecord> Records { get; set; } = [];

    [Range(1, 12)]
    public int TargetMonth { get; set; }

    [Range(1, 9999)]
    public int TargetYear { get; set; }
}
