// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class MemberLootListDto
{
    public byte Phase { get; set; }

    public LootListStatus Status { get; set; }

    public Specializations MainSpec { get; set; }

    public bool? Approved { get; set; }

    public string? ApprovedBy { get; set; }
}
