using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CyoEncode
{
    public interface IEncode
    {
        string Encode(byte[] input);
    }

    public interface IDecode
    {
        byte[] Decode(string input);
    }

    public abstract class EncodeBase : IEncode, IDecode
    {
        public abstract byte[] Decode(string input);

        public abstract string Encode(byte[] input);

        protected static void InitDecodeTable(byte[] decodeTable, string byteToChar)
        {
            for (int i = 0; i < decodeTable.Length; ++i)
                decodeTable[i] = 0xff;

            for (int i = 0; i < byteToChar.Length; ++i)
                decodeTable[byteToChar[i]] = (byte)i;
        }

        protected void ValidateEncoding(string input, int outputChars, string byteToChar, bool allowPadding)
        {
            if ((input.Length % outputChars) != 0)
                throw new Exception("Invalid encoding");

            if (allowPadding)
            {
                input = input.TrimEnd('=');
                byteToChar = byteToChar.TrimEnd('=');
            }

            for (int i = 0; i < input.Length; ++i)
            {
                char ch = input[i];
                if (byteToChar.IndexOf(ch) < 0)
                    throw new Exception($"Invalid character at offset {i}");
            }
        }

        protected int CalcOutputLen(int encodedLength, int inputBytes, int outputChars)
        {
            return (((encodedLength + outputChars - 1) / outputChars) * inputBytes);
        }

        protected byte GetNextByte(string input, int offset, byte[] decodeTable)
        {
            char ch = input[offset];
            if (ch >= 0x80)
                throw new Exception($"Invalid character at offset {offset}");
            else
                return decodeTable[ch];
        }
    }
}
