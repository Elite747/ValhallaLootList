// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class CharacterAdminDto
{
    private List<TeamRemovalDto>? _teamRemovals;
    private List<string>? _otherCharacters;

    public List<TeamRemovalDto> TeamRemovals
    {
        get => _teamRemovals ??= [];
        set => _teamRemovals = value;
    }

    public List<string> OtherCharacters
    {
        get => _otherCharacters ??= [];
        set => _otherCharacters = value;
    }

    public GuildMemberDto? Owner { get; set; }

    public GuildMemberDto? VerifiedBy { get; set; }
}
