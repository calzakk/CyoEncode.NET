// Internal/Base32.cs - part of the CyoEncode.NET library
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
using System.Diagnostics;
using System.IO;

namespace CyoEncode.Internal;

internal class Base32 : Encoder
{
    private const int InputBytes = 5;
    private const int OutputChars = 8;
    private const int Padding = 32;
    private readonly char[] _encodeTable;
    private readonly byte[] _decodeTable;
    private readonly int _bufferSize;
    private readonly bool _optionalPadding;

    public Base32(int bufferSize, bool optionalPadding)
    {
        var (encode, decode) = Tables.Init("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567=");
        _encodeTable = encode;
        _decodeTable = decode;
        _bufferSize = bufferSize;
        _optionalPadding = optionalPadding;
    }

    protected override int GetBufferSize() => _bufferSize;

    // Arrays

    protected override string EncodeBytes(byte[] input)
    {
        var outputLen = ArrayEncoder.GetLengthOfOutputString(input.Length, InputBytes, OutputChars);
        var output = new string(default, outputLen);
        var outputOffset = 0;
        var offset = 0;
        var remaining = input.Length;

        unsafe
        {
            fixed (char* outputPtr = output)
            {
                while (remaining != 0)
                {
                    // Input...
                    var blockSize = (remaining < InputBytes ? remaining : InputBytes);
                    Debug.Assert(blockSize >= 1);
                    var n1 = ((input[offset] & 0xf8) >> 3);
                    var n2 = ((input[offset] & 0x07) << 2);
                    var n3 = Padding;
                    var n4 = Padding;
                    var n5 = Padding;
                    var n6 = Padding;
                    var n7 = Padding;
                    var n8 = Padding;
                    if (blockSize >= 2)
                    {
                        n2 |= (input[offset + 1] & 0xc0) >> 6;
                        n3 = (input[offset + 1] & 0x3e) >> 1;
                        n4 = (input[offset + 1] & 0x01) << 4;
                        if (blockSize >= 3)
                        {
                            n4 |= (input[offset + 2] & 0xf0) >> 4;
                            n5 = (input[offset + 2] & 0x0f) << 1;
                            if (blockSize >= 4)
                            {
                                n5 |= (input[offset + 3] & 0x80) >> 7;
                                n6 = (input[offset + 3] & 0x7c) >> 2;
                                n7 = (input[offset + 3] & 0x03) << 3;
                                if (blockSize >= 5)
                                {
                                    n7 |= (input[offset + 4] & 0xe0) >> 5;
                                    n8 = (input[offset + 4] & 0x1f);
                                }
                            }
                        }
                    }
                    offset += blockSize;
                    remaining -= blockSize;

                    // Validate...
                    Debug.Assert(0 <= n1 && n1 <= Padding);
                    Debug.Assert(0 <= n2 && n2 <= Padding);
                    Debug.Assert(0 <= n3 && n3 <= Padding);
                    Debug.Assert(0 <= n4 && n4 <= Padding);
                    Debug.Assert(0 <= n5 && n5 <= Padding);
                    Debug.Assert(0 <= n6 && n6 <= Padding);
                    Debug.Assert(0 <= n7 && n7 <= Padding);
                    Debug.Assert(0 <= n8 && n8 <= Padding);

                    // Output...
                    outputPtr[outputOffset] = _encodeTable[n1];
                    outputPtr[outputOffset + 1] = _encodeTable[n2];
                    outputPtr[outputOffset + 2] = _encodeTable[n3];
                    outputPtr[outputOffset + 3] = _encodeTable[n4];
                    outputPtr[outputOffset + 4] = _encodeTable[n5];
                    outputPtr[outputOffset + 5] = _encodeTable[n6];
                    outputPtr[outputOffset + 6] = _encodeTable[n7];
                    outputPtr[outputOffset + 7] = _encodeTable[n8];
                    outputOffset += OutputChars;
                }
            }
        }

        return output;
    }

