// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class DonationDto
{
    public long Id { get; set; }
    public DateTimeOffset DonatedAt { get; set; }
    public int Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public long EnteredById { get; set; }
    public long CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
}
