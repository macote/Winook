namespace Winook
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    public abstract class Hook : IDisposable
    {
        #region Fields

        private const string LibHostExeBaseName = "winook.support\\Winook.Lib.Host";

        private Process _targetProcess;
        private Process _libHostProcess;
        private Mutex _libHostMutex;
        private MessageReceiver _messageReceiver;
        private int _hookType;
        private bool _disposed = false;

        #endregion

        #region Constructors

        public Hook(Process targetProcess, int hookType, int messageSizeInBytes)
        {
            _targetProcess = targetProcess;
            _hookType = hookType;
            _messageReceiver = new MessageReceiver(messageSizeInBytes);
            _messageReceiver.MessageReceived += OnMessageReceived;
        }

        #endregion

        #region Methods

        public virtual void Install()
        {
            _messageReceiver.StartListening();

            var libHostMutexGuid = Guid.NewGuid().ToString();
            var libHostMutex = $"Global\\{libHostMutexGuid}";
            _libHostMutex = new Mutex(true, libHostMutex, out bool _);

            var libHostExtension = (Is64BitProcess(_targetProcess) ? ".x64" : ".x86") + ".exe";
            var libHostExeName = $"{LibHostExeBaseName}{libHostExtension}";
            var libHostExePath = Path.Combine(GetExecutingAssemblyDirectory(), libHostExeName);

            Debug.WriteLine($"{libHostExeName} args: {_hookType} {_messageReceiver.Port} {_targetProcess.Id} {libHostMutexGuid}");

            _libHostProcess = Process.Start(libHostExePath, $"{_hookType} {_messageReceiver.Port} {_targetProcess.Id} {libHostMutexGuid}");

            // TODO: add a hook confirmation by validating an init message sent from lib
            // TODO: check for lib host errors
        }

        public void Uninstall()
        {
            ReleaseAndDisposeMutex();
            _messageReceiver.Stop();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ReleaseAndDisposeMutex();
                    _libHostProcess?.Dispose();
                    _messageReceiver.Dispose();
                }

                _disposed = true;
            }
        }

        private static bool Is64BitProcess(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
            {
                return false;
            }
            if (!NativeMethods.IsWow64Process(process.Handle, out bool wow64Process))
            {
                throw new Win32Exception();
            }

            return !wow64Process;
        }

        private void ReleaseAndDisposeMutex()
        {
            if (_libHostMutex != null)
            {
                _libHostMutex.ReleaseMutex(); // Let exe unhook and terminate
                _libHostMutex.Dispose();
                _libHostMutex = null;
            }
        }

        private static string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        protected abstract void OnMessageReceived(object sender, MessageEventArgs e);

        #endregion
    }
}