    protected override byte[] DecodeString(string input)
    {
        if ((input.Length % OutputChars) != 0 && !_optionalPadding)
            throw new BadLengthException($"Encoding has bad length: {input.Length}");

        var outputLen = ArrayEncoder.GetLengthOfOutputBuffer(input.Length, InputBytes, OutputChars);
        var output = new byte[outputLen];
        var outputOffset = 0;
        var inputOffset = 0;
        var remaining = input.Length;

        while (remaining != 0)
        {
            // Inputs...
            var startOffset = inputOffset;
            var in1 = GetNextByte(input, ref inputOffset, ref remaining);
            var in2 = GetNextByte(input, ref inputOffset, ref remaining);
            var in3 = GetNextByte(input, ref inputOffset, ref remaining);
            var in4 = GetNextByte(input, ref inputOffset, ref remaining);
            var in5 = GetNextByte(input, ref inputOffset, ref remaining);
            var in6 = GetNextByte(input, ref inputOffset, ref remaining);
            var in7 = GetNextByte(input, ref inputOffset, ref remaining);
            var in8 = GetNextByte(input, ref inputOffset, ref remaining);

            // Validate padding...
            ArrayEncoder.EnsureNotPadding(in1, Padding, startOffset);
            ArrayEncoder.EnsureNotPadding(in2, Padding, startOffset + 1);
            if (remaining >= 1)
            {
                ArrayEncoder.EnsureNotPadding(in3, Padding, startOffset + 2);
                ArrayEncoder.EnsureNotPadding(in4, Padding, startOffset + 3);
                ArrayEncoder.EnsureNotPadding(in5, Padding, startOffset + 4);
                ArrayEncoder.EnsureNotPadding(in6, Padding, startOffset + 5);
                ArrayEncoder.EnsureNotPadding(in7, Padding, startOffset + 6);
                ArrayEncoder.EnsureNotPadding(in8, Padding, startOffset + 7);
            }
            else
            {
                var padding = false;
                ArrayEncoder.ValidatePadding(in3, Padding, startOffset + 2, ref padding);
                ArrayEncoder.ValidatePadding(in4, Padding, startOffset + 3, ref padding);
                ArrayEncoder.ValidatePadding(in5, Padding, startOffset + 4, ref padding);
                ArrayEncoder.ValidatePadding(in6, Padding, startOffset + 5, ref padding);
                ArrayEncoder.ValidatePadding(in7, Padding, startOffset + 6, ref padding);
                ArrayEncoder.ValidatePadding(in8, Padding, startOffset + 7, ref padding);
            }

            // Outputs...
            output[outputOffset] = (byte)(((in1 & 0x1f) << 3) | ((in2 & 0x1c) >> 2));
            output[outputOffset + 1] = (byte)(((in2 & 0x03) << 6) | ((in3 & 0x1f) << 1) | ((in4 & 0x10) >> 4));
            output[outputOffset + 2] = (byte)(((in4 & 0x0f) << 4) | ((in5 & 0x1e) >> 1));
            output[outputOffset + 3] = (byte)(((in5 & 0x01) << 7) | ((in6 & 0x1f) << 2) | ((in7 & 0x18) >> 3));
            output[outputOffset + 4] = (byte)(((in7 & 0x07) << 5) | (in8 & 0x1f));
            outputOffset += InputBytes;

            // Padding...
            if (in8 != Padding)
                continue;
            --outputOffset;
            Debug.Assert((in7 == Padding && in6 == Padding) || (in7 != Padding));
            if (in6 != Padding)
                continue;
            --outputOffset;
            if (in5 != Padding)
                continue;
            --outputOffset;
            Debug.Assert((in4 == Padding && in3 == Padding) || (in4 != Padding));
            if (in3 != Padding)
                continue;
            --outputOffset;
        }

        return output.AsSpan(0, outputOffset).ToArray();
    }

