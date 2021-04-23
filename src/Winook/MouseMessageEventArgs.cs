namespace Winook
{
    using System;

#pragma warning disable CA1051 // Do not declare visible instance fields
    public class MouseMessageEventArgs : EventArgs
    {
        public int MessageCode;
        public int X;
        public int Y;
        public int Handle;
        public int HitTestCode;
        public short Delta;
        public short XButtons;
        public ushort Modifiers;
        public bool Shift;
        public bool Control;
        public bool Alt;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
