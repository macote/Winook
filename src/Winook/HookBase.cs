namespace Winook
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HookBase : IDisposable
    {
        #region Fields

        private const string LibHostExeBaseName = "winook.support\\Winook.Lib.Host";
        private const int InitializationTimeoutInMilliseconds = 1000;

        private readonly int _processId;
        private readonly MessageReceiver _messageReceiver;
        private readonly HookType _hookType;
        private Process _libHostProcess;
        private Process _libHostProcess64;
        private ManualResetEvent _libHostMutexReleaseEvent;
        private bool _disposed = false;

        #endregion

        #region Constructors

        public HookBase(int processId, HookType hookType, int messageSizeInBytes)
        {
            _processId = processId;
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

            var libHostExeName = $"{LibHostExeBaseName}.x86.exe";
            var libHostExePath = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHostExeName);
            _libHostProcess = Process.Start(libHostExePath, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            Debug.WriteLine($"{libHostExeName} args: {(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            if (Environment.Is64BitOperatingSystem)
            {
                var libHost64ExeName = $"{LibHostExeBaseName}.x64.exe";
                var libHost64ExePath = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHost64ExeName);
                _libHostProcess64 = Process.Start(libHost64ExePath, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");
            }

            var stopwatch = Stopwatch.StartNew();
            var exitCode = 0;
            var exitCode64 = 0;
            var hostRunning = false;
            var host64Running = false;
            using (var signal64 = new ManualResetEvent(false))
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    Task.Run(() =>
                    {
                        if (!_libHostProcess64.WaitForExit(InitializationTimeoutInMilliseconds))
                        {
                            host64Running = true;
                        }
                        else
                        {
                            exitCode64 = _libHostProcess64.ExitCode;
                        }
                        signal64.Set();
                    });
                }
                else
                {
                    signal64.Set();
                }

                if (!_libHostProcess.WaitForExit(InitializationTimeoutInMilliseconds))
                {
                    hostRunning = true;
                }
                else
                {
                    exitCode = _libHostProcess.ExitCode;
                }

                signal64.WaitOne();

                if (exitCode != 0 || exitCode64 != 0)
                {
                }
                else if (hostRunning && host64Running)
                {
                }

                Debug.WriteLine($"exitCode: {exitCode}; exitCode64: {exitCode64}");
                Debug.WriteLine($"hostRunning: {hostRunning}; host64Running: {host64Running}");
            }

            Debug.WriteLine($"elapsed: {stopwatch.ElapsedMilliseconds}");

            // TODO: add a hook confirmation by validating an init message sent from lib
        }

        public virtual void Uninstall()
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
                    _libHostProcess64?.Dispose();
                    _messageReceiver.Dispose();
                }

                _disposed = true;
            }
        }

        protected abstract void OnMessageReceived(object sender, MessageEventArgs e);

        private void ReleaseAndDisposeMutex()
        {
            if (_libHostMutexReleaseEvent != null && !_libHostMutexReleaseEvent.SafeWaitHandle.IsClosed)
            {
                _libHostMutexReleaseEvent.Set();
                _libHostMutexReleaseEvent.Dispose();
            }
        }

        #endregion
    }
}
