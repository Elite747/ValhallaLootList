// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class EncounterKillDto
{
    private List<EncounterDropDto>? _drops;
    private List<long>? _characters;

    public string EncounterId { get; set; } = string.Empty;

    public string EncounterName { get; set; } = string.Empty;

    public byte TrashIndex { get; set; }

    public DateTimeOffset KilledAt { get; set; }

    public List<EncounterDropDto> Drops
    {
        get => _drops ??= [];
        set => _drops = value;
    }

    public List<long> Characters
    {
        get => _characters ??= [];
        set => _characters = value;
    }
}
