// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json;
using CommandLine;
using DartSassHost;
using DartSassHost.Helpers;
using JavaScriptEngineSwitcher.V8;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using NUglify;
using NUglify.JavaScript;
using ValhallaLootList.Minify;

return Parser.Default.ParseArguments<ScssOptions, JsOptions>(args).MapResult<ScssOptions, JsOptions, int>(ProcessScss, ProcessJs, _ => -1);

static int ProcessScss(ScssOptions options)
{
    using var sassCompiler = new SassCompiler(new V8JsEngineFactory());

    foreach (var target in Unglob(options, ".min.css", ".min.css.map"))
    {
        try
        {
            var compilation = sassCompiler.CompileFile(target.Input, outputPath: target.Output, sourceMapPath: target.Map, options: new CompilationOptions
            {
                SourceMap = true,
                OutputStyle = OutputStyle.Compressed
            });

            File.WriteAllText(target.Output, compilation.CompiledContent);

            var sourceMap = JsonSerializer.Deserialize<SourceMap>(compilation.SourceMap);

            if (sourceMap is not null)
            {
                if (sourceMap.Sources is not null)
                {
                    sourceMap.Sources = Array.ConvertAll(sourceMap.Sources, uri => Path.GetFileName(uri));
                }

                sourceMap.File = Path.GetFileName(sourceMap.File);

                File.WriteAllText(target.Map, JsonSerializer.Serialize(sourceMap));
            }
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
    }

    return 0;
}

static int ProcessJs(JsOptions options)
{
    foreach (var target in Unglob(options, ".min.js", ".min.js.map"))
    {
        string inputJs = File.ReadAllText(target.Input);
        using var mapStream = File.CreateText(target.Map);
        using var sourceMap = new V3SourceMap(mapStream);
        sourceMap.StartPackage(target.Output, target.Map);
        var uglifyResult = Uglify.Js(inputJs, target.Input, new() { SymbolsMap = sourceMap });

        if (uglifyResult.HasErrors)
        {
            foreach (var error in uglifyResult.Errors)
            {
                Console.WriteLine(error);
            }
            return -1;
        }
        File.WriteAllText(target.Output, uglifyResult.Code);
    }

    return 0;
}

static TargetPaths[] Unglob(OptionsBase options, string defaultSuffix, string defaultMapSuffix)
{
    string suffix = options.Suffix ?? defaultSuffix;
    string mapSuffix = options.MapSuffix ?? defaultMapSuffix;
    Matcher matcher = new();
    matcher.AddIncludePatterns(options.Include);
    matcher.AddExcludePatterns(options.Exclude);
    var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(Directory.GetCurrentDirectory())));
    return result.HasMatches ? result.Files.Select(f => new TargetPaths(f.Path, suffix, mapSuffix)).ToArray() : Array.Empty<TargetPaths>();
}