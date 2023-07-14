// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class KillSubmissionDto
{
    private List<long>? _characters;
    private List<KillDropSubmissionDto>? _drops;

    [Required]
    public string? EncounterId { get; set; }

    public DateTimeOffset? Date { get; set; }

    public List<long> Characters
    {
        get => _characters ??= new();
        set => _characters = value;
    }

    public List<KillDropSubmissionDto> Drops
    {
        get => _drops ??= new();
        set => _drops = value;
    }
}

public class KillDropSubmissionDto
{
    public uint ItemId { get; set; }

    public long? WinnerId { get; set; }

    public bool Disenchanted { get; set; }
}
