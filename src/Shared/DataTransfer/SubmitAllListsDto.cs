// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class SubmitAllListsDto : MultiTimestampDto
    {
        private List<long>? _submitTo;

        public List<long> SubmitTo
        {
            get => _submitTo ??= new();
            set => _submitTo = value;
        }
    }
}
