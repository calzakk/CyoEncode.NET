using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CyoEncode
{
    public sealed class Base64 : EncodeBase
    {
        public override string Encode(byte[] input)
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

        public override byte[] Decode(string input)
        {
            ValidateEncoding(input, OutputChars, ByteToChar, true);

            int maxOutputLen = CalcOutputLen(input.Length, InputBytes, OutputChars);
            var output = new List<byte>(maxOutputLen);
            int outputLen = 0;
            int inputOffset = 0;
            int remaining = input.Length;

            while (remaining != 0)
            {
                // Inputs...
                byte in1 = GetNextByte(input, inputOffset++, DecodeTable);
                byte in2 = GetNextByte(input, inputOffset++, DecodeTable);
                byte in3 = GetNextByte(input, inputOffset++, DecodeTable);
                byte in4 = GetNextByte(input, inputOffset++, DecodeTable);
                remaining -= OutputChars;

                // Validate padding...
                if (remaining == 0)
                {
                    //this is the final block
                    //the first two chars cannot be padding
                    if (in1 >= Padding || in2 >= Padding)
                        throw new Exception("Invalid base64 character");
                    //the following can be padding
                    if (in3 > Padding || in4 > Padding)
                        throw new Exception("Invalid base64 character");
                }
                else
                {
                    //no chars can be padding
                    if (in1 >= Padding || in2 >= Padding || in3 >= Padding || in4 >= Padding)
                        throw new Exception("Invalid base64 character");
                }

                // Outputs...
                output.Add((byte)(((in1 & 0x3f) << 2) | ((in2 & 0x30) >> 4)));
                output.Add((byte)(((in2 & 0x0f) << 4) | ((in3 & 0x3c) >> 2)));
                output.Add((byte)(((in3 & 0x03) << 6) | (in4 & 0x3f)));
                outputLen += InputBytes;

                // Padding...
                if (in4 == Padding)
                {
                    --outputLen;
                    if (in3 == Padding)
                        --outputLen;
                }
            }

            if (outputLen < output.Count)
            {
                int bytesToRemove = (output.Count - outputLen);
                output.RemoveRange(output.Count - bytesToRemove, bytesToRemove);
            }

            return output.ToArray();
        }

        #region Implementation

        private const int InputBytes = 3;
        private const int OutputChars = 4;
        private const byte Padding = 64;
        private const string ByteToChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
        private static readonly byte[] DecodeTable = new byte[128];

        static Base64()
        {
            InitDecodeTable(DecodeTable, ByteToChar);
        }

        #endregion
    }
}
