// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.IO;
using System.Text.Json;
using CommandLine;
using DartSassHost;
using DartSassHost.Helpers;
using JavaScriptEngineSwitcher.V8;
using NUglify;
using NUglify.JavaScript;
using ValhallaLootList.Minify;

return Parser.Default.ParseArguments<ScssOptions, JsOptions>(args).MapResult<ScssOptions, JsOptions, int>(ProcessScss, ProcessJs, _ => -1);

static int ProcessScss(ScssOptions options)
{
    var input = new FileInfo(options.Input);

    if (!input.Exists)
    {
        Console.WriteLine($"Input file '{options.Input}' does not exist.");
        return -1;
    }

    try
    {
        using var sassCompiler = new SassCompiler(new V8JsEngineFactory());
        CreatePaths(options.Input, options.Output, options.Map, ".css", out string output, out string map);

        var compilation = sassCompiler.CompileFile(input.FullName, outputPath: output, sourceMapPath: map, options: new CompilationOptions
        {
            SourceMap = true,
            OutputStyle = OutputStyle.Compressed
        });

        File.WriteAllText(output, compilation.CompiledContent);
        WriteSourceMap(compilation.SourceMap, map);
    }
    catch (SassCompilerLoadException e)
    {
        Console.WriteLine("During loading of Sass compiler an error occurred. See details:");
        Console.WriteLine();
        Console.WriteLine(SassErrorHelpers.GenerateErrorDetails(e));
        return -1;
    }
    catch (SassCompilationException e)
    {
        Console.WriteLine("During compilation of SCSS code an error occurred. See details:");
        Console.WriteLine();
        Console.WriteLine(SassErrorHelpers.GenerateErrorDetails(e));
        return -1;
    }
    catch (SassException e)
    {
        Console.WriteLine("During working of Sass compiler an unknown error occurred. See details:");
        Console.WriteLine();
        Console.WriteLine(SassErrorHelpers.GenerateErrorDetails(e));
        return -1;
    }

    return 0;
}

static int ProcessJs(JsOptions options)
{
    string inputJs = File.ReadAllText(options.Input);
    CreatePaths(options.Input, options.Output, options.Map, ".js", out string output, out string map);
    using var mapStream = File.CreateText(map);
    using var sourceMap = new V3SourceMap(mapStream);
    sourceMap.StartPackage(output, map);
    var uglifyResult = Uglify.Js(inputJs, options.Input, new() { SymbolsMap = sourceMap });

    if (uglifyResult.HasErrors)
    {
        foreach (var error in uglifyResult.Errors)
        {
            Console.WriteLine(error);
        }
        return -1;
    }
    File.WriteAllText(output, uglifyResult.Code);
    return 0;
}

static void CreatePaths(string input, string? optionalOutput, string? optionalMap, string ext, out string output, out string map)
{
    output = optionalOutput ?? Path.ChangeExtension(input, ".min" + ext);
    map = optionalMap ?? (output + ".map");
}

static void WriteSourceMap(string sourceMapContent, string path)
{
    var sourceMap = JsonSerializer.Deserialize<SourceMap>(sourceMapContent);

    if (sourceMap is not null)
    {
        if (sourceMap.Sources is not null)
        {
            sourceMap.Sources = Array.ConvertAll(sourceMap.Sources, uri => Path.GetFileName(uri));
        }

        sourceMap.File = Path.GetFileName(sourceMap.File);

        File.WriteAllText(path, JsonSerializer.Serialize(sourceMap));
    }
}