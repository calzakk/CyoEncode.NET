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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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

            if (data.blockSize == InputBytes)
            {
                EncodeBlock(data, output, false);
                data.blockSize = 0;
                data.blockData = 0;
            }
        }

        protected override void EncodeEnd(Stream output, object context)
        {
            var data = (EncodingData)context;

            if (data.blockSize >= 1)
            {
                EncodeBlock(data, output, true);
            }
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
                return;
            }
            var padding = (InputBytes - data.blockSize);
            Debug.Assert(finalBlock
                ? 0 <= padding && padding <= InputBytes - 1
                : padding == 0);
            var n = (uint)data.blockData;
            n <<= (8 * padding);
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
                output.WriteByte((byte)(n1 + '!'));
                output.WriteByte((byte)(n2 + '!'));
                output.WriteByte((byte)(n3 + '!'));
                output.WriteByte((byte)(n4 + '!'));
                output.WriteByte((byte)(n5 + '!'));
            }
            else
            {
                // Final; 1-4 outputs
                Debug.Assert(1 <= padding && padding <= 4);
                output.WriteByte((byte)(n1 + '!'));
                if (padding < 4)
                    output.WriteByte((byte)(n2 + '!'));
                if (padding < 3)
                    output.WriteByte((byte)(n3 + '!'));
                if (padding < 2)
                    output.WriteByte((byte)(n4 + '!'));
                if (padding < 1)
                    output.WriteByte((byte)(n5 + '!'));
            }
        }

        protected override void DecodeChar(char c, Stream output, object context)
        {
            var data = (DecodingData)context;

            //TODO
        }

        protected override void DecodeEnd(Stream output, object context)
        {
            var data = (DecodingData)context;

            if (data.blockSize == 0)
                return;

            //TODO
        }
    }
}
