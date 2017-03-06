// TestBase16.cs - part of the CyoEncode.NET library
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

using CyoEncode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class TestBase16
    {
        private Base16 base16 = new Base16();

        [TestMethod]
        public void TestVectorsFromRFC4648()
        {
            // Encoding...
            Assert.AreEqual("", base16.Encode(Encoding.ASCII.GetBytes("")));
            Assert.AreEqual("66", base16.Encode(Encoding.ASCII.GetBytes("f")));
            Assert.AreEqual("666F", base16.Encode(Encoding.ASCII.GetBytes("fo")));
            Assert.AreEqual("666F6F", base16.Encode(Encoding.ASCII.GetBytes("foo")));
            Assert.AreEqual("666F6F62", base16.Encode(Encoding.ASCII.GetBytes("foob")));
            Assert.AreEqual("666F6F6261", base16.Encode(Encoding.ASCII.GetBytes("fooba")));
            Assert.AreEqual("666F6F626172", base16.Encode(Encoding.ASCII.GetBytes("foobar")));

            // Decoding...
            Assert.AreEqual("", Encoding.ASCII.GetString(base16.Decode("")));
            Assert.AreEqual("f", Encoding.ASCII.GetString(base16.Decode("66")));
            Assert.AreEqual("fo", Encoding.ASCII.GetString(base16.Decode("666F")));
            Assert.AreEqual("foo", Encoding.ASCII.GetString(base16.Decode("666F6F")));
            Assert.AreEqual("foob", Encoding.ASCII.GetString(base16.Decode("666F6F62")));
            Assert.AreEqual("fooba", Encoding.ASCII.GetString(base16.Decode("666F6F6261")));
            Assert.AreEqual("foobar", Encoding.ASCII.GetString(base16.Decode("666F6F626172")));
        }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void InvalidLength1() { base16.Decode("A"); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadCharacter() { base16.Decode("ZZ"); } //must be multiple of 2 chars
    }
}
