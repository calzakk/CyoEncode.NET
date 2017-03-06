// TestBase32.cs - part of the CyoEncode.NET library
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
    public class TestBase32
    {
        private Base32 base32 = new Base32();

        [TestMethod]
        public void TestVectorsFromRFC4648()
        {
            // Encoding...
            Assert.AreEqual("", base32.Encode(Encoding.ASCII.GetBytes("")));
            Assert.AreEqual("MY======", base32.Encode(Encoding.ASCII.GetBytes("f")));
            Assert.AreEqual("MZXQ====", base32.Encode(Encoding.ASCII.GetBytes("fo")));
            Assert.AreEqual("MZXW6===", base32.Encode(Encoding.ASCII.GetBytes("foo")));
            Assert.AreEqual("MZXW6YQ=", base32.Encode(Encoding.ASCII.GetBytes("foob")));
            Assert.AreEqual("MZXW6YTB", base32.Encode(Encoding.ASCII.GetBytes("fooba")));
            Assert.AreEqual("MZXW6YTBOI======", base32.Encode(Encoding.ASCII.GetBytes("foobar")));

            // Decoding...
            Assert.AreEqual("", Encoding.ASCII.GetString(base32.Decode("")));
            Assert.AreEqual("f", Encoding.ASCII.GetString(base32.Decode("MY======")));
            Assert.AreEqual("fo", Encoding.ASCII.GetString(base32.Decode("MZXQ====")));
            Assert.AreEqual("foo", Encoding.ASCII.GetString(base32.Decode("MZXW6===")));
            Assert.AreEqual("foob", Encoding.ASCII.GetString(base32.Decode("MZXW6YQ=")));
            Assert.AreEqual("fooba", Encoding.ASCII.GetString(base32.Decode("MZXW6YTB")));
            Assert.AreEqual("foobar", Encoding.ASCII.GetString(base32.Decode("MZXW6YTBOI======")));
        }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength1() { base32.Decode("A"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength2() { base32.Decode("AA"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength3() { base32.Decode("AAA"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength4() { base32.Decode("AAAA"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength5() { base32.Decode("AAAAA"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength6() { base32.Decode("AAAAAA"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength7() { base32.Decode("AAAAAAA"); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadCharacter() { base32.Decode("@@@@@@@@"); } //must be multiple of 8 chars

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding1() { base32.Decode("MZXW6YQ=MZXW6YQ="); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding2() { base32.Decode("MZXW6Y=Q"); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding3() { base32.Decode("MZXW6=Y="); }
    }
}
