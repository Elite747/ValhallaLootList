// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using CommandLine;

namespace ValhallaLootList.Minify
{
    [Verb("scss", HelpText = "Compiles and minifies Scss stylesheets.")]
    internal class ScssOptions
    {
        [Value(0, Required = true, HelpText = "The file to process.")]
        public string Input { get; set; } = string.Empty;

        [Option('o', "output", HelpText = "The file to create. If not specified, the input is used with the .min.js extension.")]
        public string? Output { get; set; }

        [Option('m', "map", HelpText = "The map file to create. If not specified, the input is used with the .min.js.map extension.")]
        public string? Map { get; set; }
    }
}