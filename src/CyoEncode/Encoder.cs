// Encoder.cs - part of the CyoEncode.NET library
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
    public abstract class Encoder : IEncoder
    {
        public const int MinBufferSize = 1;
        public const int DefaultBufferSize = 1024 * 1024; //1 MiB

        private int _bufferSize = DefaultBufferSize;

        public int BufferSize
        {
            get => _bufferSize;
            set => _bufferSize = (value >= MinBufferSize) ? value : throw new Exception($"Insufficient BufferSize: {BufferSize}");
        }

        // Spans

        public string Encode(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == 0)
                return string.Empty;

            return EncodeBytes(input);
        }

        public byte[] Decode(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == 0)
                return Array.Empty<byte>();

            return DecodeString(input);
        }

        // Streams

        public void Encode(Stream input, Stream output)
            => EncodeAsync(input, output).Wait();

        public void Decode(Stream input, Stream output)
            => DecodeAsync(input, output).Wait();

        public async Task EncodeAsync(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var buffer = new byte[BufferSize];
            var data = CreateEncodingData();

            while (true)
            {
                var length = await input.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (length == 0)
                    break;

                for (var i = 0; i < length; ++i)
                    EncodeByte(buffer[i], output, data);
            }

            EncodeEnd(output, data);
        }

        public async Task DecodeAsync(Stream input, Stream output)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var buffer = new byte[BufferSize];
            var data = CreateDecodingData();

            while (true)
            {
                var length = await input.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (length == 0)
                    break;

                for (var i = 0; i < length; ++i)
                    DecodeChar((char)buffer[i], output, data);
            }

            DecodeEnd(output, data);
        }

        //

        protected const byte Invalid = 0xff;

        protected static void InitEncodeTable(char[] encodeTable, string charset)
        {
            for (int i = 0; i < charset.Length; ++i)
                encodeTable[i] = charset[i];
        }

        protected static void InitDecodeTable(byte[] decodeTable, string charset)
        {
            for (int i = 0; i < decodeTable.Length; ++i)
                decodeTable[i] = Invalid;

            for (int i = 0; i < charset.Length; ++i)
                decodeTable[charset[i]] = (byte)i;
        }

        // Spans

        protected abstract string EncodeBytes(byte[] input);

        protected abstract byte[] DecodeString(string input);

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

        protected void ValidatePadding(byte value, byte padding, int offset, ref bool expectedPadding)
        {
            if (value == padding)
                expectedPadding = true;
            else if (expectedPadding)
                throw new BadCharacterException($"Bad character at offset {offset}");
        }

        // Streams

        protected abstract object CreateEncodingData();

        protected abstract object CreateDecodingData();

        protected abstract void EncodeByte(byte b, Stream output, object context);

        protected abstract void EncodeEnd(Stream output, object context);

        protected abstract void DecodeChar(char c, Stream output, object context);

        protected abstract void DecodeEnd(Stream output, object context);
    }
}
