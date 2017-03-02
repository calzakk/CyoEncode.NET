using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class Base64
    {
        [TestMethod]
        public void TestVectorsFromRFC4648()
        {
            var base64 = new CyoEncode.Base64();

            // Encoding...
            Assert.AreEqual("", base64.Encode(Utils.GetBytes("")));
            Assert.AreEqual("Zg==", base64.Encode(Utils.GetBytes("f")));
            Assert.AreEqual("Zm8=", base64.Encode(Utils.GetBytes("fo")));
            Assert.AreEqual("Zm9v", base64.Encode(Utils.GetBytes("foo")));
            Assert.AreEqual("Zm9vYg==", base64.Encode(Utils.GetBytes("foob")));
            Assert.AreEqual("Zm9vYmE=", base64.Encode(Utils.GetBytes("fooba")));
            Assert.AreEqual("Zm9vYmFy", base64.Encode(Utils.GetBytes("foobar")));

            // Decoding...
            Assert.AreEqual("", Utils.GetString(base64.Decode("")));
            Assert.AreEqual("f", Utils.GetString(base64.Decode("Zg==")));
            Assert.AreEqual("fo", Utils.GetString(base64.Decode("Zm8=")));
            Assert.AreEqual("foo", Utils.GetString(base64.Decode("Zm9v")));
            Assert.AreEqual("foob", Utils.GetString(base64.Decode("Zm9vYg==")));
            Assert.AreEqual("fooba", Utils.GetString(base64.Decode("Zm9vYmE=")));
            Assert.AreEqual("foobar", Utils.GetString(base64.Decode("Zm9vYmFy")));
        }
    }
}
