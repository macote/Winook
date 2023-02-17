namespace Winook.Desktop.Test
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Winook;

    public partial class Form1 : Form
    {
        private MouseHook _mouseHook;
        private KeyboardHook _keyboardHook;
        private Process _process;
        private bool _mouseHookInstalled;
        private bool _keyboardHookInstalled;

        public Form1()
        {
            InitializeComponent();
            if (!Environment.Is64BitOperatingSystem)
            {
                radio64bit.Enabled = false;
            }
        }

        private async void mouseButton_Click(object sender, EventArgs e)
        {
            if (!_mouseHookInstalled)
            {
                if (_process == null || _process.HasExited)
                {
                    if (Environment.Is64BitOperatingSystem && radio32bit.Checked)
                    {
                        _process = Process.Start(@"c:\windows\syswow64\notepad.exe");
                    }
                    else
                    {
                        _process = Process.Start(@"c:\windows\notepad.exe");
                    }
                }

                await Task.Delay(1000); // wait a bit for app to show its window

                if (ignoreMove.Checked)
                {
                    _mouseHook = new MouseHook(_process.Id, MouseMessageTypes.IgnoreMove);
                }
                else
                {
                    _mouseHook = new MouseHook(_process.Id);
                }

                _mouseHook.MessageReceived += MouseHook_MessageReceived;
                _mouseHook.LeftButtonUp += MouseHook_LeftButtonUp;
                _mouseHook.AddHandler(MouseMessageCode.NCLeftButtonUp, MouseHook_NCLButtonUp);
                _mouseHook.RemoveHandler(MouseMessageCode.NCLeftButtonUp, MouseHook_NCLButtonUp);
                mouseButton.Text = "Installing hook...";
                await _mouseHook.InstallAsync();
                _mouseHookInstalled = true;
                mouseButton.Text = "Mouse Unhook";
            }
            else
            {
                _mouseHook.Uninstall();
                _mouseHookInstalled = false;
                mouseButton.Text = "Mouse Hook";
            }
        }

        private void MouseHook_LeftButtonUp(object sender, MouseMessageEventArgs e)
        {
            testLabel.Invoke((MethodInvoker)delegate
            {
                testLabel.Text = $"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; "
                    + $"Modifiers: {e.Modifiers:x}; Delta: {e.Delta}; XButtons: {e.XButtons}";
            });
        }

        private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
        {
            mouseLabel.Invoke((MethodInvoker)delegate
            {
                mouseLabel.Text = $"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; "
                    + $"Modifiers: {e.Modifiers:x}; Delta: {e.Delta}; XButtons: {e.XButtons}";
            });
        }

        private void MouseHook_NCLButtonUp(object sender, MouseMessageEventArgs e)
        {
            testLabel.Invoke((MethodInvoker)delegate
            {
                testLabel.Text = $"Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; "
                    + $"Modifiers: {e.Modifiers:x}; Delta: {e.Delta}; XButtons: {e.XButtons}";
            });
        }

        private async void keyboardButton_Click(object sender, EventArgs e)
        {
            if (!_keyboardHookInstalled)
            {
                if (_process == null || _process.HasExited)
                {
                    if (Environment.Is64BitOperatingSystem && radio32bit.Checked)
                    {
                        _process = Process.Start(@"c:\windows\syswow64\notepad.exe");
                    }
                    else
                    {
                        _process = Process.Start(@"c:\windows\notepad.exe");
                    }
                }

                await Task.Delay(1000); // wait a bit for app to show its window

                _keyboardHook = new KeyboardHook(_process.Id);
                _keyboardHook.MessageReceived += KeyboardHook_MessageReceived;
                _keyboardHook.AddHandler(KeyCode.F, KeyboardHook_Test);
                _keyboardHook.AddHandler(KeyCode.F, Modifiers.Shift, KeyboardHook_Test);
                _keyboardHook.AddHandler(KeyCode.Y, Modifiers.ControlShift, KeyboardHook_Test);
                _keyboardHook.AddHandler(KeyCode.U, Modifiers.Shift | Modifiers.RightControl, KeyboardHook_Test);
                _keyboardHook.AddHandler(KeyCode.N, Modifiers.AltControlShift, KeyboardHook_Test);
                _keyboardHook.AddHandler(KeyCode.T, KeyboardHook_Test);
                keyboardButton.Text = "Installing hook...";
                await _keyboardHook.InstallAsync();
                _keyboardHookInstalled = true;
                keyboardButton.Text = "Keyboard Unhook";
            }
            else
            {
                _keyboardHook.Uninstall();
                _keyboardHookInstalled = false;
                keyboardButton.Text = "Keyboard Hook";
            }
        }

        private void KeyboardHook_MessageReceived(object sender, KeyboardMessageEventArgs e)
        {
            keyboardLabel.Invoke((MethodInvoker)delegate
            {
                keyboardLabel.Text = $"Code: {e.KeyValue}; Modifiers: {e.Modifiers}; Flags: {e.Flags:x} "
                    + $"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}";
            });
        }

        private void KeyboardHook_Test(object sender, KeyboardMessageEventArgs e)
        {
            Debug.Write($"KeyboardHook_Test - Code: {e.KeyValue}; Modifiers: {e.Modifiers:x}; Flags: {e.Flags:x}; ");
            Debug.WriteLine($"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}");
            testLabel.Invoke((MethodInvoker)delegate
            {
                testLabel.Text = $"Code: {e.KeyValue}; Modifiers: {e.Modifiers}; Flags: {e.Flags:x} "
                    + $"Shift: {e.Shift}; Control: {e.Control}; Alt: {e.Alt}; Direction: {e.Direction}";
            });
        }
    }
}
