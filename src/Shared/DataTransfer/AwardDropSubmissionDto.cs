// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class AwardDropSubmissionDto
{
    public long? WinnerId { get; set; }

    public bool Disenchant { get; set; }
}
