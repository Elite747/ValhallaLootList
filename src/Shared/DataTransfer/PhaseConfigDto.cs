// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class PhaseConfigDto
    {
        private List<byte>? _phases;

        public byte CurrentPhase { get; set; }

        public List<byte> Phases
        {
            get => _phases ??= new();
            set => _phases = value;
        }
    }
}
