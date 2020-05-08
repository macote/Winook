namespace Winook
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HookBase : IDisposable
    {
        #region Fields

        private const string LibHostExeBaseName = "winook.support\\Winook.Lib.Host";

        private Process _targetProcess;
        private Process _libHostProcess;
        private MessageReceiver _messageReceiver;
        private ManualResetEvent _libHostMutexReleaseEvent;
        private int _hookType;
        private bool _disposed = false;

        #endregion

        #region Constructors

        public HookBase(Process targetProcess, int hookType, int messageSizeInBytes)
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
            var libHostMutexName = $"Global\\{libHostMutexGuid}";
            using (var acquireEvent = new ManualResetEvent(false))
            {
                _libHostMutexReleaseEvent = new ManualResetEvent(false);
                Task.Run(() =>
                {
                    using (var mutex = new Mutex(true, libHostMutexName, out bool _))
                    {
                        acquireEvent.Set();
                        _libHostMutexReleaseEvent.WaitOne();
                        mutex.ReleaseMutex(); // Let host process unhook and terminate
                }
                });
                acquireEvent.WaitOne();
            }

            var libHostExtension = (Is64BitProcess(_targetProcess) ? ".x64" : ".x86") + ".exe";
            var libHostExeName = $"{LibHostExeBaseName}{libHostExtension}";
            var libHostExePath = Path.Combine(GetExecutingAssemblyDirectory(), libHostExeName);

            Debug.WriteLine($"{libHostExeName} args: {_hookType} {_messageReceiver.Port} {_targetProcess.Id} {libHostMutexGuid}");

            _libHostProcess = Process.Start(libHostExePath, $"{_hookType} {_messageReceiver.Port} {_targetProcess.Id} {libHostMutexGuid}");

            // TODO: check for lib host errors
            // TODO: add a hook confirmation by validating an init message sent from lib
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
            if (_libHostMutexReleaseEvent != null)
            {
                _libHostMutexReleaseEvent.Set();
                _libHostMutexReleaseEvent.Dispose();
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
