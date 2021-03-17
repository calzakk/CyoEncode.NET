// Base32.cs - part of the CyoEncode.NET library
//
// MIT License
//
// Copyright(c) 2017-2021 Graham Bull
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

namespace CyoEncode
{
    public sealed class Base32 : Encoder
    {
        public bool OptionalPadding { get; set; } = false;

        private const int InputBytes = 5;
        private const int OutputChars = 8;
        private const int Padding = 32;
        private static readonly char[] EncodeTable = new char[33];
        private static readonly byte[] DecodeTable = new byte[128];

        static Base32()
        {
            const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567=";
            InitEncodeTable(EncodeTable, charset);
            InitDecodeTable(DecodeTable, charset);
        }

        // Spans

        protected override string EncodeBytes(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new string(default, outputLen);
            var outputOffset = 0;
            int offset = 0;
            int remaining = input.Length;

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
                        outputPtr[outputOffset] = EncodeTable[n1];
                        outputPtr[outputOffset + 1] = EncodeTable[n2];
                        outputPtr[outputOffset + 2] = EncodeTable[n3];
                        outputPtr[outputOffset + 3] = EncodeTable[n4];
                        outputPtr[outputOffset + 4] = EncodeTable[n5];
                        outputPtr[outputOffset + 5] = EncodeTable[n6];
                        outputPtr[outputOffset + 6] = EncodeTable[n7];
                        outputPtr[outputOffset + 7] = EncodeTable[n8];
                        outputOffset += OutputChars;
                    }
                }
            }

            return output;
        }

        protected override byte[] DecodeString(string input)
        {
            if ((input.Length % OutputChars) != 0 && !OptionalPadding)
                throw new BadLengthException($"Encoding has bad length: {input.Length}");

            var outputLen = CalcOutputLen(input.Length, InputBytes, OutputChars);
            var output = new byte[outputLen];
            var outputOffset = 0;
            var inputOffset = 0;
            var remaining = input.Length;

            while (remaining != 0)
            {
                // Inputs...
                var startOffset = inputOffset;
                var in1 = NextChar(input, ref inputOffset, ref remaining);
                var in2 = NextChar(input, ref inputOffset, ref remaining);
                var in3 = NextChar(input, ref inputOffset, ref remaining);
                var in4 = NextChar(input, ref inputOffset, ref remaining);
                var in5 = NextChar(input, ref inputOffset, ref remaining);
                var in6 = NextChar(input, ref inputOffset, ref remaining);
                var in7 = NextChar(input, ref inputOffset, ref remaining);
                var in8 = NextChar(input, ref inputOffset, ref remaining);

                // Validate padding...
                EnsureNotPadding(in1, Padding, startOffset);
                EnsureNotPadding(in2, Padding, startOffset + 1);
                if (remaining >= 1)
                {
                    EnsureNotPadding(in3, Padding, startOffset + 2);
                    EnsureNotPadding(in4, Padding, startOffset + 3);
                    EnsureNotPadding(in5, Padding, startOffset + 4);
                    EnsureNotPadding(in6, Padding, startOffset + 5);
                    EnsureNotPadding(in7, Padding, startOffset + 6);
                    EnsureNotPadding(in8, Padding, startOffset + 7);
                }
                else
                {
                    var padding = false;
                    ValidatePadding(in3, Padding, startOffset + 2, ref padding);
                    ValidatePadding(in4, Padding, startOffset + 3, ref padding);
                    ValidatePadding(in5, Padding, startOffset + 4, ref padding);
                    ValidatePadding(in6, Padding, startOffset + 5, ref padding);
                    ValidatePadding(in7, Padding, startOffset + 6, ref padding);
                    ValidatePadding(in8, Padding, startOffset + 7, ref padding);
                }

                // Outputs...
                output[outputOffset] = (byte)(((in1 & 0x1f) << 3) | ((in2 & 0x1c) >> 2));
                output[outputOffset + 1] = (byte)(((in2 & 0x03) << 6) | ((in3 & 0x1f) << 1) | ((in4 & 0x10) >> 4));
                output[outputOffset + 2] = (byte)(((in4 & 0x0f) << 4) | ((in5 & 0x1e) >> 1));
                output[outputOffset + 3] = (byte)(((in5 & 0x01) << 7) | ((in6 & 0x1f) << 2) | ((in7 & 0x18) >> 3));
                output[outputOffset + 4] = (byte)(((in7 & 0x07) << 5) | (in8 & 0x1f));
                outputOffset += InputBytes;

                // Padding...
                if (in8 == Padding)
                {
                    --outputOffset;
                    Debug.Assert((in7 == Padding && in6 == Padding) || (in7 != Padding));
                    if (in6 == Padding)
                    {
                        --outputOffset;
                        if (in5 == Padding)
                        {
                            --outputOffset;
                            Debug.Assert((in4 == Padding && in3 == Padding) || (in4 != Padding));
                            if (in3 == Padding)
                                --outputOffset;
                        }
                    }
                }
            }

            return output.AsSpan(0, outputOffset).ToArray();
        }

        private byte NextChar(string input, ref int inputOffset, ref int remaining)
        {
            if (remaining == 0)
                return Padding;

            var b = DecodeTable[input[inputOffset]];
            if (b == Invalid)
                throw new BadCharacterException($"Bad character at offset {inputOffset}");

            ++inputOffset;
            --remaining;
            return b;
        }

        // Streams

        private class EncodingData
        {
            public int offset = 0;
            public int blockSize = 0;
            public ulong blockData = 0;
        }

        private class DecodingData
        {
            public int offset = 0;
            public int blockSize = 0;
            public ulong blockData = 0;
            public int padding = 0;
        }

        protected override object CreateEncodingData() => new EncodingData();

        protected override object CreateDecodingData() => new DecodingData();

        protected override void EncodeByte(byte b, Stream output, object context)
        {
            var data = (EncodingData)context;

            ++data.offset;

            ++data.blockSize;
            data.blockData <<= 8;
            data.blockData |= b;

            if (data.blockSize < InputBytes)
                return;

            EncodeBlock(data, output);
        }

        protected override void EncodeEnd(Stream output, object context)
        {
            var data = (EncodingData)context;

            if (data.blockSize == 0)
                return;

            EncodeBlock(data, output);
        }

        private void EncodeBlock(EncodingData data, Stream output)
        {
            Debug.Assert(data.blockSize <= InputBytes);

            // Padding...
            data.blockData <<= (8 * (InputBytes - data.blockSize));

            // Input...
            var in1 = (byte)(data.blockData >> 32);
            var in2 = (byte)(data.blockData >> 24);
            var in3 = (byte)(data.blockData >> 16);
            var in4 = (byte)(data.blockData >> 8);
            var in5 = (byte)data.blockData;
            var n1 = (byte)((in1 & 0xf8) >> 3);
            var n2 = (byte)((in1 & 0x07) << 2);
            var n3 = Padding;
            var n4 = Padding;
            var n5 = Padding;
            var n6 = Padding;
            var n7 = Padding;
            var n8 = Padding;
            if (data.blockSize >= 2)
            {
                n2 |= (byte)((in2 & 0xc0) >> 6);
                n3 = (byte)((in2 & 0x3e) >> 1);
                n4 = (byte)((in2 & 0x01) << 4);
                if (data.blockSize >= 3)
                {
                    n4 |= (byte)((in3 & 0xf0) >> 4);
                    n5 = (byte)((in3 & 0x0f) << 1);
                    if (data.blockSize >= 4)
                    {
                        n5 |= (byte)((in4 & 0x80) >> 7);
                        n6 = (byte)((in4 & 0x7c) >> 2);
                        n7 = (byte)((in4 & 0x03) << 3);
                        if (data.blockSize == 5)
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
            output.WriteByte((byte)EncodeTable[n1]);
            output.WriteByte((byte)EncodeTable[n2]);
            output.WriteByte((byte)EncodeTable[n3]);
            output.WriteByte((byte)EncodeTable[n4]);
            output.WriteByte((byte)EncodeTable[n5]);
            output.WriteByte((byte)EncodeTable[n6]);
            output.WriteByte((byte)EncodeTable[n7]);
            output.WriteByte((byte)EncodeTable[n8]);

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
        }

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = (DecodingData)context;

            ++data.offset;

            byte b = DecodeTable[c];
            if (b == Invalid || (b != Padding && data.padding >= 1))
                throw new BadCharacterException($"Bad character at offset {data.offset}");

            if (b == Padding)
            {
                ++data.padding;
                return;
            }

            ++data.blockSize;
            data.blockData <<= 8;
            data.blockData |= b;

            if (data.blockSize != OutputChars)
                return;

            DecodeBlock(data, output);
        }

        protected override void DecodeEnd(Stream output, object context)
        {
            var data = (DecodingData)context;

            if (data.blockSize == 0)
                return;

            var blockSize = (data.blockSize + data.padding);
            if (blockSize != OutputChars && !OptionalPadding)
                throw new BadLengthException($"Encoding has bad length: {data.offset}");

            DecodeBlock(data, output);
        }

        private void DecodeBlock(DecodingData data, Stream output)
        {
            Debug.Assert(2 <= data.blockSize && data.blockSize <= OutputChars);

            // Padding...
            data.blockData <<= (8 * (OutputChars - data.blockSize));

            // Inputs...
            var in1 = (byte)(data.blockData >> 56);
            var in2 = (byte)(data.blockData >> 48);
            var in3 = (byte)(data.blockData >> 40);
            var in4 = (byte)(data.blockData >> 32);
            var in5 = (byte)(data.blockData >> 24);
            var in6 = (byte)(data.blockData >> 16);
            var in7 = (byte)(data.blockData >> 8);
            var in8 = (byte)data.blockData;

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
            if (data.blockSize == OutputChars)
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
                if (data.blockSize >= 4)
                {
                    output.WriteByte((byte)(((in2 & 0x03) << 6) | ((in3 & 0x1f) << 1) | ((in4 & 0x10) >> 4)));
                    if (data.blockSize >= 5)
                    {
                        output.WriteByte((byte)(((in4 & 0x0f) << 4) | ((in5 & 0x1e) >> 1)));
                        if (data.blockSize >= 6)
                        {
                            output.WriteByte((byte)(((in5 & 0x01) << 7) | ((in6 & 0x1f) << 2) | ((in7 & 0x18) >> 3)));
                        }
                    }
                }
            }

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
            data.padding = 0;
        }
    }
}
