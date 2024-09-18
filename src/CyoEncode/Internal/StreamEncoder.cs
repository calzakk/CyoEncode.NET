// Internal/StreamEncoder.cs - part of the CyoEncode.NET library
//
// MIT License
//
// Copyright(c) 2017-2024 Graham Bull
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.IO;
using System.Threading.Tasks;

namespace CyoEncode.Internal;

internal static class StreamEncoder
{
    public static async Task EncodeAsync(Stream input, Stream output, int bufferSize,
        Func<object> encodeStart,
        Action<byte, Stream, object> encodeByte,
        Action<Stream, object> encodeEnd)
    {
        var buffer = new byte[bufferSize];
        var context = encodeStart();

        while (true)
        {
            var length = await input.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (length == 0)
                break;

            for (var i = 0; i < length; ++i)
                encodeByte(buffer[i], output, context);
        }

        encodeEnd(output, context);
    }

    public static async Task DecodeAsync(Stream input, Stream output, int bufferSize,
        Func<object> decodeStart,
        Action<char, Stream, object> decodeChar,
        Action<Stream, object> decodeEnd)
    {
        var buffer = new byte[bufferSize];
        var context = decodeStart();

        while (true)
        {
            var length = await input.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (length == 0)
                break;

            for (var i = 0; i < length; ++i)
                decodeChar((char)buffer[i], output, context);
        }

        decodeEnd(output, context);
    }
}