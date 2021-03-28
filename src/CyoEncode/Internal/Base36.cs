// Internal/Base36.cs - part of the CyoEncode.NET library
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

namespace CyoEncode.Internal
{
    internal class Base36 : Encoder
    {
        private const int InputBytes = 4;
        private const int OutputChars = 6;
        private readonly char[] _encodeTable;
        private readonly byte[] _decodeTable;
        private readonly int _bufferSize;

        public Base36(int bufferSize)
        {
            var (encode, decode) = Tables.Init("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ=");
            _encodeTable = encode;
            _decodeTable = decode;
            _bufferSize = bufferSize;
        }

        protected override int GetBufferSize() => _bufferSize;

        // Arrays

        protected override string EncodeBytes(byte[] input)
        {
            var maxOutputLen = ArrayEncoder.GetLengthOfOutputString(input.Length, InputBytes, OutputChars);
            var output = new string(default, maxOutputLen);
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
                        var n6 = (n % 36);
                        n = (n - n6) / 36;
                        var n5 = (n % 36);
                        n = (n - n5) / 36;
                        var n4 = (n % 36);
                        n = (n - n4) / 36;
                        var n3 = (n % 36);
                        n = (n - n3) / 36;
                        var n2 = (n % 36);
                        n = (n - n2) / 36;
                        var n1 = n;

                        // Validate...
                        Debug.Assert(0 <= n1 && n1 < 36);
                        Debug.Assert(0 <= n2 && n2 < 36);
                        Debug.Assert(0 <= n3 && n3 < 36);
                        Debug.Assert(0 <= n4 && n4 < 36);
                        Debug.Assert(0 <= n5 && n5 < 36);
                        Debug.Assert(0 <= n6 && n6 < 36);

                        // Output...
                        if (padding == 0)
                        {
                            // 6 chars
                            outputPtr[outputOffset] = _encodeTable[n1];
                            outputPtr[outputOffset + 1] = _encodeTable[n2];
                            outputPtr[outputOffset + 2] = _encodeTable[n3];
                            outputPtr[outputOffset + 3] = _encodeTable[n4];
                            outputPtr[outputOffset + 4] = _encodeTable[n5];
                            outputPtr[outputOffset + 5] = _encodeTable[n6];
                            outputOffset += OutputChars;
                        }
                        else
                        {
                            // Final; 1-6 chars
                            Debug.Assert(1 <= padding && padding <= 2);
                            outputPtr[outputOffset] = _encodeTable[n1];
                            if (padding < 5)
                                outputPtr[outputOffset++] = _encodeTable[n2];
                            if (padding < 4)
                                outputPtr[outputOffset++] = _encodeTable[n3];
                            if (padding < 3)
                                outputPtr[outputOffset++] = _encodeTable[n4];
                            if (padding < 2)
                                outputPtr[outputOffset++] = _encodeTable[n5];
                            if (padding < 1)
                                outputPtr[outputOffset++] = _encodeTable[n6];
                        }
                    }
                }
            }

            return output.TrimEnd(default(char));
        }

        protected override byte[] DecodeString(string input)
        {
            var outputLen = ArrayEncoder.GetLengthOfOutputBuffer(input.Length, InputBytes, OutputChars);
            var output = new byte[outputLen];
            var outputOffset = 0;
            var inputOffset = 0;
            var remaining = input.Length;

            while (remaining >= 1)
            {
                // 6 input chars
                var padding = 0;
                var in1 = GetNextByte(input, inputOffset++, ref remaining, ref padding);
                var in2 = GetNextByte(input, inputOffset++, ref remaining, ref padding);
                Debug.Assert(padding == 0);
                var in3 = GetNextByte(input, inputOffset++, ref remaining, ref padding);
                var in4 = GetNextByte(input, inputOffset++, ref remaining, ref padding);
                var in5 = GetNextByte(input, inputOffset++, ref remaining, ref padding);
                var in6 = GetNextByte(input, inputOffset++, ref remaining, ref padding);

                // 4 output bytes
                var n = (in1 * Power(36, 5))
                    + (in2 * Power(36, 4))
                    + (in3 * Power(36, 3))
                    + (in4 * Power(36, 2))
                    + (in5 * Power(36, 1))
                    + in6;
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

        private byte GetNextByte(string input, int inputOffset, ref int remaining, ref int padding)
        {
            if (inputOffset >= input.Length)
            {
                ++padding;
                return (36 - 1);
            }

            var b = (byte)(input[inputOffset] - '!');
            if (b < 36)
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

        protected override object EncodeStart() => new EncodingData();

        protected override void EncodeByte(byte b, Stream output, object context)
        {
            var data = context as EncodingData;

            ++data.offset;

            ++data.blockSize;
            data.blockData <<= 8;
            data.blockData |= b;

            if (data.blockSize < InputBytes)
                return;

            EncodeBlock(output, data);
        }

        protected override void EncodeEnd(Stream output, object context)
        {
            var data = context as EncodingData;

            if (data.blockSize == 0)
                return;

            Debug.Assert(1 <= data.blockSize && data.blockSize <= InputBytes);
            EncodeBlock(output, data);
        }

        private void EncodeBlock(Stream output, EncodingData data)
        {
            // Input...
            data.blockData <<= (8 * (InputBytes - data.blockSize));
            var n6 = (byte)(data.blockData % 36);
            data.blockData = (data.blockData - n6) / 36;
            var n5 = (byte)(data.blockData % 36);
            data.blockData = (data.blockData - n5) / 36;
            var n4 = (byte)(data.blockData % 36);
            data.blockData = (data.blockData - n4) / 36;
            var n3 = (byte)(data.blockData % 36);
            data.blockData = (data.blockData - n3) / 36;
            var n2 = (byte)(data.blockData % 36);
            data.blockData = (data.blockData - n2) / 36;
            var n1 = (byte)data.blockData;

            // Validate...
            Debug.Assert(0 <= n1 && n1 < 36);
            Debug.Assert(0 <= n2 && n2 < 36);
            Debug.Assert(0 <= n3 && n3 < 36);
            Debug.Assert(0 <= n4 && n4 < 36);
            Debug.Assert(0 <= n5 && n5 < 36);
            Debug.Assert(0 <= n6 && n6 < 36);

            // Output...
            if (data.blockSize == OutputChars)
            {
                // 6 chars
                output.WriteByte((byte)_encodeTable[n1]);
                output.WriteByte((byte)_encodeTable[n2]);
                output.WriteByte((byte)_encodeTable[n3]);
                output.WriteByte((byte)_encodeTable[n4]);
                output.WriteByte((byte)_encodeTable[n5]);
                output.WriteByte((byte)_encodeTable[n6]);
            }
            else
            {
                // Final; 1-6 chars
                output.WriteByte((byte)_encodeTable[n1]);
                if (data.blockSize >= 1)
                    output.WriteByte((byte)_encodeTable[n2]);
                if (data.blockSize >= 2)
                    output.WriteByte((byte)_encodeTable[n3]);
                if (data.blockSize >= 3)
                    output.WriteByte((byte)_encodeTable[n4]);
                if (data.blockSize >= 4)
                    output.WriteByte((byte)_encodeTable[n5]);
                if (data.blockSize >= 5)
                    output.WriteByte((byte)_encodeTable[n6]);
            }

            // Reset...
            data.blockSize = 0;
            data.blockData = 0;
        }

        protected override object DecodeStart() => new DecodingData();

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = context as DecodingData;

            ++data.offset;

            var b = _decodeTable[c];
            if (b == Tables.InvalidChar/* || (b != Padding && data.padding >= 1)*/)
                throw new BadCharacterException($"Bad character at offset {data.offset}");

            ++data.blockSize;
            data.blockData <<= 8;
            data.blockData |= b;

            if (data.blockSize != OutputChars)
                return;

            DecodeBlock(output, data);
        }

        protected override void DecodeEnd(Stream output, object context)
        {
            var data = context as DecodingData;

            if (data.blockSize == 0)
                return;

            /*var blockSize = (data.blockSize + data.padding);
            if (blockSize != OutputChars && !_optionalPadding)
                throw new BadLengthException($"Encoding has bad length: {data.offset}");*/

            DecodeBlock(output, data);
        }

        private void DecodeBlock(Stream output, DecodingData data)
        {
            Debug.Assert(1 <= data.blockSize && data.blockSize <= OutputChars);

            // Padding...
            data.blockData <<= (8 * (OutputChars - data.blockSize));

            // Inputs...
            var in1 = (byte)(data.blockData >> 25);
            var in2 = data.blockSize <= 1 ? 35 : (byte)(data.blockData >> 20);
            var in3 = data.blockSize <= 2 ? 35 : (byte)(data.blockData >> 15);
            var in4 = data.blockSize <= 3 ? 35 : (byte)(data.blockData >> 10);
            var in5 = data.blockSize <= 4 ? 35 : (byte)(data.blockData >> 5);
            var in6 = data.blockSize <= 5 ? 35 : (byte)data.blockData;

            // Validate...
            Debug.Assert(0 <= in1 && in1 < 36);
            Debug.Assert(0 <= in2 && in2 < 36);
            Debug.Assert(0 <= in3 && in3 < 36);
            Debug.Assert(0 <= in4 && in4 < 36);
            Debug.Assert(0 <= in5 && in5 < 36);
            Debug.Assert(0 <= in6 && in6 < 36);

            // Outputs...
            var n = (in1 * Power(36, 5))
                + (in2 * Power(36, 4))
                + (in3 * Power(36, 3))
                + (in4 * Power(36, 2))
                + (in5 * Power(36, 1))
                + in6;
            if (data.blockSize == OutputChars)
            {
                // 4 bytes
                output.WriteByte((byte)(n >> 24));
                output.WriteByte((byte)(n >> 16));
                output.WriteByte((byte)(n >> 8));
                output.WriteByte((byte)n);
            }
            else
            {
                // Final; 1-3 bytes
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
