using System;

namespace VdGfx
{
    public class VxException : Exception
    {
        public VxException()
        {
        }

        public VxException(string message) : base(message)
        {
        }

        public VxException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
