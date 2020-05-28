namespace Winook
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HookBase : IDisposable
    {
        #region Fields

        private const string LibHostBaseName = "winook.support\\Winook.Lib.Host";
        private const int InitializationTimeout1InMilliseconds = 222;
        private const int InitializationTimeout2InMilliseconds = 1111;
        private const int InitializationTimeout3InMilliseconds = 3333;

        private readonly int _processId;
        private readonly MessageReceiver _messageReceiver;
        private readonly HookType _hookType;
        private Process _libHostProcess;
        private Process _libHostProcess64;
        private ManualResetEventSlim _libHostMutexReleaseEvent;
        private bool _disposed = false;

        private ResourceManager _resourceManager = new ResourceManager(typeof(Winook.Properties.Resources));

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

        public async Task InstallAsync()
        {
            _messageReceiver.StartListening();

            var libHostMutexGuid = Guid.NewGuid().ToString();
            var libHostMutexName = $"Global\\{libHostMutexGuid}";
            using (var acquireEvent = new ManualResetEventSlim(false))
            {
                _libHostMutexReleaseEvent = new ManualResetEventSlim(false);
                _ = Task.Run(() =>
                  {
                      using (var mutex = new Mutex(true, libHostMutexName, out _))
                      {
                          acquireEvent.Set();
                          _libHostMutexReleaseEvent.Wait();
                          mutex.ReleaseMutex(); // Let host process unhook and terminate
                    }
                  });
                acquireEvent.Wait();
            }

            var libHostName = $"{LibHostBaseName}.x86.exe";
            var libHostPath = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHostName);
            _libHostProcess = Process.Start(libHostPath, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            Debug.WriteLine($"{libHostName} args: {(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");

            if (Environment.Is64BitOperatingSystem)
            {
                var libHost64Name = $"{LibHostBaseName}.x64.exe";
                var libHost64Path = Path.Combine(Helper.GetExecutingAssemblyDirectory(), libHost64Name);
                _libHostProcess64 = Process.Start(libHost64Path, $"{(int)_hookType} {_messageReceiver.Port} {_processId} {libHostMutexGuid}");
            }

            var errorFile = Path.Combine(Path.GetTempPath(), libHostMutexGuid);
            await Task.Delay(InitializationTimeout1InMilliseconds).ConfigureAwait(false);
            CheckLibHostsStatus(out bool hostRunning, out bool host64Running, out int exitCode, out int exitCode64);
            if (hostRunning && host64Running)
            {
                await Task.Delay(InitializationTimeout2InMilliseconds).ConfigureAwait(false);
                CheckLibHostsStatus(out hostRunning, out host64Running, out exitCode, out exitCode64);
            }

            if (hostRunning && host64Running)
            {
                await Task.Delay(InitializationTimeout3InMilliseconds).ConfigureAwait(false);
                CheckLibHostsStatus(out hostRunning, out host64Running, out exitCode, out exitCode64);
            }

            if (hostRunning && host64Running)
            {
                throw new WinookException(_resourceManager.GetString("HostApplicationsTimedOut", CultureInfo.CurrentCulture));
            }

            var errorFileExists = File.Exists(errorFile);
            if (exitCode != 0 || exitCode64 != 0 || errorFileExists)
            {
                if (errorFileExists)
                {
                    throw new WinookException(File.ReadAllText(errorFile));
                }
                else if (!(exitCode != 0 && exitCode64 != 0))
                {
                    throw new WinookException(_resourceManager.GetString("HostApplicationFailed", CultureInfo.CurrentCulture));
                }
                else
                {
                    throw new WinookException(_resourceManager.GetString("HostApplicationsFailed", CultureInfo.CurrentCulture));
                }
            }
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
                    _libHostProcess64?.Dispose();
                    _messageReceiver.Dispose();
                }

                _disposed = true;
            }
        }

        protected abstract void OnMessageReceived(object sender, MessageEventArgs e);

        private void CheckLibHostsStatus(out bool hostRunning, out bool host64Running, out int exitCode, out int exitCode64)
        {
            exitCode = 0;
            exitCode64 = 0;
            host64Running = false;
            if (Environment.Is64BitOperatingSystem)
            {
                host64Running = !_libHostProcess64.HasExited;
                if (!host64Running)
                {
                    exitCode64 = _libHostProcess64.ExitCode;
                }
            }

            hostRunning = !_libHostProcess.HasExited;
            if (!hostRunning)
            {
                exitCode = _libHostProcess.ExitCode;
            }
        }

        private void ReleaseAndDisposeMutex()
        {
            if (_libHostMutexReleaseEvent != null)
            {
                _libHostMutexReleaseEvent.Set();
                _libHostMutexReleaseEvent.Dispose();
            }
        }

        #endregion
    }
}
