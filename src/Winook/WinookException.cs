namespace Winook
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WinookException : Exception
    {
        public WinookException()
        {
        }

        public WinookException(string message)
            : base(message)
        {
        }

        public WinookException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected WinookException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
