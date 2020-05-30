namespace Winook
{
    using System;

#pragma warning disable CA1051 // Do not declare visible instance fields
    public class KeyboardMessageEventArgs : EventArgs
    {
        public ushort VirtualKeyCode;
        public ushort ShiftControlAltFlags;
        public uint Flags;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
