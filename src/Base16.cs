// Base16.cs - part of the CyoEncode.NET library
//
// MIT License
//
// Copyright(c) 2017 Graham Bull
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
using System.Text;

namespace CyoEncode
{
    public sealed class Base16 : EncodeBase
    {
        public override string Encode(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new StringBuilder(outputLen);

            foreach (byte b in input)
            {
                // Input...
                byte n1 = (byte)((b & 0xf0) >> 4);
                byte n2 = (byte)(b & 0x0f);

                // Validate...
                Debug.Assert(0 <= n1 && n1 <= MaxValue);
                Debug.Assert(0 <= n2 && n2 <= MaxValue);

                // Output...
                output.Append(ByteToChar[n1]);
                output.Append(ByteToChar[n2]);
            }

            return output.ToString();
        }

        public override byte[] Decode(string input)
        {
            ValidateEncoding(input, OutputChars, ByteToChar, false);

            int maxOutputLen = CalcOutputLen(input.Length, InputBytes, OutputChars);
            var output = new List<byte>(maxOutputLen);
            int outputLen = 0;
            int inputOffset = 0;
            int remaining = input.Length;

            while (remaining != 0)
            {
                // Inputs...
                byte in1 = DecodeTable[input[inputOffset++]];
                byte in2 = DecodeTable[input[inputOffset++]];
                remaining -= OutputChars;

                // Outputs...
                output.Add((byte)((in1 << 4) | in2));
                outputLen += InputBytes;
            }

            Debug.Assert(outputLen == maxOutputLen);

            return output.ToArray();
        }

        #region Implementation

        private const int InputBytes = 1;
        private const int OutputChars = 2;
        private const byte MaxValue = 15;
        private const string ByteToChar = "0123456789ABCDEF";
        private static readonly byte[] DecodeTable = new byte[128];

        static Base16()
        {
            InitDecodeTable(DecodeTable, ByteToChar);
        }

        #endregion
    }
}