    private byte GetNextByte(string input, ref int inputOffset, ref int remaining)
    {
        if (remaining == 0)
            return Padding;

        var b = _decodeTable[input[inputOffset]];
        if (b == Tables.InvalidChar)
            throw new BadCharacterException($"Bad character at offset {inputOffset}");

        ++inputOffset;
        --remaining;
        return b;
    }

    // Streams

    private class EncodingData
    {
        public int BlockSize = 0;
        public ulong BlockData = 0;
    }

    private class DecodingData
    {
        public int Offset = 0;
        public int BlockSize = 0;
        public ulong BlockData = 0;
        public int PaddingSize = 0;
    }

    protected override object EncodeStart() => new EncodingData();

    protected override void EncodeByte(byte b, Stream output, object context)
    {
        var data = (EncodingData)context;

        ++data.BlockSize;
        data.BlockData <<= 8;
        data.BlockData |= b;

        if (data.BlockSize < InputBytes)
            return;

        EncodeBlock(output, data);
    }

    protected override void EncodeEnd(Stream output, object context)
    {
        var data = (EncodingData)context;

        if (data.BlockSize == 0)
            return;

        EncodeBlock(output, data);
    }

    private void EncodeBlock(Stream output, EncodingData data)
    {
        Debug.Assert(data.BlockSize <= InputBytes);

        // Padding...
        data.BlockData <<= (8 * (InputBytes - data.BlockSize));

        // Input...
        var in1 = (byte)(data.BlockData >> 32);
        var in2 = (byte)(data.BlockData >> 24);
        var in3 = (byte)(data.BlockData >> 16);
        var in4 = (byte)(data.BlockData >> 8);
        var in5 = (byte)data.BlockData;
        var n1 = (byte)((in1 & 0xf8) >> 3);
        var n2 = (byte)((in1 & 0x07) << 2);
        var n3 = Padding;
        var n4 = Padding;
        var n5 = Padding;
        var n6 = Padding;
        var n7 = Padding;
        var n8 = Padding;
        if (data.BlockSize >= 2)
        {
            n2 |= (byte)((in2 & 0xc0) >> 6);
            n3 = (byte)((in2 & 0x3e) >> 1);
            n4 = (byte)((in2 & 0x01) << 4);
            if (data.BlockSize >= 3)
            {
                n4 |= (byte)((in3 & 0xf0) >> 4);
                n5 = (byte)((in3 & 0x0f) << 1);
                if (data.BlockSize >= 4)
                {
                    n5 |= (byte)((in4 & 0x80) >> 7);
                    n6 = (byte)((in4 & 0x7c) >> 2);
                    n7 = (byte)((in4 & 0x03) << 3);
                    if (data.BlockSize == 5)
                    {
                        n7 |= (byte)((in5 & 0xe0) >> 5);
                        n8 = (byte)(in5 & 0x1f);
                    }
                }
            }
        }

        // Validate...
        Debug.Assert(0 <= n1 && n1 <= Padding);
        Debug.Assert(0 <= n2 && n2 <= Padding);
        Debug.Assert(0 <= n3 && n3 <= Padding);
        Debug.Assert(0 <= n4 && n4 <= Padding);
        Debug.Assert(0 <= n5 && n5 <= Padding);
        Debug.Assert(0 <= n6 && n6 <= Padding);
        Debug.Assert(0 <= n7 && n7 <= Padding);
        Debug.Assert(0 <= n8 && n8 <= Padding);

        // Output...
        output.WriteByte((byte)_encodeTable[n1]);
        output.WriteByte((byte)_encodeTable[n2]);
        output.WriteByte((byte)_encodeTable[n3]);
        output.WriteByte((byte)_encodeTable[n4]);
        output.WriteByte((byte)_encodeTable[n5]);
        output.WriteByte((byte)_encodeTable[n6]);
        output.WriteByte((byte)_encodeTable[n7]);
        output.WriteByte((byte)_encodeTable[n8]);

        // Reset block...
        data.BlockSize = 0;
        data.BlockData = 0;
    }

    protected override object DecodeStart() => new DecodingData();

