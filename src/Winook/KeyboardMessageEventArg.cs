namespace Winook
{
    using System;

#pragma warning disable CA1051 // Do not declare visible instance fields
    public class KeyboardMessageEventArgs : EventArgs
    {
        public ushort KeyValue;
        public ushort Modifiers;
        public uint Flags;
        public bool Shift;
        public bool Control;
        public bool Alt;
        public KeyDirection Direction;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
