// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Buffers;

namespace ValhallaLootList.Server;

public static class WeakAurasEncoder
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

    public static async Task<string> EncodeAsync(Stream input, CancellationToken cancellationToken)
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
}
