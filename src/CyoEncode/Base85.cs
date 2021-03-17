// Base85.cs - part of the CyoEncode.NET library
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
    public class Base85 : Encoder
    {
        /// <summary>
        /// Output 'z' instead of '!!!!!'.
        /// </summary>
        public bool FoldZero { get; set; } = true;

        private const int InputBytes = 4;
        private const int OutputChars = 5;

        // Spans

        protected override string EncodeBytes(byte[] input)
        {
            var outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new string(default, outputLen);
            var outputOffset = 0;
            var offset = 0;
            var remaining = input.Length;

            unsafe
            {
                fixed (char* outputPtr = output)
                {
                    while (remaining >= 1)
                    {
                        // Input...
                        uint n = 0;
                        var padding = 0;
                        for (int i = 0; i < InputBytes; ++i)
                        {
                            n <<= 8;
                            if (remaining >= 1)
                            {
                                n |= input[offset++];
                                --remaining;
                            }
                            else
                                ++padding;
                        }
                        if (FoldZero && n == 0)
                        {
                            outputPtr[outputOffset] = 'z';
                            ++outputOffset;
                            continue;
                        }
                        uint n5 = (n % 85);
                        n = (n - n5) / 85;
                        uint n4 = (n % 85);
                        n = (n - n4) / 85;
                        uint n3 = (n % 85);
                        n = (n - n3) / 85;
                        uint n2 = (n % 85);
                        n = (n - n2) / 85;
                        uint n1 = n;

                        // Validate...
                        Debug.Assert(0 <= n1 && n1 < 85);
                        Debug.Assert(0 <= n2 && n2 < 85);
                        Debug.Assert(0 <= n3 && n3 < 85);
                        Debug.Assert(0 <= n4 && n4 < 85);
                        Debug.Assert(0 <= n5 && n5 < 85);

                        // Output...
                        if (padding == 0)
                        {
                            // 5 outputs
                            outputPtr[outputOffset] = (char)(n1 + '!');
                            outputPtr[outputOffset + 1] = (char)(n2 + '!');
                            outputPtr[outputOffset + 2] = (char)(n3 + '!');
                            outputPtr[outputOffset + 3] = (char)(n4 + '!');
                            outputPtr[outputOffset + 4] = (char)(n5 + '!');
                            outputOffset += OutputChars;
                        }
                        else
                        {
                            // Final; 1-4 outputs
                            Debug.Assert(1 <= padding && padding <= 4);
                            outputPtr[outputOffset++] = (char)(n1 + '!');
                            if (padding < 4)
                                outputPtr[outputOffset++] = (char)(n2 + '!');
                            if (padding < 3)
                                outputPtr[outputOffset++] = (char)(n3 + '!');
                            if (padding < 2)
                                outputPtr[outputOffset++] = (char)(n4 + '!');
                            if (padding < 1)
                                outputPtr[outputOffset++] = (char)(n5 + '!');
                        }
                    }
                }
            }

            return output.TrimEnd(default(char));
        }

        protected override byte[] DecodeString(string input)
        {
            var outputLen = CalcOutputLen(input.Length, InputBytes, OutputChars);
            var output = new byte[outputLen];
            var outputOffset = 0;
            var inputOffset = 0;
            var remaining = input.Length;

            while (remaining >= 1)
            {
                if (input[inputOffset] == 'z')
                {
                    if (!FoldZero)
                        throw new BadCharacterException($"Bad character at offset {inputOffset}");
                    ++inputOffset;
                    output[outputOffset] = 0;
                    output[outputOffset + 1] = 0;
                    output[outputOffset + 2] = 0;
                    output[outputOffset + 3] = 0;
                    outputOffset += 4;
                    --remaining;
                    continue;
                }

                // 5 inputs
                var padding = 0;
                var in1 = NextByte(input, inputOffset++, ref remaining, ref padding);
                var in2 = NextByte(input, inputOffset++, ref remaining, ref padding);
                Debug.Assert(padding == 0);
                var in3 = NextByte(input, inputOffset++, ref remaining, ref padding);
                var in4 = NextByte(input, inputOffset++, ref remaining, ref padding);
                var in5 = NextByte(input, inputOffset++, ref remaining, ref padding);

                // Output
                var n = (in1 * Power(85, 4))
                    + (in2 * Power(85, 3))
                    + (in3 * Power(85, 2))
                    + (in4 * Power(85, 1))
                    + in5;
                output[outputOffset++] = (byte)(n >> 24);
                if (padding <= 2)
                {
                    output[outputOffset++] = (byte)(n >> 16);
                    if (padding <= 1)
                    {
                        output[outputOffset++] = (byte)(n >> 8);
                        if (padding == 0)
                        {
                            output[outputOffset++] = (byte)n;
                        }
                    }
                }
            }

            return output.AsSpan(0, outputOffset).ToArray();
        }

        private byte NextByte(string input, int inputOffset, ref int remaining, ref int padding)
        {
            if (inputOffset >= input.Length)
            {
                ++padding;
                return (85 - 1);
            }

            var b = (byte)(input[inputOffset] - '!');
            if (b < 85)
            {
                --remaining;
                return b;
            }

            throw new BadCharacterException($"Bad character at offset {inputOffset}");
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

            EncodeBlock(data, output, false);
        }

        protected override void EncodeEnd(Stream output, object context)
        {
            var data = (EncodingData)context;

            if (data.blockSize == 0)
                return;

            EncodeBlock(data, output, true);
        }

        private void EncodeBlock(EncodingData data, Stream output, bool finalBlock)
        {
            Debug.Assert(finalBlock
                ? 1 <= data.blockSize && data.blockSize <= InputBytes
                : data.blockSize == InputBytes);

            // Input...
            if (FoldZero && (data.blockData == 0) && (data.blockSize == InputBytes))
            {
                output.WriteByte((byte)'z');
                data.blockSize = 0;
                return;
            }
            data.blockData <<= (8 * (InputBytes - data.blockSize));
            var n5 = (byte)(data.blockData % 85);
            data.blockData = (data.blockData - n5) / 85;
            var n4 = (byte)(data.blockData % 85);
            data.blockData = (data.blockData - n4) / 85;
            var n3 = (byte)(data.blockData % 85);
            data.blockData = (data.blockData - n3) / 85;
            var n2 = (byte)(data.blockData % 85);
            data.blockData = (data.blockData - n2) / 85;
            var n1 = (byte)data.blockData;

            // Validate...
            Debug.Assert(0 <= n1 && n1 < 85);
            Debug.Assert(0 <= n2 && n2 < 85);
            Debug.Assert(0 <= n3 && n3 < 85);
            Debug.Assert(0 <= n4 && n4 < 85);
            Debug.Assert(0 <= n5 && n5 < 85);

            // Output...
            if (data.blockSize == OutputChars)
            {
                output.WriteByte((byte)(n1 + '!'));
                output.WriteByte((byte)(n2 + '!'));
                output.WriteByte((byte)(n3 + '!'));
                output.WriteByte((byte)(n4 + '!'));
                output.WriteByte((byte)(n5 + '!'));
            }
            else
            {
                output.WriteByte((byte)(n1 + '!'));
                if (data.blockSize >= 1)
                    output.WriteByte((byte)(n2 + '!'));
                if (data.blockSize >= 2)
                    output.WriteByte((byte)(n3 + '!'));
                if (data.blockSize >= 3)
                    output.WriteByte((byte)(n4 + '!'));
                if (data.blockSize >= 4)
                    output.WriteByte((byte)(n5 + '!'));
            }

            // Reset...
            data.blockSize = 0;
            data.blockData = 0;
        }

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = (DecodingData)context;

            ++data.offset;

            if (c == 'z')
            {
                if (!FoldZero || (data.blockSize != 0))
                    throw new BadCharacterException($"Bad character at offset {data.offset}");

                for (var i = 0; i < InputBytes; ++i)
                    output.WriteByte(0);

                return;
            }

            ++data.blockSize;
            data.blockData <<= 8;
            data.blockData |= (byte)(c - '!');

            if (data.blockSize != OutputChars)
                return;

            DecodeBlock(data, output, false);
        }

        protected override void DecodeEnd(Stream output, object context)
        {
            var data = (DecodingData)context;

            if (data.blockSize == 0)
                return;

            DecodeBlock(data, output, true);
        }

        private void DecodeBlock(DecodingData data, Stream output, bool finalBlock)
        {
            Debug.Assert(1 <= data.blockSize && data.blockSize <= OutputChars);

            // Padding...
            data.blockData <<= (8 * (OutputChars - data.blockSize));

            // Inputs...
            var in1 = (byte)(data.blockData >> 32);
            var in2 = data.blockSize <= 1 ? 84 : (byte)(data.blockData >> 24);
            var in3 = data.blockSize <= 2 ? 84 : (byte)(data.blockData >> 16);
            var in4 = data.blockSize <= 3 ? 84 : (byte)(data.blockData >> 8);
            var in5 = data.blockSize <= 4 ? 84 : (byte)data.blockData;

            // Validate...
            Debug.Assert(0 <= in1 && in1 < 85);
            Debug.Assert(0 <= in2 && in2 < 85);
            Debug.Assert(0 <= in3 && in3 < 85);
            Debug.Assert(0 <= in4 && in4 < 85);
            Debug.Assert(0 <= in5 && in5 < 85);

            // Outputs...
            var n = (in1 * Power(85, 4))
                + (in2 * Power(85, 3))
                + (in3 * Power(85, 2))
                + (in4 * Power(85, 1))
                + in5;
            if (data.blockSize == OutputChars)
            {
                output.WriteByte((byte)(n >> 24));
                output.WriteByte((byte)(n >> 16));
                output.WriteByte((byte)(n >> 8));
                output.WriteByte((byte)n);
            }
            else
            {
                output.WriteByte((byte)(n >> 24));
                if (data.blockSize >= 3)
                {
                    output.WriteByte((byte)(n >> 16));
                    if (data.blockSize >= 4)
                    {
                        output.WriteByte((byte)(n >> 8));
                    }
                }
            }

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
        }

        private uint Power(byte num, int count)
        {
            var total = 1u;
            for (var i = 0; i < count; ++i)
                total *= num;
            return total;
        }
    }
}
