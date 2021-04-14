// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.Server.Data
{
    public class TeamRemoval
    {
        public TeamRemoval(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
            }

            Id = id;
        }

        public long Id { get; }

        public DateTimeOffset RemovedAt { get; set; }

        public long TeamId { get; set; }

        public RaidTeam Team { get; set; } = null!;

        public long CharacterId { get; set; }

        public Character Character { get; set; } = null!;
    }
}
