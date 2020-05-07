namespace Winook
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, out bool wow64Process);
    }
}
