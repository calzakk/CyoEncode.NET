// Base64.cs - part of the CyoEncode.NET library
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

using CyoEncode.Exceptions;
using System.Diagnostics;
using System.IO;

namespace CyoEncode
{
    public sealed class Base64 : Encoder
    {
        public bool OptionalPadding { get; set; } = false;

        private const int InputBytes = 3;
        private const int OutputChars = 4;
        private const byte Padding = 64;
        private static readonly byte[] EncodeTable = new byte[65];
        private static readonly byte[] DecodeTable = new byte[128];

        static Base64()
        {
            const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
            InitEncodeTable(EncodeTable, charset);
            InitDecodeTable(DecodeTable, charset);
        }

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
            var padding = (InputBytes - data.blockSize);
            data.blockData <<= (8 * padding);

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
            output.WriteByte(EncodeTable[n1]);
            output.WriteByte(EncodeTable[n2]);
            output.WriteByte(EncodeTable[n3]);
            output.WriteByte(EncodeTable[n4]);

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
        }

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = (DecodingData)context;

            byte b = DecodeTable[c];
            if (b == Invalid || (b != Padding && data.padding >= 1))
                throw new BadCharacterException($"Bad character at offset {data.offset}");

            ++data.offset;

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
            Debug.Assert(data.blockSize <= OutputChars);

            // Padding...
            var padding = (OutputChars - data.blockSize);
            data.blockData <<= (8 * padding);

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
            output.WriteByte((byte)(((in1 & 0x3f) << 2) | ((in2 & 0x30) >> 4)));
            if (data.blockSize >= 3)
            {
                output.WriteByte((byte)(((in2 & 0x0f) << 4) | ((in3 & 0x3c) >> 2)));
                if (data.blockSize == 4)
                {
                    output.WriteByte((byte)(((in3 & 0x03) << 6) | (in4 & 0x3f)));
                }
            }

            // Reset block...
            data.blockSize = 0;
            data.blockData = 0;
            data.padding = 0;
        }
    }
}
