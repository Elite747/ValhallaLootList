// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data;

public class CharacterEncounterKill
{
    [Required]
    public long CharacterId { get; set; }

    [Required]
    public long EncounterKillRaidId { get; set; }

    [Required]
    public string EncounterKillEncounterId { get; set; } = null!;

    public byte EncounterKillTrashIndex { get; set; }

    [Required]
    public Character Character { get; set; } = null!;

    [Required]
    public virtual EncounterKill EncounterKill { get; set; } = null!;
}
