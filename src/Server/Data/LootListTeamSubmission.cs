// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data;

public class LootListTeamSubmission
{
    public long LootListCharacterId { get; set; }

    public byte LootListPhase { get; set; }

    public long TeamId { get; set; }

    public CharacterLootList LootList { get; set; } = null!;

    public RaidTeam Team { get; set; } = null!;
}
