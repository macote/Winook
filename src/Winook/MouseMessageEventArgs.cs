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
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
