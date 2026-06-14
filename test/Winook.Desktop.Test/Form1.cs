namespace Winook.Desktop.Test
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Winook;

    public partial class Form1 : Form
    {
        private const int RecentMessageCapacity = 20;

        private readonly List<string> _recentMessages = [];
        private readonly object _recentMessagesLock = new();
        private MouseHook _mouseHook;
        private KeyboardHook _keyboardHook;
        private Process _process;
        private bool _mouseHookInstalled;
        private bool _keyboardHookInstalled;
        private bool _mouseHookTransitioning;
        private bool _keyboardHookTransitioning;
        private int _recentMessagesUpdatePending;
        private Label[] _recentMessageLabels;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            _recentMessageLabels =
            [
                recentMessageLabel01,
                recentMessageLabel02,
                recentMessageLabel03,
                recentMessageLabel04,
                recentMessageLabel05,
                recentMessageLabel06,
                recentMessageLabel07,
                recentMessageLabel08,
                recentMessageLabel09,
                recentMessageLabel10,
                recentMessageLabel11,
                recentMessageLabel12,
                recentMessageLabel13,
                recentMessageLabel14,
                recentMessageLabel15,
                recentMessageLabel16,
                recentMessageLabel17,
                recentMessageLabel18,
                recentMessageLabel19,
                recentMessageLabel20,
            ];

            if (!Environment.Is64BitOperatingSystem)
            {
                radio64bit.Enabled = false;
            }
        }

        private async void MouseButton_Click(object sender, EventArgs e)
        {
            if (_mouseHookTransitioning)
            {
                return;
            }

            _mouseHookTransitioning = true;
            mouseButton.Enabled = false;

            try
            {
                if (!_mouseHookInstalled)
                {
                    await EnsureTargetProcessAsync();

                    _mouseHook?.Dispose();
                    _mouseHook = new MouseHook(_process.Id, GetSelectedMouseMessageTypes());

                    ignoreMove.Enabled = false;
                    _mouseHook.MessageReceived += MouseHook_MessageReceived;
                    mouseButton.Text = "Installing hook...";
                    await _mouseHook.InstallAsync();
                    _mouseHookInstalled = true;
                    mouseButton.Text = "Mouse Unhook";
                }
                else
                {
                    _mouseHook.MessageReceived -= MouseHook_MessageReceived;
                    _mouseHook.Uninstall();
                    _mouseHookInstalled = false;
                    mouseButton.Text = "Mouse Hook";
                    ignoreMove.Enabled = true;
                }
            }
            finally
            {
                mouseButton.Enabled = true;
                _mouseHookTransitioning = false;
            }
        }

        private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
        {
            AddRecentMessage("Mouse", FormatMouseMessage(e), e.SendTimestamp);
        }

        private async void KeyboardButton_Click(object sender, EventArgs e)
        {
            if (_keyboardHookTransitioning)
            {
                return;
            }

            _keyboardHookTransitioning = true;
            keyboardButton.Enabled = false;

            try
            {
                if (!_keyboardHookInstalled)
                {
                    await EnsureTargetProcessAsync();

                    _keyboardHook?.Dispose();
                    _keyboardHook = new KeyboardHook(_process.Id);
                    _keyboardHook.MessageReceived += KeyboardHook_MessageReceived;
                    keyboardButton.Text = "Installing hook...";
                    await _keyboardHook.InstallAsync();
                    _keyboardHookInstalled = true;
                    keyboardButton.Text = "Keyboard Unhook";
                }
                else
                {
                    _keyboardHook.MessageReceived -= KeyboardHook_MessageReceived;
                    _keyboardHook.Uninstall();
                    _keyboardHookInstalled = false;
                    keyboardButton.Text = "Keyboard Hook";
                }
            }
            finally
            {
                keyboardButton.Enabled = true;
                _keyboardHookTransitioning = false;
            }
        }

        private void KeyboardHook_MessageReceived(object sender, KeyboardMessageEventArgs e)
        {
            AddRecentMessage("Keyboard", FormatKeyboardMessage(e), e.SendTimestamp);
        }

        private async Task EnsureTargetProcessAsync()
        {
            if (_process != null && !_process.HasExited)
            {
                return;
            }

            var existingCharacterMapProcessIds = Process.GetProcessesByName("charmap")
                .Select(process => process.Id)
                .ToArray();

            var characterMapPath = Environment.Is64BitOperatingSystem && radio32bit.Checked
                ? @"c:\windows\syswow64\charmap.exe"
                : @"c:\windows\system32\charmap.exe";

            var startedProcess = Process.Start(characterMapPath);
            _process = startedProcess;

            await Task.Delay(1000);

            if (HasMainWindow(startedProcess))
            {
                return;
            }

            var replacementProcess = (Process.GetProcessesByName("charmap")
                .Where(process => !existingCharacterMapProcessIds.Contains(process.Id))
                .Where(HasMainWindow)
                .OrderByDescending(GetStartTime)
                .FirstOrDefault()
                ?? Process.GetProcessesByName("charmap")
                    .Where(HasMainWindow)
                    .OrderByDescending(GetStartTime)
                    .FirstOrDefault()) ?? throw new InvalidOperationException("Unable to find the Character Map process window to hook.");
            if (startedProcess != replacementProcess)
            {
                startedProcess?.Dispose();
            }

            _process = replacementProcess;
        }

        private static bool HasMainWindow(Process process)
        {
            if (process == null)
            {
                return false;
            }

            try
            {
                process.Refresh();
                return !process.HasExited && process.MainWindowHandle != IntPtr.Zero;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static DateTime GetStartTime(Process process)
        {
            try
            {
                return process.StartTime;
            }
            catch (InvalidOperationException)
            {
                return DateTime.MinValue;
            }
        }

        private void AddRecentMessage(string hookType, string message, DateTimeOffset sendTimestamp)
        {
            var receivedTimestamp = DateTimeOffset.Now;
            var latency = sendTimestamp == default
                ? "Latency: n/a"
                : $"Latency: {(receivedTimestamp - sendTimestamp).TotalMilliseconds:0.0} ms";
            var sent = sendTimestamp == default
                ? "Sent: n/a"
                : $"Sent: {sendTimestamp.ToLocalTime():HH:mm:ss.fff}";

            lock (_recentMessagesLock)
            {
                _recentMessages.Insert(0, $"{receivedTimestamp:HH:mm:ss.fff} {hookType,-8} {sent}; {latency}; {message}");
                if (_recentMessages.Count > RecentMessageCapacity)
                {
                    _recentMessages.RemoveAt(_recentMessages.Count - 1);
                }
            }

            ScheduleRecentMessagesUpdate();
        }

        private void ScheduleRecentMessagesUpdate()
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            if (Interlocked.Exchange(ref _recentMessagesUpdatePending, 1) == 0)
            {
                BeginInvoke((MethodInvoker)FlushRecentMessages);
            }
        }

        private void FlushRecentMessages()
        {
            Interlocked.Exchange(ref _recentMessagesUpdatePending, 0);

            string[] recentMessages;
            lock (_recentMessagesLock)
            {
                recentMessages = [.. _recentMessages];
            }

            for (var i = 0; i < _recentMessageLabels.Length; i++)
            {
                _recentMessageLabels[i].Text = i < recentMessages.Length ? recentMessages[i] : string.Empty;
            }
        }

        private static string FormatMouseMessage(MouseMessageEventArgs e)
            => $"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Modifiers: {e.Modifiers:x}; "
                + $"Delta: {e.Delta}; XButtons: {e.XButtons}";

        private static string FormatKeyboardMessage(KeyboardMessageEventArgs e)
            => $"Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; "
                + $"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}";

        private MouseMessageTypes GetSelectedMouseMessageTypes()
        {
            return ignoreMove.Checked ? MouseMessageTypes.IgnoreMove : MouseMessageTypes.All;
        }
    }

}
