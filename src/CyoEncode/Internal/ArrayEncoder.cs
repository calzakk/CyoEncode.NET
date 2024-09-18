// Internal/ArrayEncoder.cs - part of the CyoEncode.NET library
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

namespace CyoEncode.Internal;

internal static class ArrayEncoder
{
    public static string Encode(byte[] input, Func<byte[], string> encodeBytes)
    {
        if (input.Length == 0)
            return string.Empty;

        return encodeBytes(input);
    }

    public static byte[] Decode(string input, Func<string, byte[]> decodeString)
    {
        if (input.Length == 0)
            return Array.Empty<byte>();

        return decodeString(input);
    }

    public static int GetLengthOfOutputString(int inputLength, int inputBytes, int outputChars)
    {
        return (((inputLength + inputBytes - 1) / inputBytes) * outputChars);
    }

    public static int GetLengthOfOutputBuffer(int encodedLength, int inputBytes, int outputChars)
    {
        return (((encodedLength + outputChars - 1) / outputChars) * inputBytes);
    }

    public static void EnsurePadding(byte value, byte padding, int offset)
    {
        if (value != padding)
            throw new BadCharacterException($"Bad character at offset {offset}");
    }

    public static void EnsureNotPadding(byte value, byte padding, int offset)
    {
        if (value == padding)
            throw new BadCharacterException($"Bad character at offset {offset}");
    }

    public static void ValidatePadding(byte value, byte padding, int offset, ref bool expectedPadding)
    {
        if (value == padding)
            expectedPadding = true;
        else if (expectedPadding)
            throw new BadCharacterException($"Bad character at offset {offset}");
    }
}