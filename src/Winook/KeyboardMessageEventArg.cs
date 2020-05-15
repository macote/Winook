namespace Winook
{
    using System;

#pragma warning disable CA1051 // Do not declare visible instance fields
    public class KeyboardMessageEventArgs : EventArgs
    {
        public int VirtualKeyCode;
        public uint Flags;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
