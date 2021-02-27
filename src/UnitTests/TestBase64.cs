// TestBase64.cs - part of the CyoEncode.NET library
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
using System.Text;
using Xunit;

namespace UnitTests
{
    public class TestBase64
    {
        private readonly Base64 _base64 = new Base64();

        [Theory]
        [InlineData("", "")]
        [InlineData("f", "Zg==")]
        [InlineData("fo", "Zm8=")]
        [InlineData("foo", "Zm9v")]
        [InlineData("foob", "Zm9vYg==")]
        [InlineData("fooba", "Zm9vYmE=")]
        [InlineData("foobar", "Zm9vYmFy")]
        public void TestVectorsFromRFC4648(string original, string encoding)
        {
            var encoded = _base64.Encode(Encoding.ASCII.GetBytes(original));
            encoded.Should().Be(encoding);

            var decoded = Encoding.ASCII.GetString(_base64.Decode(encoding));
            decoded.Should().Be(original);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AA")]
        [InlineData("AAA")]
        public void BadLength(string input)
        {
            Action action = () => _base64.Decode(input);
            action.Should().Throw<BadLengthException>();
        }

        [Theory]
        [InlineData("@@@@")]
        public void BadCharacter(string input)
        {
            Action action = () => _base64.Decode(input);
            action.Should().Throw<BadCharacterException>();
        }

        [Theory]
        [InlineData("Zm8=Zm8=")]
        [InlineData("Zm=8")]
        [InlineData("Z=m8")]
        [InlineData("Z=m=")]
        public void BadPadding(string input)
        {
            Action action = () => _base64.Decode(input);
            action.Should().Throw<BadCharacterException>();
        }
    }
}
