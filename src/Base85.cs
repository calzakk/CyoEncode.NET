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

#define FOLD_ZERO //output 'z' instead of '!!!!!'
//#define FOLD_SPACES //output 'y' instead of 4 spaces

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CyoEncode
{
    public class Base85 : EncodeBase
    {
        public override string Encode(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new StringBuilder(outputLen);
            int offset = 0;
            int remaining = input.Length;

            while (remaining >= 1)
            {
                // Input...
                int n = 0;
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
#if FOLD_ZERO
                if (n == 0)
                {
                    output.Append('z');
                    continue;
                }
#endif
                int n5 = (n % 85);
                n = (n - n5) / 85;
                int n4 = (n % 85);
                n = (n - n4) / 85;
                int n3 = (n % 85);
                n = (n - n3) / 85;
                int n2 = (n % 85);
                n = (n - n2) / 85;
                int n1 = n;

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
                    // 1-4 outputs
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
            throw new NotImplementedException();
        }

        #region Implementation

        private const int InputBytes = 4;
        private const int OutputChars = 5;

        #endregion
    }
}
