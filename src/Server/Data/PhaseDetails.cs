// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.Server.Data
{
    public class PhaseDetails
    {
        public PhaseDetails()
        {
        }

        public PhaseDetails(byte id, DateTime startsAtUtc)
        {
            Id = id;
            StartsAtUtc = startsAtUtc;
        }

        public byte Id { get; set; }

        public DateTime StartsAtUtc { get; set; }
    }
}
