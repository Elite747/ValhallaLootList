// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class PhaseConfigDto
    {
        private Dictionary<byte, List<BracketDto>>? _brackets;

        public byte CurrentPhase { get; set; }

        public Dictionary<byte, List<BracketDto>> Brackets
        {
            get => _brackets ??= new Dictionary<byte, List<BracketDto>>();
            set => _brackets = value;
        }

        public IReadOnlyList<BracketDto> GetCurrentBrackets()
        {
            return GetBrackets(CurrentPhase) ?? throw new System.Exception("Configuration for the current phase was not found.");
        }

        public IReadOnlyList<BracketDto>? GetBrackets(int phase)
        {
            if (phase < byte.MinValue || phase > byte.MaxValue)
            {
                return null;
            }

            if (Brackets.TryGetValue((byte)phase, out var brackets))
            {
                return brackets;
            }

            return null;
        }
    }
}
