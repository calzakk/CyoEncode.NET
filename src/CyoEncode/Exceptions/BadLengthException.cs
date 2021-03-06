using System;

namespace CyoEncode.Exceptions
{
    public class BadLengthException : Exception
    {
        public BadLengthException(string message) : base(message)
        {
        }
    }
}
