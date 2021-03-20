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
using System.IO;
using System.Threading.Tasks;

namespace CyoEncode
{
    public class Base16 : IBase16
    {
        // IBase16

        /// <summary>
        /// Number of bytes to allocate for the buffer when reading from a stream
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 1024; //1 MiB

        // IEncoder

        /// <summary>
        /// Convert bytes to a Base16-encoded string
        /// </summary>
        /// <param name="input">Bytes to convert</param>
        /// <returns>Base16-encoded string</returns>
        public string Encode(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var impl = new Internal.Base16(BufferSize);
            return impl.Encode(input);
        }

        /// <summary>
        /// Convert bytes to a Base16-encoded string
        /// </summary>
        /// <param name="input">Bytes to convert</param>
        /// <param name="output">Base16-encoded string</param>
        public Task EncodeStreamAsync(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var impl = new Internal.Base16(BufferSize);
            return impl.EncodeAsync(input, output);
        }

        /// <summary>
        /// Decode the Base16-encoded string
        /// </summary>
        /// <param name="input">Base16-encoded string</param>
        /// <returns>Decoded bytes</returns>
        public byte[] Decode(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var impl = new Internal.Base16(BufferSize);
            return impl.Decode(input);
        }

        /// <summary>
        /// Decode the Base16-encoded string
        /// </summary>
        /// <param name="input">Base16-encoded string</param>
        /// <param name="output">Decoded bytes</param>
        public Task DecodeStreamAsync(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var impl = new Internal.Base16(BufferSize);
            return impl.DecodeAsync(input, output);
        }
    }
}
