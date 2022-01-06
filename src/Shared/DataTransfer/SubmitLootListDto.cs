// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class SubmitLootListDto : TimestampDto
{
    private List<long>? _submitTo;

    public List<long> SubmitTo
    {
        get => _submitTo ??= new();
        set => _submitTo = value;
    }
}
