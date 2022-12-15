// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class PhaseConfigDto
{
    private List<byte>? _phases;

    public byte CurrentPhase { get; set; }

    public List<byte> Phases
    {
        get => _phases ??= new();
        set => _phases = value;
    }
}

public class PhaseDto
{
    private List<BracketDto>? _brackets;

    public byte Phase { get; set; }

    public DateTimeOffset StartsAt { get; set; }

    public List<BracketDto> Brackets
    {
        get => _brackets ??= new();
        set => _brackets = value;
    }
}

public class BracketDto
{
    public bool AllowOffspec { get; set; }

    public bool AllowTypeDuplicates { get; set; }

    public int NormalItems { get; set; }

    public int HeroicItems { get; set; }

    public int MinRank { get; set; }

    public int MaxRank { get; set; }
}
