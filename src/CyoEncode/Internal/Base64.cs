// Internal/Base64.cs - part of the CyoEncode.NET library
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
    internal class Base64 : Encoder
    {
        private const int InputBytes = 3;
        private const int OutputChars = 4;
        private const int Padding = 64;
        private readonly char[] _encodeTable;
        private readonly byte[] _decodeTable;
        private readonly int _bufferSize;
        private readonly bool _optionalPadding;

        public Base64(int bufferSize, bool optionalPadding)
        {
            var (encode, decode) = Tables.Init("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=");
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
                        var n1 = (input[offset] & 0xfc) >> 2;
                        var n2 = (input[offset] & 0x03) << 4;
                        var n3 = Padding;
                        var n4 = Padding;
                        if (blockSize >= 2)
                        {
                            n2 |= (input[offset + 1] & 0xf0) >> 4;
                            n3 = (input[offset + 1] & 0x0f) << 2;
                            if (blockSize >= 3)
                            {
                                n3 |= (input[offset + 2] & 0xc0) >> 6;
                                n4 = (input[offset + 2] & 0x3f);
                            }
                        }
                        offset += blockSize;
                        remaining -= blockSize;

                        // Validate...
                        Debug.Assert(0 <= n1 && n1 <= Padding);
                        Debug.Assert(0 <= n2 && n2 <= Padding);
                        Debug.Assert(0 <= n3 && n3 <= Padding);
                        Debug.Assert(0 <= n4 && n4 <= Padding);

                        // Output...
                        outputPtr[outputOffset] = _encodeTable[n1];
                        outputPtr[outputOffset + 1] = _encodeTable[n2];
                        outputPtr[outputOffset + 2] = _encodeTable[n3];
                        outputPtr[outputOffset + 3] = _encodeTable[n4];
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

                // Validate padding...
                ArrayEncoder.EnsureNotPadding(in1, Padding, startOffset);
                ArrayEncoder.EnsureNotPadding(in2, Padding, startOffset + 1);
                if (remaining >= 1)
                {
                    ArrayEncoder.EnsureNotPadding(in3, Padding, startOffset + 2);
                    ArrayEncoder.EnsureNotPadding(in4, Padding, startOffset + 3);
                }
                else
                {
                    var padding = false;
                    ArrayEncoder.ValidatePadding(in3, Padding, startOffset + 2, ref padding);
                    ArrayEncoder.ValidatePadding(in4, Padding, startOffset + 3, ref padding);
                }

                // Outputs...
                output[outputOffset] = (byte)(((in1 & 0x3f) << 2) | ((in2 & 0x30) >> 4));
                output[outputOffset + 1] = (byte)(((in2 & 0x0f) << 4) | ((in3 & 0x3c) >> 2));
                output[outputOffset + 2] = (byte)(((in3 & 0x03) << 6) | (in4 & 0x3f));
                outputOffset += InputBytes;

                // Padding...
                if (in4 == Padding)
                {
                    --outputOffset;
                    if (in3 == Padding)
                        --outputOffset;
                }
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

            EncodeBlock(output, data);
        }

        private void EncodeBlock(Stream output, EncodingData data)
        {
            Debug.Assert(data.blockSize <= InputBytes);

            // Padding...
            data.blockData <<= (8 * (InputBytes - data.blockSize));

            // Input...
            var in1 = (byte)(data.blockData >> 16);
            var in2 = (byte)(data.blockData >> 8);
            var in3 = (byte)data.blockData;
            var n1 = (byte)((in1 & 0xfc) >> 2);
            var n2 = (byte)((in1 & 0x03) << 4);
            var n3 = Padding;
            var n4 = Padding;
            if (data.blockSize >= 2)
            {
                n2 |= (byte)((in2 & 0xf0) >> 4);
                n3 = (byte)((in2 & 0x0f) << 2);
                if (data.blockSize == 3)
                {
                    n3 |= (byte)((in3 & 0xc0) >> 6);
                    n4 = (byte)(in3 & 0x3f);
                }
            }

            // Validate...
            Debug.Assert(0 <= n1 && n1 <= Padding);
            Debug.Assert(0 <= n2 && n2 <= Padding);
            Debug.Assert(0 <= n3 && n3 <= Padding);
            Debug.Assert(0 <= n4 && n4 <= Padding);

            // Output...
            output.WriteByte((byte)_encodeTable[n1]);
            output.WriteByte((byte)_encodeTable[n2]);
            output.WriteByte((byte)_encodeTable[n3]);
            output.WriteByte((byte)_encodeTable[n4]);

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
        }

        protected override object DecodeStart() => new DecodingData();

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = context as DecodingData;

            ++data.offset;

            byte b = _decodeTable[c];
            if (b == Tables.InvalidChar || (b != Padding && data.padding >= 1))
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

            DecodeBlock(output, data);
        }

        protected override void DecodeEnd(Stream output, object context)
        {
            var data = context as DecodingData;

            if (data.blockSize == 0)
                return;

            var blockSize = (data.blockSize + data.padding);
            if (blockSize != OutputChars && !_optionalPadding)
                throw new BadLengthException($"Encoding has bad length: {data.offset}");

            DecodeBlock(output, data);
        }

        private void DecodeBlock(Stream output, DecodingData data)
        {
            Debug.Assert(2 <= data.blockSize && data.blockSize <= OutputChars);

            // Padding...
            data.blockData <<= (8 * (OutputChars - data.blockSize));

            // Inputs...
            var in1 = (byte)(data.blockData >> 24);
            var in2 = (byte)(data.blockData >> 16);
            var in3 = (byte)(data.blockData >> 8);
            var in4 = (byte)data.blockData;

            // Validate...
            Debug.Assert(0 <= in1 && in1 < Padding); //cannot be padding
            Debug.Assert(0 <= in2 && in2 < Padding); //cannot be padding
            Debug.Assert(0 <= in3 && in3 <= Padding);
            Debug.Assert(0 <= in4 && in4 <= Padding);

            // Outputs...
            if (data.blockSize == OutputChars)
            {
                output.WriteByte((byte)(((in1 & 0x3f) << 2) | ((in2 & 0x30) >> 4)));
                output.WriteByte((byte)(((in2 & 0x0f) << 4) | ((in3 & 0x3c) >> 2)));
                output.WriteByte((byte)(((in3 & 0x03) << 6) | (in4 & 0x3f)));
            }
            else
            {
                output.WriteByte((byte)(((in1 & 0x3f) << 2) | ((in2 & 0x30) >> 4)));
                if (data.blockSize == 3)
                    output.WriteByte((byte)(((in2 & 0x0f) << 4) | ((in3 & 0x3c) >> 2)));
            }

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
            data.padding = 0;
        }
    }
}
