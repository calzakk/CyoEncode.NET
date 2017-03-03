using CyoEncode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class TestBase64
    {
        private Base64 base64 = new Base64();

        [TestMethod]
        public void TestVectorsFromRFC4648()
        {
            var base64 = new Base64();

            // Encoding...
            Assert.AreEqual("", base64.Encode(Encoding.ASCII.GetBytes("")));
            Assert.AreEqual("Zg==", base64.Encode(Encoding.ASCII.GetBytes("f")));
            Assert.AreEqual("Zm8=", base64.Encode(Encoding.ASCII.GetBytes("fo")));
            Assert.AreEqual("Zm9v", base64.Encode(Encoding.ASCII.GetBytes("foo")));
            Assert.AreEqual("Zm9vYg==", base64.Encode(Encoding.ASCII.GetBytes("foob")));
            Assert.AreEqual("Zm9vYmE=", base64.Encode(Encoding.ASCII.GetBytes("fooba")));
            Assert.AreEqual("Zm9vYmFy", base64.Encode(Encoding.ASCII.GetBytes("foobar")));

            // Decoding...
            Assert.AreEqual("", Encoding.ASCII.GetString(base64.Decode("")));
            Assert.AreEqual("f", Encoding.ASCII.GetString(base64.Decode("Zg==")));
            Assert.AreEqual("fo", Encoding.ASCII.GetString(base64.Decode("Zm8=")));
            Assert.AreEqual("foo", Encoding.ASCII.GetString(base64.Decode("Zm9v")));
            Assert.AreEqual("foob", Encoding.ASCII.GetString(base64.Decode("Zm9vYg==")));
            Assert.AreEqual("fooba", Encoding.ASCII.GetString(base64.Decode("Zm9vYmE=")));
            Assert.AreEqual("foobar", Encoding.ASCII.GetString(base64.Decode("Zm9vYmFy")));
        }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength1() { base64.Decode("A"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength2() { base64.Decode("AA"); }

        [TestMethod]
        [ExpectedException(typeof(BadLengthException))]
        public void BadLength3() { base64.Decode("AAA"); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadCharacter() { base64.Decode("@@@@"); } //must be multiple of 4 chars

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding1() { base64.Decode("Zm8=Zm8="); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding2() { base64.Decode("Zm=8"); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding3() { base64.Decode("Z=m8"); }

        [TestMethod]
        [ExpectedException(typeof(BadCharacterException))]
        public void BadPadding4() { base64.Decode("Z=m="); }
    }
}
