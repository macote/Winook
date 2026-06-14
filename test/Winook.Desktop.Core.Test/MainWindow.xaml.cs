using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Winook;

namespace Winook.Desktop.Core.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int RecentMessageCapacity = 20;

        private readonly ObservableCollection<string> _recentMessages = [];
        private MouseHook _mouseHook;
        private KeyboardHook _keyboardHook;
        private Process _process;
        private bool _mouseHookInstalled;
        private bool _keyboardHookInstalled;

        public MainWindow()
        {
            InitializeComponent();
            recentMessagesItemsControl.ItemsSource = _recentMessages;
            if (!Environment.Is64BitOperatingSystem)
            {
                radio64bit.IsEnabled = false;
            }
        }

        private async void MouseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_mouseHookInstalled)
            {
                await EnsureTargetProcessAsync();

                _mouseHook?.Dispose();
                if (ignoreMove.IsChecked ?? false)
                {
                    _mouseHook = new MouseHook(_process.Id, MouseMessageTypes.IgnoreMove);
                }
                else
                {
                    _mouseHook = new MouseHook(_process.Id);
                }

                ignoreMove.IsEnabled = false;
                _mouseHook.MessageReceived += MouseHook_MessageReceived;
                mouseButton.Content = "Installing hook...";
                await _mouseHook.InstallAsync();
                _mouseHookInstalled = true;
                mouseButton.Content = "Mouse Unhook";
            }
            else
            {
                _mouseHook.Uninstall();
                _mouseHookInstalled = false;
                mouseButton.Content = "Mouse Hook";
                ignoreMove.IsEnabled = true;
            }
        }

        private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AddRecentMessage("Mouse", FormatMouseMessage(e), e.SendTimestamp);
            }));
        }

        private async void KeyboardButton_Click(object sender, EventArgs e)
        {
            if (!_keyboardHookInstalled)
            {
                await EnsureTargetProcessAsync();

                _keyboardHook?.Dispose();
                _keyboardHook = new KeyboardHook(_process.Id);
                _keyboardHook.MessageReceived += KeyboardHook_MessageReceived;
                keyboardButton.Content = "Installing hook...";
                await _keyboardHook.InstallAsync();
                _keyboardHookInstalled = true;
                keyboardButton.Content = "Keyboard Unhook";
            }
            else
            {
                _keyboardHook.Uninstall();
                _keyboardHookInstalled = false;
                keyboardButton.Content = "Keyboard Hook";
            }
        }

        private void KeyboardHook_MessageReceived(object sender, KeyboardMessageEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                AddRecentMessage("Keyboard", FormatKeyboardMessage(e), e.SendTimestamp);
            }));
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

            _recentMessages.Insert(0, $"{receivedTimestamp:HH:mm:ss.fff} {hookType,-8} {sent}; {latency}; {message}");
            if (_recentMessages.Count > RecentMessageCapacity)
            {
                _recentMessages.RemoveAt(_recentMessages.Count - 1);
            }
        }

        private static string FormatMouseMessage(MouseMessageEventArgs e)
            => $"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Modifiers: {e.Modifiers:x}; "
                + $"Delta: {e.Delta}; XButtons: {e.XButtons}";

        private static string FormatKeyboardMessage(KeyboardMessageEventArgs e)
            => $"Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; "
                + $"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}";

        private async Task EnsureTargetProcessAsync()
        {
            if (_process != null && !_process.HasExited)
            {
                return;
            }

            var existingCharacterMapProcessIds = Process.GetProcessesByName("charmap")
                .Select(process => process.Id)
                .ToArray();

            var characterMapPath = Environment.Is64BitOperatingSystem && (radio32bit.IsChecked ?? false)
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
    }
}
