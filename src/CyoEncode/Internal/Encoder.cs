// Internal/Encoder.cs - part of the CyoEncode.NET library
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

using System.IO;
using System.Threading.Tasks;

namespace CyoEncode.Internal
{
    internal abstract class Encoder
    {
        public string Encode(byte[] input)
        {
            return ArrayEncoder.Encode(input, EncodeBytes);
        }

        public Task EncodeAsync(Stream input, Stream output)
        {
            return StreamEncoder.EncodeAsync(input, output, GetBufferSize(), EncodeStart, EncodeByte, EncodeEnd);
        }

        public byte[] Decode(string input)
        {
            return ArrayEncoder.Decode(input, DecodeString);
        }

        public Task DecodeAsync(Stream input, Stream output)
        {
            return StreamEncoder.DecodeAsync(input, output, GetBufferSize(), DecodeStart, DecodeChar, DecodeEnd);
        }

        protected abstract int GetBufferSize();

        protected abstract string EncodeBytes(byte[] input);

        protected abstract byte[] DecodeString(string input);

        protected abstract object EncodeStart();

        protected abstract void EncodeByte(byte b, Stream output, object context);

        protected abstract void EncodeEnd(Stream output, object context);

        protected abstract object DecodeStart();

        protected abstract void DecodeChar(char c, Stream output, object context);

        protected abstract void DecodeEnd(Stream output, object context);
    }
}
