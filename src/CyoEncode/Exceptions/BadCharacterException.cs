using System;

namespace CyoEncode.Exceptions
{
    public class BadCharacterException : Exception
    {
        public BadCharacterException(string message) : base(message)
        {
        }
    }
}
