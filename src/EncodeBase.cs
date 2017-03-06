// EncodeBase.cs - part of the CyoEncode.NET library
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
using System.Linq;
using System.Text;

namespace CyoEncode
{
    public interface IEncode
    {
        string Encode(byte[] input);
    }

    public interface IDecode
    {
        byte[] Decode(string input);
    }

    public class BadLengthException : Exception
    {
        public BadLengthException(string msg)
            : base(msg)
        {
        }
    };

    public class BadCharacterException : Exception
    {
        public BadCharacterException(string msg)
            : base(msg)
        {
        }
    };

    public abstract class EncodeBase : IEncode, IDecode
    {
        public abstract byte[] Decode(string input);

        public abstract string Encode(byte[] input);

        protected static void InitDecodeTable(byte[] decodeTable, string byteToChar)
        {
            for (int i = 0; i < decodeTable.Length; ++i)
                decodeTable[i] = 0xff;

            for (int i = 0; i < byteToChar.Length; ++i)
                decodeTable[byteToChar[i]] = (byte)i;
        }

        protected void ValidateEncoding(string input, int outputChars, string byteToChar, bool allowPadding)
        {
            if ((input.Length % outputChars) != 0)
                throw new BadLengthException($"Encoding has bad length: {input.Length}");

            if (allowPadding)
            {
                input = input.TrimEnd('=');
                byteToChar = byteToChar.TrimEnd('=');
            }

            for (int i = 0; i < input.Length; ++i)
            {
                char ch = input[i];
                if (byteToChar.IndexOf(ch) < 0)
                    throw new BadCharacterException($"Bad character at offset {i}");
            }
        }

        protected int CalcOutputLen(int encodedLength, int inputBytes, int outputChars)
        {
            return (((encodedLength + outputChars - 1) / outputChars) * inputBytes);
        }

        protected void EnsurePadding(byte value, byte padding, int offset)
        {
            if (value != padding)
                throw new BadCharacterException($"Bad character at offset {offset}");
        }

        protected void EnsureNotPadding(byte value, byte padding, int offset)
        {
            if (value == padding)
                throw new BadCharacterException($"Bad character at offset {offset}");
        }
    }
}
