namespace Winook
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    internal class Helper
    {
        internal static bool Is64BitProcess(IntPtr processHandle)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                return false;
            }

            if (!NativeMethods.IsWow64Process(processHandle, out bool wow64Process))
            {
                throw new Win32Exception();
            }

            return !wow64Process;
        }

        internal static string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }
    }
}
