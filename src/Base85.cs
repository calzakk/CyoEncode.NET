// Base85.cs - part of the CyoEncode.NET library
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
    public class Base85 : EncodeBase
    {
        /// <summary>
        /// Output 'z' instead of '!!!!!'.
        /// </summary>
        public bool FoldZero { get; set; } = true;

        public override string Encode(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new StringBuilder(outputLen);
            int offset = 0;
            int remaining = input.Length;

            while (remaining >= 1)
            {
                // Input...
                uint n = 0;
                int padding = 0;
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
                    output.Append('z');
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
                    output.Append((char)(n1 + '!'));
                    output.Append((char)(n2 + '!'));
                    output.Append((char)(n3 + '!'));
                    output.Append((char)(n4 + '!'));
                    output.Append((char)(n5 + '!'));
                }
                else
                {
                    // Final; 1-4 outputs
                    Debug.Assert(1 <= padding && padding <= 4);
                    output.Append((char)(n1 + '!'));
                    if (padding < 4)
                        output.Append((char)(n2 + '!'));
                    if (padding < 3)
                        output.Append((char)(n3 + '!'));
                    if (padding < 2)
                        output.Append((char)(n4 + '!'));
                    if (padding < 1)
                        output.Append((char)(n5 + '!'));
                }
            }

            return output.ToString();
        }

        public override byte[] Decode(string input)
        {
            int maxOutputLen = CalcOutputLen(input.Length, InputBytes, OutputChars);
            var output = new List<byte>(maxOutputLen);
            int inputOffset = 0;
            int remaining = input.Length;

            while (remaining >= 1)
            {
                if (input[inputOffset] == 'z')
                {
                    if (!FoldZero)
                        throw new BadCharacterException($"Bad character at offset {inputOffset}");
                    ++inputOffset;
                    output.Add(0);
                    --remaining;
                    continue;
                }

                // 5 inputs
                int padding = 0;
                byte in1 = NextByte(input, inputOffset++, ref remaining, ref padding);
                byte in2 = NextByte(input, inputOffset++, ref remaining, ref padding);
                Debug.Assert(padding == 0);
                byte in3 = NextByte(input, inputOffset++, ref remaining, ref padding);
                byte in4 = NextByte(input, inputOffset++, ref remaining, ref padding);
                byte in5 = NextByte(input, inputOffset++, ref remaining, ref padding);

                // Output
                uint n = (in1 * Power(85, 4))
                    + (in2 * Power(85, 3))
                    + (in3 * Power(85, 2))
                    + (in4 * Power(85, 1))
                    + in5;
                output.Add((byte)(n >> 24));
                if (padding <= 2)
                {
                    output.Add((byte)(n >> 16));
                    if (padding <= 1)
                    {
                        output.Add((byte)(n >> 8));
                        if (padding == 0)
                        {
                            output.Add((byte)n);
                        }
                    }
                }
            }

            return output.ToArray();
        }

        #region Implementation

        private const int InputBytes = 4;
        private const int OutputChars = 5;

        private byte NextByte(string input, int inputOffset, ref int remaining, ref int padding)
        {
            if (inputOffset >= input.Length)
            {
                ++padding;
                return (85 - 1);
            }

            byte b = (byte)(input[inputOffset] - '!');
            if (b < 85)
            {
                --remaining;
                return b;
            }

            throw new BadCharacterException($"Bad character at offset {inputOffset}");
        }

        private uint Power(byte num, int count)
        {
            uint total = 1;
            for (int i = 0; i < count; ++i)
                total *= 85;
            return total;
        }

        #endregion
    }
}
