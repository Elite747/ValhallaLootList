// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Client.Pages.Raids
{
    public class NumberInput
    {
        public NumberInput(int value) => Value = value;

        [Range(0, 5)]
        public int Value { get; set; }
    }
}
