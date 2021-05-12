// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class ApproveOrRejectLootListDto : TimestampDto
    {
        public bool Approved { get; set; }

        public long TeamId { get; set; }

        public string? Message { get; set; }
    }
}
