﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Donation
    {
        public Donation(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
            }

            Id = id;
        }

        public long Id { get; }

        public DateTimeOffset DonatedAt { get; set; }

        public long EnteredById { get; set; }

        public int CopperAmount { get; set; }

        public byte Month { get; set; }

        public short Year { get; set; }

        [Required]
        public long CharacterId { get; set; }

        [Required]
        public virtual Character Character { get; set; } = null!;
    }
}