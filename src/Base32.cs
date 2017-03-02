using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CyoEncode
{
    public sealed class Base32 : EncodeBase
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
                byte in5 = GetNextByte(input, inputOffset++, DecodeTable);
                byte in6 = GetNextByte(input, inputOffset++, DecodeTable);
                byte in7 = GetNextByte(input, inputOffset++, DecodeTable);
                byte in8 = GetNextByte(input, inputOffset++, DecodeTable);
                remaining -= OutputChars;

                // Validate padding...
                if (remaining == 0)
                {
                    //this is the final block
                    //the first two chars cannot be padding
                    if (in1 >= Padding || in2 >= Padding)
                        throw new Exception("Invalid base32 character");
                    //the following can be padding
                    if (in3 > Padding || in4 > Padding || in5 > Padding
                        || in6 > Padding || in7 > Padding || in8 > Padding)
                        throw new Exception("Invalid base32 character");
                }
                else
                {
                    //no chars can be padding
                    if (in1 >= Padding || in2 >= Padding || in3 >= Padding || in4 >= Padding
                        || in5 >= Padding || in6 >= Padding || in7 >= Padding || in8 >= Padding)
                        throw new Exception("Invalid base32 character");
                }

                // Outputs...
                output.Add((byte)(((in1 & 0x1f) << 3) | ((in2 & 0x1c) >> 2)));
                output.Add((byte)(((in2 & 0x03) << 6) | ((in3 & 0x1f) << 1) | ((in4 & 0x10) >> 4)));
                output.Add((byte)(((in4 & 0x0f) << 4) | ((in5 & 0x1e) >> 1)));
                output.Add((byte)(((in5 & 0x01) << 7) | ((in6 & 0x1f) << 2) | ((in7 & 0x18) >> 3)));
                output.Add((byte)(((in7 & 0x07) << 5) | (in8 & 0x1f)));
                outputLen += InputBytes;

                // Padding...
                if (in8 == Padding)
                {
                    --outputLen;
                    Debug.Assert((in7 == Padding && in6 == Padding) || (in7 != Padding));
                    if (in6 == Padding)
                    {
                        --outputLen;
                        if (in5 == Padding)
                        {
                            --outputLen;
                            Debug.Assert((in4 == Padding && in3 == Padding) || (in4 != Padding));
                            if (in3 == Padding)
                                --outputLen;
                        }
                    }
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

        private const int InputBytes = 5;
        private const int OutputChars = 8;
        private const byte Padding = 32;
        private const string ByteToChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567=";
        private static readonly byte[] DecodeTable = new byte[128];

        static Base32()
        {
            InitDecodeTable(DecodeTable, ByteToChar);
        }

        #endregion
    }
}