    protected override void DecodeChar(char c, Stream output, object context)
    {
        var data = (DecodingData)context;

        ++data.Offset;

        var b = _decodeTable[c];
        if (b == Tables.InvalidChar || (b != Padding && data.PaddingSize >= 1))
            throw new BadCharacterException($"Bad character at offset {data.Offset}");

        if (b == Padding)
        {
            ++data.PaddingSize;
            return;
        }

        ++data.BlockSize;
        data.BlockData <<= 8;
        data.BlockData |= b;

        if (data.BlockSize != OutputChars)
            return;

        DecodeBlock(output, data);
    }

    protected override void DecodeEnd(Stream output, object context)
    {
        var data = (DecodingData)context;

        if (data.BlockSize == 0)
            return;

        var blockSize = (data.BlockSize + data.PaddingSize);
        if (blockSize != OutputChars && !_optionalPadding)
            throw new BadLengthException($"Encoding has bad length: {data.Offset}");

        DecodeBlock(output, data);
    }

    private static void DecodeBlock(Stream output, DecodingData data)
    {
        Debug.Assert(2 <= data.BlockSize && data.BlockSize <= OutputChars);

        // Padding...
        data.BlockData <<= (8 * (OutputChars - data.BlockSize));

        // Inputs...
        var in1 = (byte)(data.BlockData >> 56);
        var in2 = (byte)(data.BlockData >> 48);
        var in3 = (byte)(data.BlockData >> 40);
        var in4 = (byte)(data.BlockData >> 32);
        var in5 = (byte)(data.BlockData >> 24);
        var in6 = (byte)(data.BlockData >> 16);
        var in7 = (byte)(data.BlockData >> 8);
        var in8 = (byte)data.BlockData;

        // Validate...
        Debug.Assert(0 <= in1 && in1 < Padding); //cannot be padding
        Debug.Assert(0 <= in2 && in2 < Padding); //cannot be padding
        Debug.Assert(0 <= in3 && in3 <= Padding);
        Debug.Assert(0 <= in4 && in4 <= Padding);
        Debug.Assert(0 <= in5 && in5 <= Padding);
        Debug.Assert(0 <= in6 && in6 <= Padding);
        Debug.Assert(0 <= in7 && in7 <= Padding);
        Debug.Assert(0 <= in8 && in8 <= Padding);

        // Outputs...
        if (data.BlockSize == OutputChars)
        {
            output.WriteByte((byte)(((in1 & 0x1f) << 3) | ((in2 & 0x1c) >> 2)));
            output.WriteByte((byte)(((in2 & 0x03) << 6) | ((in3 & 0x1f) << 1) | ((in4 & 0x10) >> 4)));
            output.WriteByte((byte)(((in4 & 0x0f) << 4) | ((in5 & 0x1e) >> 1)));
            output.WriteByte((byte)(((in5 & 0x01) << 7) | ((in6 & 0x1f) << 2) | ((in7 & 0x18) >> 3)));
            output.WriteByte((byte)(((in7 & 0x07) << 5) | (in8 & 0x1f)));
        }
        else
        {
            output.WriteByte((byte)(((in1 & 0x1f) << 3) | ((in2 & 0x1c) >> 2)));
            if (data.BlockSize >= 4)
            {
                output.WriteByte((byte)(((in2 & 0x03) << 6) | ((in3 & 0x1f) << 1) | ((in4 & 0x10) >> 4)));
                if (data.BlockSize >= 5)
                {
                    output.WriteByte((byte)(((in4 & 0x0f) << 4) | ((in5 & 0x1e) >> 1)));
                    if (data.BlockSize >= 6)
                    {
                        output.WriteByte((byte)(((in5 & 0x01) << 7) | ((in6 & 0x1f) << 2) | ((in7 & 0x18) >> 3)));
                    }
                }
            }
        }

        // Reset block...
        data.BlockSize = 0;
        data.BlockData = 0;
        data.PaddingSize = 0;
    }
}