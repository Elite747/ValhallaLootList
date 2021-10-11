// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.IO;

namespace ValhallaLootList.Minify
{
    internal readonly struct TargetPaths
    {
        public TargetPaths(string input, string suffix, string mapSuffix)
        {
            string noExt = Path.ChangeExtension(input, null);
            Input = input;
            Output = noExt + suffix;
            Map = noExt + mapSuffix;
        }

        public string Input { get; }

        public string Output { get; }

        public string Map { get; }
    }
}