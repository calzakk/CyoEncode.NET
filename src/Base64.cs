using System;
using System.Diagnostics;
using System.Text;

namespace CyoEncode
{
    public class Base64
    {
        public string Encode(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new StringBuilder(outputLen);

            int offset = 0;
            int remaining = input.Length;
            while (remaining >= 1)
            {
                // Input...
                int blockSize = (remaining < InputBytes ? remaining : InputBytes);
                Debug.Assert(blockSize >= 1);
                byte n1 = (byte)((input[offset] & 0xfc) >> 2);
                byte n2 = (byte)((input[offset] & 0x03) << 4);
                byte n3 = Padding;
                byte n4 = Padding;
                if (blockSize >= 2)
                {
                    n2 |= (byte)((input[offset + 1] & 0xf0) >> 4);
                    n3 = (byte)((input[offset + 1] & 0x0f) << 2);
                }
                if (blockSize >= 3)
                {
                    n3 |= (byte)((input[offset + 2] & 0xc0) >> 6);
                    n4 = (byte)(input[offset + 2] & 0x3f);
                }
                offset += blockSize;
                remaining -= blockSize;

                // Validate...
                Debug.Assert(0 <= n1 && n1 <= Padding);
                Debug.Assert(0 <= n2 && n2 <= Padding);
                Debug.Assert(0 <= n3 && n3 <= Padding);
                Debug.Assert(0 <= n4 && n4 <= Padding);

                // Output...
                output.Append(ByteToChar[n1]);
                output.Append(ByteToChar[n2]);
                output.Append(ByteToChar[n3]);
                output.Append(ByteToChar[n4]);
            }

            return output.ToString();
        }

        public byte[] Decode(string input)
        {
            throw new NotImplementedException();
        }

        #region Implementation

        private const int InputBytes = 3;
        private const int OutputChars = 4;
        private const byte Padding = 64;
        private const string ByteToChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        #endregion
    }
}
