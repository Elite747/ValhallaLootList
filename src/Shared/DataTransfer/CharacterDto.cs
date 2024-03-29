﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class CharacterDto
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public PlayerRace Race { get; set; }

    public Classes Class { get; set; }

    public bool Deactivated { get; set; }

    public List<long> Teams { get; set; } = [];

    public bool Verified { get; set; }
}
