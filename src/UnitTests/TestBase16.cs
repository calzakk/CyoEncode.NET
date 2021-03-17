// TestBase16.cs - part of the CyoEncode.NET library
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

using CyoEncode;
using FluentAssertions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests
{
    public class TestBase16
    {
        private readonly Base16 _base16 = new Base16();

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "66")]
        [InlineData("fo", "666F")]
        [InlineData("foo", "666F6F")]
        [InlineData("foob", "666F6F62")]
        [InlineData("fooba", "666F6F6261")]
        [InlineData("foobar", "666F6F626172")]
        public void TestVectorsFromRFC4648_should_encode_successfully(string original, string encoding)
        {
            var encoded = _base16.Encode(Encoding.ASCII.GetBytes(original));
            encoded.Should().Be(encoding);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "66")]
        [InlineData("fo", "666F")]
        [InlineData("foo", "666F6F")]
        [InlineData("foob", "666F6F62")]
        [InlineData("fooba", "666F6F6261")]
        [InlineData("foobar", "666F6F626172")]
        public void TestVectorsFromRFC4648_should_decode_successfully(string original, string encoding)
        {
            var decoded = Encoding.ASCII.GetString(_base16.Decode(encoding));
            decoded.Should().Be(original);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "66")]
        [InlineData("fo", "666F")]
        [InlineData("foo", "666F6F")]
        [InlineData("foob", "666F6F62")]
        [InlineData("fooba", "666F6F6261")]
        [InlineData("foobar", "666F6F626172")]
        public void TestVectorsFromRFC4648_should_encode_successfully_using_streams(string original, string encoding)
        {
            using var input = new MemoryStream(Encoding.ASCII.GetBytes(original));
            using var output = new MemoryStream();

            _base16.Encode(input, output);

            output.Flush();
            var outputText = Encoding.ASCII.GetString(output.ToArray());
            outputText.Should().Be(encoding);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "66")]
        [InlineData("fo", "666F")]
        [InlineData("foo", "666F6F")]
        [InlineData("foob", "666F6F62")]
        [InlineData("fooba", "666F6F6261")]
        [InlineData("foobar", "666F6F626172")]
        public void TestVectorsFromRFC4648_should_decode_successfully_using_streams(string original, string encoding)
        {
            using var input = new MemoryStream(Encoding.ASCII.GetBytes(encoding));
            using var output = new MemoryStream();

            _base16.Decode(input, output);

            output.Flush();
            var outputText = Encoding.ASCII.GetString(output.ToArray());
            outputText.Should().Be(original);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "66")]
        [InlineData("fo", "666F")]
        [InlineData("foo", "666F6F")]
        [InlineData("foob", "666F6F62")]
        [InlineData("fooba", "666F6F6261")]
        [InlineData("foobar", "666F6F626172")]
        public async Task TestVectorsFromRFC4648_should_encode_successfully_using_async_streams(string original, string encoding)
        {
            using var input = new MemoryStream(Encoding.ASCII.GetBytes(original));
            using var output = new MemoryStream();

            await _base16.EncodeAsync(input, output);

            output.Flush();
            var outputText = Encoding.ASCII.GetString(output.ToArray());
            outputText.Should().Be(encoding);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "66")]
        [InlineData("fo", "666F")]
        [InlineData("foo", "666F6F")]
        [InlineData("foob", "666F6F62")]
        [InlineData("fooba", "666F6F6261")]
        [InlineData("foobar", "666F6F626172")]
        public async Task TestVectorsFromRFC4648_should_decode_successfully_using_async_streams(string original, string encoding)
        {
            using var input = new MemoryStream(Encoding.ASCII.GetBytes(encoding));
            using var output = new MemoryStream();

            await _base16.DecodeAsync(input, output);

            output.Flush();
            var outputText = Encoding.ASCII.GetString(output.ToArray());
            outputText.Should().Be(original);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AAA")]
        [InlineData("AAAAA")]
        public void BadLength(string input)
        {
            Action action = () => _base16.Decode(input);
            action.Should().Throw<BadLengthException>().WithMessage($"Encoding has bad length: {input.Length}");
        }

        [Theory]
        [InlineData("GG")]
        [InlineData("ZZ")]
        public void BadCharacter(string input)
        {
            Action action = () => _base16.Decode(input);
            action.Should().Throw<BadCharacterException>().WithMessage("Bad character at offset 0");
        }
    }
}
