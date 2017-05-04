// TestBase85.cs - part of the CyoEncode.NET library
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
    public class TestBase85
    {
        private Base85 base85 = new Base85();

        [TestMethod]
        public void TestVectors()
        {
            //test taken from: https://en.wikipedia.org/wiki/Ascii85

            // Encoding...
            var input = "Man is distinguished, not only by his reason, but by this singular passion from other "
                + "animals, which is a lust of the mind, that by a perseverance of delight in the continued and "
                + "indefatigable generation of knowledge, exceeds the short vehemence of any carnal pleasure.";
            var expected = "9jqo^BlbD-BleB1DJ+*+F(f,q/0JhKF<GL>Cj@.4Gp$d7F!,L7@<6@)/0JDEF<G%<+EV:2F!,O<DJ+"
                + "*.@<*K0@<6L(Df-\\0Ec5e;DffZ(EZee.Bl.9pF\"AGXBPCsi+DGm>@3BB/F*&OCAfu2/AKYi(DIb:@FD,*)+C]U"
                + "=@3BN#EcYf8ATD3s@q?d$AftVqCh[NqF<G:8+EV:.+Cf>-FD5W8ARlolDIal(DId<j@<?3r@:F%a+D58'ATD4$B"
                + "l@l3De:,-DJs`8ARoFb/0JMK@qB4^F!,R<AKZ&-DfTqBG%G>uD.RTpAKYo'+CT/5+Cei#DII?(E,9)oF*2M7/c";
            var encoded = base85.Encode(Encoding.ASCII.GetBytes(input));
            Assert.AreEqual(expected, encoded);

            // Decoding...
            //TODO
        }
    }
}
