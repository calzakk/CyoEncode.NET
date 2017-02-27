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
            //TODO
        }
    }
}
