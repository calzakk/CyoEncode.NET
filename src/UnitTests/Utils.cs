using System.Text;

namespace UnitTests
{
    static class Utils
    {
        public static byte[] GetBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }
}
