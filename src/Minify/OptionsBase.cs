// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using CommandLine;

namespace ValhallaLootList.Minify;

internal class OptionsBase
{
    [Value(0, Required = true, HelpText = "The file or files to process.")]
    public IEnumerable<string> Include { get; set; } = Array.Empty<string>();

    [Option('x', "exclude", HelpText = "The file or files to exclude from processing.")]
    public IEnumerable<string> Exclude { get; set; } = Array.Empty<string>();

    [Option('s', "suffix", HelpText = "The filename suffix and/or extension to apply to the compiled files.")]
    public string? Suffix { get; set; }

    [Option('m', "map", HelpText = "The filename suffix and/or extension to apply to maps to source files.")]
    public string? MapSuffix { get; set; }
}
