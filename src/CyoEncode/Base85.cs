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

                for (var i = 0; i < 5; ++i)
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
