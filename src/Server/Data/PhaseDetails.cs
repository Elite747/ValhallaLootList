﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.Server.Data
{
    public class PhaseDetails
    {
        public PhaseDetails(byte id, DateTimeOffset startsAt)
        {
            Id = id;
            StartsAt = startsAt;
        }

        public byte Id { get; }

        public DateTimeOffset StartsAt { get; set; }
    }
}
