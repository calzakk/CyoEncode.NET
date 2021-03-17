// Base16.cs - part of the CyoEncode.NET library
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
    public sealed class Base16 : Encoder
    {
        private const int InputBytes = 1;
        private const int OutputChars = 2;
        private const int MaxValue = 15;
        private static readonly char[] EncodeTable = new char[16];
        private static readonly byte[] DecodeTable = new byte[128];

        static Base16()
        {
            const string charset = "0123456789ABCDEF";
            InitEncodeTable(EncodeTable, charset);
            InitDecodeTable(DecodeTable, charset);
        }

        // Spans

        protected override string EncodeBytes(byte[] input)
        {
            var outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new string(default, outputLen);
            var outputOffset = 0;

            unsafe
            {
                fixed (char* outputPtr = output)
                {
                    foreach (byte b in input)
                    {
                        // Input...
                        var n1 = ((b & 0xf0) >> 4);
                        var n2 = (b & 0x0f);

                        // Validate...
                        Debug.Assert(0 <= n1 && n1 <= MaxValue);
                        Debug.Assert(0 <= n2 && n2 <= MaxValue);

                        // Output...
                        outputPtr[outputOffset] = EncodeTable[n1];
                        outputPtr[outputOffset + 1] = EncodeTable[n2];
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

            var outputLen = CalcOutputLen(input.Length, InputBytes, OutputChars);
            var output = new byte[outputLen];
            var outputOffset = 0;
            var inputOffset = 0;
            var remaining = input.Length;

            while (remaining != 0)
            {
                // Inputs...
                var in1 = NextChar(input, ref inputOffset, ref remaining);
                var in2 = NextChar(input, ref inputOffset, ref remaining);

                // Outputs...
                output[outputOffset] = (byte)((in1 << 4) | in2);
                outputOffset += InputBytes;
            }

            Debug.Assert(outputOffset == outputLen);

            return output;
        }

        private byte NextChar(string input, ref int inputOffset, ref int remaining)
        {
            var b = DecodeTable[input[inputOffset]];
            if (b == Invalid)
                throw new BadCharacterException($"Bad character at offset {inputOffset}");

            ++inputOffset;
            --remaining;
            return b;
        }

        // Streams

        private class DecodingData
        {
            public int offset = 0;
            public int blockSize = 0;
            public byte blockData = 0;
        }

        protected override object CreateEncodingData() => null;

        protected override object CreateDecodingData() => new DecodingData();

        protected override void EncodeByte(byte b, Stream output, object context)
        {
            var n1 = ((b & 0xf0) >> 4);
            var n2 = (b & 0x0f);

            Debug.Assert(0 <= n1 && n1 <= MaxValue);
            Debug.Assert(0 <= n2 && n2 <= MaxValue);

            output.WriteByte((byte)EncodeTable[n1]);
            output.WriteByte((byte)EncodeTable[n2]);
        }

        protected override void EncodeEnd(Stream output, object context)
        {
        }

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = (DecodingData)context;

            ++data.offset;

            byte b = DecodeTable[c];
            if (b == Invalid)
                throw new BadCharacterException($"Bad character at offset {data.offset}");

            ++data.blockSize;
            data.blockData <<= 4;
            data.blockData |= b;

            if (data.blockSize == OutputChars)
            {
                DecodeBlock(data, output);

                data.blockSize = 0;
                data.blockData = 0;
            }
        }

        protected override void DecodeEnd(Stream output, object context)
        {
            var data = (DecodingData)context;

            if (data.blockSize == 0)
                return;

            if (data.blockSize != OutputChars)
                throw new BadLengthException($"Encoding has bad length: {data.offset}");

            DecodeBlock(data, output);
        }

        private void DecodeBlock(DecodingData data, Stream output)
        {
            output.WriteByte(data.blockData);
        }
    }
}
