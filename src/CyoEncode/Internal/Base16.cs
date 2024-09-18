// Internal/Base16.cs - part of the CyoEncode.NET library
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

using System.Diagnostics;
using System.IO;

namespace CyoEncode.Internal;

internal class Base16 : Encoder
{
    private const int InputBytes = 1;
    private const int OutputChars = 2;
    private const int MaxValue = 15;
    private readonly char[] _encodeTable;
    private readonly byte[] _decodeTable;
    private readonly int _bufferSize;

    public Base16(int bufferSize)
    {
        var (encode, decode) = Tables.Init("0123456789ABCDEF");
        _encodeTable = encode;
        _decodeTable = decode;
        _bufferSize = bufferSize;
    }

    protected override int GetBufferSize() => _bufferSize;

    // Arrays

    protected override string EncodeBytes(byte[] input)
    {
        var outputLen = ArrayEncoder.GetLengthOfOutputString(input.Length, InputBytes, OutputChars);
        var output = new string(default, outputLen);
        var outputOffset = 0;

        unsafe
        {
            fixed (char* outputPtr = output)
            {
                foreach (var b in input)
                {
                    // Input...
                    var n1 = ((b & 0xf0) >> 4);
                    var n2 = (b & 0x0f);

                    // Validate...
                    Debug.Assert(0 <= n1 && n1 <= MaxValue);
                    Debug.Assert(0 <= n2 && n2 <= MaxValue);

                    // Output...
                    outputPtr[outputOffset] = _encodeTable[n1];
                    outputPtr[outputOffset + 1] = _encodeTable[n2];
                    outputOffset += OutputChars;
                }
            }
        }

        return output;
    }

    protected override byte[] DecodeString(string input)
    {
        if ((input.Length % OutputChars) != 0)
            throw new BadLengthException($"Encoding has bad length: {input.Length}");

        var outputLen = ArrayEncoder.GetLengthOfOutputBuffer(input.Length, InputBytes, OutputChars);
        var output = new byte[outputLen];
        var outputOffset = 0;
        var inputOffset = 0;
        var remaining = input.Length;

        while (remaining != 0)
        {
            // Inputs...
            var in1 = GetNextChar(input, ref inputOffset, ref remaining);
            var in2 = GetNextChar(input, ref inputOffset, ref remaining);

            // Outputs...
            output[outputOffset] = (byte)((in1 << 4) | in2);
            outputOffset += InputBytes;
        }

        Debug.Assert(outputOffset == outputLen);

        return output;
    }

    private byte GetNextChar(string input, ref int inputOffset, ref int remaining)
    {
        var b = _decodeTable[input[inputOffset]];
        if (b == Tables.InvalidChar)
            throw new BadCharacterException($"Bad character at offset {inputOffset}");

        ++inputOffset;
        --remaining;
        return b;
    }

    // Streams

    private class DecodingData
    {
        public int Offset = 0;
        public int BlockSize = 0;
        public byte BlockData = 0;
    }

    protected override object EncodeStart() => null;

    protected override void EncodeByte(byte b, Stream output, object context)
    {
        var n1 = ((b & 0xf0) >> 4);
        var n2 = (b & 0x0f);

        Debug.Assert(0 <= n1 && n1 <= MaxValue);
        Debug.Assert(0 <= n2 && n2 <= MaxValue);

        output.WriteByte((byte)_encodeTable[n1]);
        output.WriteByte((byte)_encodeTable[n2]);
    }

    protected override void EncodeEnd(Stream output, object context)
    {
        //nothing to do
    }

    protected override object DecodeStart() => new DecodingData();

    protected override void DecodeChar(char c, Stream output, object context)
    {
        var data = (DecodingData)context;

        ++data.Offset;

        var b = _decodeTable[c];
        if (b == Tables.InvalidChar)
            throw new BadCharacterException($"Bad character at offset {data.Offset}");

        ++data.BlockSize;
        data.BlockData <<= 4;
        data.BlockData |= b;

        if (data.BlockSize != OutputChars)
            return;

        output.WriteByte(data.BlockData);

        data.BlockSize = 0;
        data.BlockData = 0;
    }

    protected override void DecodeEnd(Stream output, object context)
    {
        var data = (DecodingData)context;

        if (data.BlockSize == 0)
            return;

        if (data.BlockSize != OutputChars)
            throw new BadLengthException($"Encoding has bad length: {data.Offset}");

        output.WriteByte(data.BlockData);
    }
}