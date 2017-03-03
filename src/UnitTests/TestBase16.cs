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
