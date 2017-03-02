using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class Base16
    {
        [TestMethod]
        public void TestVectorsFromRFC4648()
        {
            var base16 = new CyoEncode.Base16();

            // Encoding...
            Assert.AreEqual("", base16.Encode(Utils.GetBytes("")));
            Assert.AreEqual("66", base16.Encode(Utils.GetBytes("f")));
            Assert.AreEqual("666F", base16.Encode(Utils.GetBytes("fo")));
            Assert.AreEqual("666F6F", base16.Encode(Utils.GetBytes("foo")));
            Assert.AreEqual("666F6F62", base16.Encode(Utils.GetBytes("foob")));
            Assert.AreEqual("666F6F6261", base16.Encode(Utils.GetBytes("fooba")));
            Assert.AreEqual("666F6F626172", base16.Encode(Utils.GetBytes("foobar")));

            // Decoding...
            Assert.AreEqual("", Utils.GetString(base16.Decode("")));
            Assert.AreEqual("f", Utils.GetString(base16.Decode("66")));
            Assert.AreEqual("fo", Utils.GetString(base16.Decode("666F")));
            Assert.AreEqual("foo", Utils.GetString(base16.Decode("666F6F")));
            Assert.AreEqual("foob", Utils.GetString(base16.Decode("666F6F62")));
            Assert.AreEqual("fooba", Utils.GetString(base16.Decode("666F6F6261")));
            Assert.AreEqual("foobar", Utils.GetString(base16.Decode("666F6F626172")));
        }
    }
}
