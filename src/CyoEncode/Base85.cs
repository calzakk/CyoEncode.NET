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
using System.IO;
using System.Threading.Tasks;

namespace CyoEncode
{
    /// <summary>
    /// Base85/Ascii85 encode/decode functions
    /// </summary>
    public class Base85 : IBase85
    {
        // IBase85

        /// <summary>
        /// Number of bytes to allocate for the buffer when reading from a stream
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 1024; //1 MiB

        /// <summary>
        /// Output 'z' instead of '!!!!!' when encoding four zero-value bytes
        /// </summary>
        public bool FoldZero { get; set; } = true;

        // IEncoder

        /// <summary>
        /// Encode bytes to a Base85-encoded string
        /// </summary>
        /// <param name="input">Bytes to encode</param>
        /// <returns>Base85-encoded string</returns>
        public string Encode(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var impl = new Internal.Base85(BufferSize, FoldZero);
            return impl.Encode(input);
        }

        /// <summary>
        /// Encode bytes to a Base85-encoded string
        /// </summary>
        /// <param name="input">Bytes to encode</param>
        /// <param name="output">Base85-encoded string</param>
        public Task EncodeStreamAsync(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var impl = new Internal.Base85(BufferSize, FoldZero);
            return impl.EncodeAsync(input, output);
        }

        /// <summary>
        /// Decode the Base85-encoded string
        /// </summary>
        /// <param name="input">Base85-encoded string</param>
        /// <returns>Decoded bytes</returns>
        public byte[] Decode(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var impl = new Internal.Base85(BufferSize, FoldZero);
            return impl.Decode(input);
        }

        /// <summary>
        /// Decode the Base85-encoded string
        /// </summary>
        /// <param name="input">Base85-encoded string</param>
        /// <param name="output">Decoded bytes</param>
        public Task DecodeStreamAsync(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var impl = new Internal.Base85(BufferSize, FoldZero);
            return impl.DecodeAsync(input, output);
        }
    }
}
