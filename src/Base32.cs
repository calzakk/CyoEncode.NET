using System;
using System.Diagnostics;
using System.Text;

namespace CyoEncode
{
    public class Base32
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
                byte n1 = (byte)((input[offset] & 0xf8) >> 3);
                byte n2 = (byte)((input[offset] & 0x07) << 2);
                byte n3 = Padding;
                byte n4 = Padding;
                byte n5 = Padding;
                byte n6 = Padding;
                byte n7 = Padding;
                byte n8 = Padding;
                if (blockSize >= 2)
                {
                    n2 |= (byte)((input[offset + 1] & 0xc0) >> 6);
                    n3 = (byte)((input[offset + 1] & 0x3e) >> 1);
                    n4 = (byte)((input[offset + 1] & 0x01) << 4);
                }
                if (blockSize >= 3)
                {
                    n4 |= (byte)((input[offset + 2] & 0xf0) >> 4);
                    n5 = (byte)((input[offset + 2] & 0x0f) << 1);
                }
                if (blockSize >= 4)
                {
                    n5 |= (byte)((input[offset + 3] & 0x80) >> 7);
                    n6 = (byte)((input[offset + 3] & 0x7c) >> 2);
                    n7 = (byte)((input[offset + 3] & 0x03) << 3);
                }
                if (blockSize >= 5)
                {
                    n7 |= (byte)((input[offset + 4] & 0xe0) >> 5);
                    n8 = (byte)(input[offset + 4] & 0x1f);
                }
                offset += blockSize;
                remaining -= blockSize;

                // Validate...
                Debug.Assert(0 <= n1 && n1 <= Padding);
                Debug.Assert(0 <= n2 && n2 <= Padding);
                Debug.Assert(0 <= n3 && n3 <= Padding);
                Debug.Assert(0 <= n4 && n4 <= Padding);
                Debug.Assert(0 <= n5 && n5 <= Padding);
                Debug.Assert(0 <= n6 && n6 <= Padding);
                Debug.Assert(0 <= n7 && n7 <= Padding);
                Debug.Assert(0 <= n8 && n8 <= Padding);


                // Output...
                output.Append(ByteToChar[n1]);
                output.Append(ByteToChar[n2]);
                output.Append(ByteToChar[n3]);
                output.Append(ByteToChar[n4]);
                output.Append(ByteToChar[n5]);
                output.Append(ByteToChar[n6]);
                output.Append(ByteToChar[n7]);
                output.Append(ByteToChar[n8]);
            }

            return output.ToString();
        }

        public byte[] Decode(string input)
        {
            throw new NotImplementedException();
        }

        #region Implementation

        private const int InputBytes = 5;
        private const int OutputChars = 8;
        private const byte Padding = 32;
        private const string ByteToChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567=";

        #endregion
    }
}
