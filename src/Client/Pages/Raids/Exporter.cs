// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Buffers;
using System.IO.Compression;
using System.Text.Json;
using MudBlazor;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Pages.Raids;

public class Exporter(ApiClient client, ISnackbar snackbar)
{
    private static readonly char[] _byteTo6BitChar = [
#pragma warning disable format
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h',
        'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
        'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
        'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F',
        'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
        'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
        'W', 'X', 'Y', 'Z', '0', '1', '2', '3',
        '4', '5', '6', '7', '8', '9', '(', ')',
#pragma warning restore format
    ];

    public async Task<string> GetExportAsync(long teamId, CancellationToken cancellationToken = default)
    {
        var operation = client.LootLists.GetForTeam(teamId, includeApplicants: false);
        operation.SendErrorTo(snackbar);
        var lists = await operation.ExecuteAndTryReturnAsync(cancellationToken: cancellationToken);

        if (lists?.Count > 0)
        {
            var items = new Dictionary<uint, Dictionary<int, HashSet<string>>>();

            foreach (var list in lists.Where(l => l.RanksVisible))
            {
                foreach (var entry in list.Entries.Where(l => !l.AutoPass && !l.Won))
                {
                    if (entry.ItemId > 0)
                    {
                        if (!items.TryGetValue(entry.ItemId.Value, out var item))
                        {
                            items[entry.ItemId.Value] = item = [];
                        }

                        var prio = entry.Rank + entry.Bonuses.Sum(b => b.Value) + list.Bonuses.Sum(b => b.Value);

                        if (!item.TryGetValue(prio, out var names))
                        {
                            item[prio] = names = [];
                        }

                        names.Add(list.CharacterName);
                    }
                }
            }

            var exportItems = items.Select(x => new ExportItem(x.Key, x.Value.Select(y => new ExportStanding(y.Key, y.Value.OrderBy(n => n).ToList())).OrderByDescending(s => s.Prio).ToList()))
                .OrderBy(x => x.Id)
                .ToList();

            await using var ms = new MemoryStream();

            await using (var compressionStream = new DeflateStream(ms, CompressionMode.Compress, leaveOpen: true))
            {
                await JsonSerializer.SerializeAsync(compressionStream, exportItems, cancellationToken: cancellationToken);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return await MakePrintableAsync(ms, cancellationToken);
        }
        return string.Empty;
    }

    private static async Task<string> MakePrintableAsync(Stream input, CancellationToken cancellationToken)
    {
        using var bufferOwner = MemoryPool<char>.Shared.Rent((int)input.Length * 2);
        using var bufferOwner2 = MemoryPool<byte>.Shared.Rent(3);
        var inputBuffer = bufferOwner2.Memory[..3];
        var outBuffer = bufferOwner.Memory;
        int outBufferSize = 0;

        int bytesRead = 0;

        while ((bytesRead = await input.ReadAsync(inputBuffer, cancellationToken)) > 0)
        {
            bool eof = bytesRead < 3;

            if (eof)
            {
                inputBuffer.Span[bytesRead..].Clear();
            }

            outBufferSize += Encode(inputBuffer.Span, outBuffer.Span[outBufferSize..], bytesRead);

            if (eof)
            {
                break;
            }
        }

        return outBuffer[..outBufferSize].ToString();

        static int Encode(ReadOnlySpan<byte> input, Span<char> output, int bytesRead)
        {
            if (bytesRead == 0)
            {
                return 0;
            }

            var cache = input[0] + (input[1] * 256) + (input[2] * 65536);
            var b1 = cache % 64;
            cache = (cache - b1) / 64;
            var b2 = cache % 64;
            cache = (cache - b2) / 64;
            var b3 = cache % 64;
            var b4 = (cache - b3) / 64;
            output[0] = _byteTo6BitChar[b1];
            output[1] = _byteTo6BitChar[b2];

            if (bytesRead == 1)
            {
                return 2;
            }

            output[2] = _byteTo6BitChar[b3];

            if (bytesRead == 2)
            {
                return 3;
            }

            output[3] = _byteTo6BitChar[b4];
            return 4;
        }
    }

    private record ExportItem(uint Id, List<ExportStanding> Standings);

    private record ExportStanding(int Prio, List<string> Names);
}
