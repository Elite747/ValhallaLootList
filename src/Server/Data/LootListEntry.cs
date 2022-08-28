// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data;

public class LootListEntry
{
    public LootListEntry(long id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
        }

        Id = id;
    }

    public long Id { get; }

    [Required]
    public virtual CharacterLootList LootList { get; set; } = null!;

    public byte Rank { get; set; }

    public uint? ItemId { get; set; }

    public virtual Item? Item { get; set; }

    public long? DropId { get; set; }

    public Drop? Drop { get; set; }

    [StringLength(256)]
    public string? Justification { get; set; }

    public bool AutoPass { get; set; }

    public bool Heroic { get; set; }

    public virtual ICollection<DropPass> Passes { get; set; } = new HashSet<DropPass>();
}
