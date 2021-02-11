// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList
{
    [Flags]
	public enum Classes
	{
        None = 0,
		Warrior = 0b_0000_0000_0001,
		Paladin = 0b_0000_0000_0010,
		Hunter = 0b_0000_0000_0100,
		Rogue = 0b_0000_0000_1000,
		Priest = 0b_0000_0001_0000,
		Shaman = 0b_0000_0100_0000,
		Mage = 0b_0000_1000_0000,
		Warlock = 0b_0001_0000_0000,
		Druid = 0b_0100_0000_0000
	}
}
