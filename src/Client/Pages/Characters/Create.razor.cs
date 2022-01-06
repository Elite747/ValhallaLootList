// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using MudBlazor;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters;

public partial class Create
{
    private static readonly Dictionary<PlayerRace, Classes[]> _classLookup = new()
    {
        [PlayerRace.Human] = new[] { Classes.Mage, Classes.Paladin, Classes.Priest, Classes.Rogue, Classes.Warlock, Classes.Warrior },
        [PlayerRace.Dwarf] = new[] { Classes.Hunter, Classes.Paladin, Classes.Priest, Classes.Rogue, Classes.Warrior },
        [PlayerRace.NightElf] = new[] { Classes.Druid, Classes.Hunter, Classes.Priest, Classes.Rogue, Classes.Warrior },
        [PlayerRace.Gnome] = new[] { Classes.Mage, Classes.Rogue, Classes.Warlock, Classes.Warrior },
        [PlayerRace.Draenei] = new[] { Classes.Hunter, Classes.Mage, Classes.Paladin, Classes.Priest, Classes.Shaman, Classes.Warrior }
    };

    private static readonly Dictionary<Classes, PlayerRace[]> _raceLookup = new()
    {
        [Classes.Druid] = new[] { PlayerRace.NightElf },
        [Classes.Hunter] = new[] { PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Draenei },
        [Classes.Mage] = new[] { PlayerRace.Human, PlayerRace.Gnome, PlayerRace.Draenei },
        [Classes.Paladin] = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.Draenei },
        [Classes.Priest] = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Draenei },
        [Classes.Rogue] = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Gnome },
        [Classes.Shaman] = new[] { PlayerRace.Draenei },
        [Classes.Warlock] = new[] { PlayerRace.Human, PlayerRace.Gnome },
        [Classes.Warrior] = _allRaces = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Gnome, PlayerRace.Draenei }
    };

    private static readonly Classes[] _allClasses = new[] {
            Classes.Druid,
            Classes.Hunter,
            Classes.Mage,
            Classes.Paladin,
            Classes.Priest,
            Classes.Rogue,
            Classes.Shaman,
            Classes.Warlock,
            Classes.Warrior
        };

    private static readonly PlayerRace[] _allRaces;

    private Task OnSubmit()
    {
        return (EditingCharacter is null ? Api.Characters.Create(_character) : Api.Characters.Update(EditingCharacter.Id, _character))
            .OnSuccess((CharacterDto character, CancellationToken ct) =>
            {
                Dialog.Close(DialogResult.Ok(character));
                if (_character.SenderIsOwner)
                {
                    return Permissions.RefreshAsync(ct);
                }
                return Task.CompletedTask;
            })
            .ValidateWith(_problemValidator)
            .ExecuteAsync();
    }

    private void RaceChanged(PlayerRace? newValue)
    {
        _character.Race = newValue;
        _raceClasses = (newValue.HasValue && _classLookup.TryGetValue(newValue.Value, out var raceClasses)) ? raceClasses : _allClasses;

        if (Array.IndexOf(_raceClasses, _character.Class) < 0)
        {
            _character.Class = default;
        }
    }
}
