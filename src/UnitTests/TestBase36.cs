// TestBase36.cs - part of the CyoEncode.NET library
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
    public class TestBase36
    {
        private readonly IBase36 _base36 = new Base36();

        [Fact]
        public void Test1()
        {
            var encoded = _base36.Encode(new byte[]{ 127, 255, 255, 255 });
            encoded.Should().Be("ZIK0ZJ");
        }

        /*[Theory]
        [InlineData("", "")]
        [InlineData("f", "MY======")]
        [InlineData("fo", "MZXQ====")]
        [InlineData("foo", "MZXW6===")]
        [InlineData("foob", "MZXW6YQ=")]
        [InlineData("fooba", "MZXW6YTB")]
        [InlineData("foobar", "MZXW6YTBOI======")]
        public void TestVectorsFromRFC4648_should_encode_successfully(string original, string encoding)
        {
            var encoded = _base36.Encode(Encoding.ASCII.GetBytes(original));
            encoded.Should().Be(encoding);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "MY======")]
        [InlineData("fo", "MZXQ====")]
        [InlineData("foo", "MZXW6===")]
        [InlineData("foob", "MZXW6YQ=")]
        [InlineData("fooba", "MZXW6YTB")]
        [InlineData("foobar", "MZXW6YTBOI======")]
        public void TestVectorsFromRFC4648_should_decode_successfully(string original, string encoding)
        {
            var decoded = Encoding.ASCII.GetString(_base36.Decode(encoding));
            decoded.Should().Be(original);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "MY")]
        [InlineData("fo", "MZXQ")]
        [InlineData("foo", "MZXW6")]
        [InlineData("foob", "MZXW6YQ")]
        [InlineData("fooba", "MZXW6YTB")]
        [InlineData("foobar", "MZXW6YTBOI")]
        public void TestVectorsFromRFC4648_should_decode_successfully_with_optional_padding(string original, string encoding)
        {
            _base36.OptionalPadding = true;
            var decoded = Encoding.ASCII.GetString(_base36.Decode(encoding));
            decoded.Should().Be(original);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "MY======")]
        [InlineData("fo", "MZXQ====")]
        [InlineData("foo", "MZXW6===")]
        [InlineData("foob", "MZXW6YQ=")]
        [InlineData("fooba", "MZXW6YTB")]
        [InlineData("foobar", "MZXW6YTBOI======")]
        public async Task TestVectorsFromRFC4648_should_encode_successfully_using_streams(string original, string encoding)
        {
            using var input = new MemoryStream(Encoding.ASCII.GetBytes(original));
            using var output = new MemoryStream();

            await _base36.EncodeStreamAsync(input, output);

            output.Flush();
            var outputText = Encoding.ASCII.GetString(output.ToArray());
            outputText.Should().Be(encoding);
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "MY======")]
        [InlineData("fo", "MZXQ====")]
        [InlineData("foo", "MZXW6===")]
        [InlineData("foob", "MZXW6YQ=")]
        [InlineData("fooba", "MZXW6YTB")]
        [InlineData("foobar", "MZXW6YTBOI======")]
        public async Task TestVectorsFromRFC4648_should_decode_successfully_using_streams(string original, string encoding)
        {
            using var input = new MemoryStream(Encoding.ASCII.GetBytes(encoding));
            using var output = new MemoryStream();

            await _base36.DecodeStreamAsync(input, output);
32
            output.Flush();
            var outputText = Encoding.ASCII.GetString(output.ToArray());
            outputText.Should().Be(original);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AA")]
        [InlineData("AAA")]
        [InlineData("AAAA")]
        [InlineData("AAAAA")]
        [InlineData("AAAAAA")]
        [InlineData("AAAAAAA")]
        public void BadLength(string input)
        {
            Action action = () => _base36.Decode(input);
            action.Should().Throw<BadLengthException>().WithMessage($"Encoding has bad length: {input.Length}");
        }

        [Theory]
        [InlineData("@@@@@@@@")]
        public void BadCharacter(string input)
        {
            Action action = () => _base36.Decode(input);
            action.Should().Throw<BadCharacterException>().WithMessage("Bad character at offset 0");
        }

        [Theory]
        [InlineData("MZXW6YQ=MZXW6YQ=")]
        public void BadPaddingInFirstBlock(string input)
        {
            Action action = () => _base36.Decode(input);
            action.Should().Throw<BadCharacterException>().WithMessage($"Bad character at offset {input.IndexOf('=')}");
        }

        [Theory]
        [InlineData("M=ZXW6YQ")]
        [InlineData("=MZXW6YQ")]
        [InlineData("M=Z=====")]
        [InlineData("=M======")]
        public void BadPaddingAtStartOfBlock(string input)
        {
            Action action = () => _base36.Decode(input);
            action.Should().Throw<BadCharacterException>().WithMessage($"Bad character at offset {input.IndexOf('=')}");
        }

        [Theory]
        [InlineData("MZXW6Y=Q")]
        [InlineData("MZXW6=YQ")]
        [InlineData("MZXW=6YQ")]
        [InlineData("MZX=W6YQ")]
        [InlineData("MZ=XW6YQ")]
        [InlineData("MZXW6=Y=")]
        [InlineData("MZXW=6==")]
        [InlineData("MZX=W===")]
        [InlineData("MZ=X====")]
        public void BadCharacterAfterPadding(string input)
        {
            Action action = () => _base36.Decode(input);
            action.Should().Throw<BadCharacterException>().WithMessage($"Bad character at offset {input.IndexOf('=') + 1}");
            //+1 to get the offset of the character immediately after the first =
        }*/
    }
}
