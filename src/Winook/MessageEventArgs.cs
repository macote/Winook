namespace Winook
{
    using System;

#pragma warning disable CA1051 // Do not declare visible instance fields
    public class MessageEventArgs : EventArgs
    {
        public byte[] Bytes;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
