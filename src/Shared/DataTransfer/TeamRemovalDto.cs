// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class TeamRemovalDto
    {
        public long Id { get; set; }

        public DateTimeOffset JoinedAt { get; set; }

        public DateTimeOffset RemovedAt { get; set; }

        public long TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;
    }
}
