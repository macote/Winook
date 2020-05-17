namespace Winook
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public enum ProcessBitness
    {
        Unknown,
        Is32Bit,
        Is64Bit
    }

    public abstract class HookBase : IDisposable
    {
        #region Fields

        private const string LibHostExeBaseName = "winook.support\\Winook.Lib.Host";

        private readonly int _processId;
        private readonly MessageReceiver _messageReceiver;
        private readonly HookType _hookType;
        private Process _libHostProcess32;
        private Process _libHostProcess64;
        private ManualResetEvent _libHostMutexReleaseEvent;
        private ProcessBitness _processBitness;
        private bool _disposed = false;

        #endregion

        #region Constructors

        public HookBase(int processId, HookType hookType, int messageSizeInBytes)
            : this(processId, ProcessBitness.Unknown, hookType, messageSizeInBytes)
        {
        }

        public HookBase(int processId, ProcessBitness processBitness, HookType hookType, int messageSizeInBytes)
        {
            _processId = processId;
            _processBitness = processBitness;
            _hookType = hookType;
            _messageReceiver = new MessageReceiver(messageSizeInBytes);
            _messageReceiver.MessageReceived += OnMessageReceived;
        }

        #endregion

        #region Methods

        public virtual void Install()
        {
            var is64BitProcess = false;
            if (_processBitness == ProcessBitness.Unknown)
            {
                using var process = GetProcess();
                var processHandle = process.Handle;
                is64BitProcess = Helper.Is64BitProcess(processHandle);
            } 
            else if (_processBitness == ProcessBitness.Is64Bit)
            {
                is64BitProcess = true;
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

            var libHostExeName = $"{LibHostExeBaseName}.x86.exe";
            var libHostExePath = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHostExeName);
            _libHostProcess32 = Process.Start(libHostExePath, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            Debug.WriteLine($"{libHostExeName} args: {(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            if (Environment.Is64BitOperatingSystem)
            {
                var libHost64ExeName = $"{LibHostExeBaseName}.x64.exe";
                var libHost64ExePath = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHost64ExeName);
                _libHostProcess64 = Process.Start(libHost64ExePath, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");
            }

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
                    _libHostProcess32?.Dispose();
                    _libHostProcess64?.Dispose();
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
