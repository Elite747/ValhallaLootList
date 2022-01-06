// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using IdGen;

namespace ValhallaLootList.Server.IdGeneration;

public class IdGeneratorConfiguration
{
    public int GeneratorId { get; set; }

    public IdStructure? Structure { get; set; }

    public ITimeSource? TimeSource { get; set; }

    public SequenceOverflowStrategy SequenceOverflowStrategy { get; set; }
}
