using System;
using System.Diagnostics;
using System.Text;

namespace CyoEncode
{
    public class Base16
    {
        public string Encode(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new StringBuilder(outputLen);
            foreach (byte b in input)
            {
                // Input...
                byte n1 = (byte)((b & 0xf0) >> 4);
                byte n2 = (byte)(b & 0x0f);

                // Validate...
                Debug.Assert(0 <= n1 && n1 <= 16);
                Debug.Assert(0 <= n2 && n2 <= 16);

                // Output...
                output.Append(ByteToChar[n1]);
                output.Append(ByteToChar[n2]);
            }
            return output.ToString();
        }

        public byte[] Decode(string input)
        {
            throw new NotImplementedException();
        }

        #region Implementation

        private const int InputBytes = 1;
        private const int OutputChars = 2;
        private const string ByteToChar = "0123456789ABCDEF";

        #endregion
    }
}
