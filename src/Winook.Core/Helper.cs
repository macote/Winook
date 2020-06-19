namespace Winook
{
    using System;
    using System.IO;
    using System.Reflection;

    internal class Helper
    {
        #region Methods

        internal static string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        #endregion
    }
}
