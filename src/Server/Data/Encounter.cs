// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data;

public class Encounter
{
    public Encounter(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
        }

        if (id.Length > 20)
        {
            throw new ArgumentException($"'{nameof(id)}' cannot be greater than 20 characters long.", nameof(id));
        }

        Id = id;
    }

    [StringLength(20)]
    public string Id { get; }

    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string InstanceId { get; set; } = null!;

    public sbyte Index { get; set; }

    [Required]
    public virtual Instance Instance { get; set; } = null!;

    public virtual ICollection<EncounterItem> Items { get; set; } = new HashSet<EncounterItem>();
}
