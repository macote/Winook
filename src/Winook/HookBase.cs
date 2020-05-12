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
        private const int ProcessWaitForInputIdleWaitTimeInMilliseconds = 2000;

        private readonly int _processId;
        private readonly MessageReceiver _messageReceiver;
        private readonly HookType _hookType;
        private Process _libHostProcess;
        private ManualResetEvent _libHostMutexReleaseEvent;
        private bool _disposed = false;

        #endregion

        #region Constructors

        public HookBase(Process process, HookType hookType, int messageSizeInBytes)
        {
            if (process == null)
            {
                throw new ArgumentNullException(nameof(process));
            }

            _processId = process.Id;
            _hookType = hookType;
            _messageReceiver = new MessageReceiver(messageSizeInBytes);
            _messageReceiver.MessageReceived += OnMessageReceived;
        }

        #endregion

        #region Methods

        public virtual void Install()
        {
            using var process = GetProcess();
            var processHandle = process.Handle;
            if (!process.WaitForInputIdle(ProcessWaitForInputIdleWaitTimeInMilliseconds))
            {
                // Assume that the wait time is good enough to continue
            }

            _messageReceiver.StartListening();

            var libHostMutexGuid = Guid.NewGuid().ToString();
            var libHostMutexName = $"Global\\{libHostMutexGuid}";
            using (var acquireEvent = new ManualResetEvent(false))
            {
                _libHostMutexReleaseEvent = new ManualResetEvent(false);
                Task.Run(() =>
                {
                    using var mutex = new Mutex(true, libHostMutexName, out bool _);
                    acquireEvent.Set();
                    _libHostMutexReleaseEvent.WaitOne();
                    mutex.ReleaseMutex(); // Let host process unhook and terminate
                });
                acquireEvent.WaitOne();
            }

            var libHostExtension = (Helper.Is64BitProcess(processHandle) ? ".x64" : ".x86") + ".exe";
            var libHostExeName = $"{LibHostExeBaseName}{libHostExtension}";
            var libHostExePath = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHostExeName);

            Debug.WriteLine($"{libHostExeName} args: {(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            _libHostProcess = Process.Start(libHostExePath, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            // TODO: check for lib host errors
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
                    _messageReceiver.Dispose();
                }

                _disposed = true;
            }
        }

        protected abstract void OnMessageReceived(object sender, MessageEventArgs e);

        private Process GetProcess() => Process.GetProcessById(_processId);

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
