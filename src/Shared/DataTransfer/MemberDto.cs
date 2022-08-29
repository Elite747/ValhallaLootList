﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class MemberDto
{
    private CharacterDto? _character;
    private List<MemberLootListDto>? _lootLists;

    public CharacterDto Character
    {
        get => _character ?? throw new InvalidOperationException("Characer has not been set.");
        set => _character = value;
    }

    public List<MemberLootListDto> LootLists
    {
        get => _lootLists ??= new();
        set => _lootLists = value;
    }

    public RaidMemberStatus Status { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public bool Enchanted { get; set; }

    public bool Prepared { get; set; }

    public int DonatedThisMonth { get; set; }

    public int DonatedNextMonth { get; set; }

    public int MaximumDonationTickets { get; set; }

    public int Absences { get; set; }

    public bool Disenchanter { get; set; }
}
