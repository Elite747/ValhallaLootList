// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data;

[Obsolete("Passes counted from drops instead.")]
public class DropPass
{
    [Required]
    public long CharacterId { get; set; }

    [Required]
    public virtual Character Character { get; set; } = null!;

    [Required]
    public long DropId { get; set; }

    [Required]
    public virtual Drop Drop { get; set; } = null!;

    public long? LootListEntryId { get; set; }

    public virtual LootListEntry? LootListEntry { get; set; }

    public int RelativePriority { get; set; }

    public long? WonEntryId { get; set; }

    public long? RemovalId { get; set; }

    public virtual TeamRemoval? Removal { get; set; }
}
