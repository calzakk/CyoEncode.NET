using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class Base32
    {
        [TestMethod]
        public void TestVectorsFromRFC4648()
        {
            var base32 = new CyoEncode.Base32();

            // Encoding...
            Assert.AreEqual("", base32.Encode(Utils.GetBytes("")));
            Assert.AreEqual("MY======", base32.Encode(Utils.GetBytes("f")));
            Assert.AreEqual("MZXQ====", base32.Encode(Utils.GetBytes("fo")));
            Assert.AreEqual("MZXW6===", base32.Encode(Utils.GetBytes("foo")));
            Assert.AreEqual("MZXW6YQ=", base32.Encode(Utils.GetBytes("foob")));
            Assert.AreEqual("MZXW6YTB", base32.Encode(Utils.GetBytes("fooba")));
            Assert.AreEqual("MZXW6YTBOI======", base32.Encode(Utils.GetBytes("foobar")));

            // Decoding...
            Assert.AreEqual("", Utils.GetString(base32.Decode("")));
            Assert.AreEqual("f", Utils.GetString(base32.Decode("MY======")));
            Assert.AreEqual("fo", Utils.GetString(base32.Decode("MZXQ====")));
            Assert.AreEqual("foo", Utils.GetString(base32.Decode("MZXW6===")));
            Assert.AreEqual("foob", Utils.GetString(base32.Decode("MZXW6YQ=")));
            Assert.AreEqual("fooba", Utils.GetString(base32.Decode("MZXW6YTB")));
            Assert.AreEqual("foobar", Utils.GetString(base32.Decode("MZXW6YTBOI======")));
        }
    }
}
