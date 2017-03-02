using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CyoEncode
{
    public sealed class Base16 : EncodeBase
    {
        public override string Encode(byte[] input)
        {
            int outputLen = (((input.Length + InputBytes - 1) / InputBytes) * OutputChars);
            var output = new StringBuilder(outputLen);
            foreach (byte b in input)
            {
                // Input...
                byte n1 = (byte)((b & 0xf0) >> 4);
                byte n2 = (byte)(b & 0x0f);

                // Validate...
                Debug.Assert(0 <= n1 && n1 <= MaxValue);
                Debug.Assert(0 <= n2 && n2 <= MaxValue);

                // Output...
                output.Append(ByteToChar[n1]);
                output.Append(ByteToChar[n2]);
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
                remaining -= OutputChars;

                // Outputs...
                output.Add((byte)((in1 << 4) | in2));
                outputLen += InputBytes;
            }

            Debug.Assert(outputLen == maxOutputLen);

            return output.ToArray();
        }

        #region Implementation

        private const int InputBytes = 1;
        private const int OutputChars = 2;
        private const byte MaxValue = 15;
        private const string ByteToChar = "0123456789ABCDEF";
        private static readonly byte[] DecodeTable = new byte[128];

        static Base16()
        {
            InitDecodeTable(DecodeTable, ByteToChar);
        }

        #endregion
    }
}
