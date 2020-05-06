namespace Winook
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class MouseHook : IDisposable
    {
        #region Imports

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process([In] IntPtr process, out bool wow64Process);

        #endregion

        #region Fields

        private const int MsgCodeMouseLeftButtonUp = 0x0202; // WM_LBUTTONUP
        private const int MouseHookMessageSizeInBytes = 24;
        private const string LibHostExeBaseName = "winook\\Winook.Lib.Host";

        private Process _targetProcess;
        private Process _libHostProcess;
        private Mutex _libHostMutex;
        private MessageReceiver _messageReceiver;
        private bool _disposed = false;

        #endregion

        #region Events

        public event EventHandler<MouseMessageEventArgs> MouseMessageReceived;
        public event EventHandler<MouseMessageEventArgs> MouseLeftButtonUp;

        #endregion

        #region Constructors

        public MouseHook(Process targetProcess)
        {
            _targetProcess = targetProcess;
            _messageReceiver = new MessageReceiver(MouseHookMessageSizeInBytes);
            _messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
        }

        #endregion

        #region Methods

        public void Install()
        {
            _messageReceiver.StartListening();

            var libHostMutexGuid = Guid.NewGuid().ToString();
            var libHostMutex = $"Global\\{libHostMutexGuid}";
            _libHostMutex = new Mutex(true, libHostMutex, out bool _);

            var libHostExtension = (Is64BitProcess(_targetProcess) ? ".x64" : ".x86") + ".exe";
            var libHostExeName = $"{LibHostExeBaseName}{libHostExtension}";
            var libHostExePath = Path.Combine(GetExecutingAssemblyDirectory(), libHostExeName);

            Debug.WriteLine($"{libHostExeName} args: {_messageReceiver.Port} {_targetProcess.Id} {libHostMutexGuid}");

            _libHostProcess = Process.Start(libHostExePath, $"{_messageReceiver.Port} {_targetProcess.Id} {libHostMutexGuid}");

            // TODO: add a hook confirmation by validating an init message sent from lib
            // TODO: check for lib host errors
        }

        public void Uninstall()
        {
            ReleaseAndDisposeMutex();
            _messageReceiver.Stop();
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        ReleaseAndDisposeMutex();
                        if (_libHostProcess != null)
                        {
                            _libHostProcess.Dispose();
                        }

                        _messageReceiver.Dispose();
                    }
                    catch
                    {
                    }
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
            if (!IsWow64Process(process.Handle, out bool wow64Process))
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
            }
        }

        private string GetExecutingAssemblyDirectory()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        private void MessageReceiver_MessageReceived(object sender, MessageEventArgs e)
        {
            var eventArgs = new MouseMessageEventArgs
            {
                MessageCode = BitConverter.ToInt32(e.Bytes, 0),
                X = BitConverter.ToInt32(e.Bytes, 4),
                Y = BitConverter.ToInt32(e.Bytes, 8),
                Handle = BitConverter.ToInt32(e.Bytes, 12),
                HitTestCode = BitConverter.ToInt32(e.Bytes, 16),
                Delta = BitConverter.ToInt16(e.Bytes, 20),
            };

            Debug.WriteLine($"Mouse Message Code: {eventArgs.MessageCode}; X: {eventArgs.X}; Y: {eventArgs.Y}; Delta: {eventArgs.Delta}");

            if (eventArgs.MessageCode == MsgCodeMouseLeftButtonUp)
            {
                this.MouseLeftButtonUp?.Invoke(this, eventArgs);
            }

            MouseMessageReceived?.Invoke(this, eventArgs);
        }

        #endregion
    }

    public class MouseMessageEventArgs : EventArgs
    {
        public int MessageCode;
        public int X;
        public int Y;
        public int Handle;
        public int HitTestCode;
        public short Delta;
    }
}
