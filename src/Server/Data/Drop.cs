// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data;

public class Drop
{
    public Drop(long id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
        }

        Id = id;
    }

    public long Id { get; }

    public long? AwardedBy { get; set; }

    public DateTimeOffset AwardedAt { get; set; }

    public bool Disenchanted { get; set; }

    [Required]
    public uint ItemId { get; set; }

    public long? WinnerId { get; set; }

    [Required]
    public long EncounterKillRaidId { get; set; }

    [Required]
    public string EncounterKillEncounterId { get; set; } = null!;

    public byte EncounterKillTrashIndex { get; set; }

    [Required]
    public virtual EncounterKill EncounterKill { get; set; } = null!;

    [Required]
    public virtual Item Item { get; set; } = null!;

    public virtual Character? Winner { get; set; }

    public virtual LootListEntry? WinningEntry { get; set; }

    [Obsolete("Passes counted from drops instead.")]
    public virtual ICollection<DropPass> Passes { get; set; } = new HashSet<DropPass>();
}
