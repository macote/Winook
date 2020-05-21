namespace Winook.Desktop.Test
{
    using System;
    using System.Diagnostics;
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

        private void mouseButton_Click(object sender, EventArgs e)
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

                _mouseHook = new MouseHook(_process.Id);
                _mouseHook.MessageReceived += MouseHook_MessageReceived;
                _mouseHook.Install();
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
        private void MouseHook_MessageReceived(object sender, MouseMessageEventArgs e)
        {
            mouseLabel.Invoke((MethodInvoker)delegate
            {
                mouseLabel.Text = $"Mouse Message Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Delta: {e.Delta}";
            });
        }

        private void keyboardButton_Click(object sender, EventArgs e)
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

                _keyboardHook = new KeyboardHook(_process.Id);
                _keyboardHook.MessageReceived += KeyboardHook_MessageReceived;
                _keyboardHook.Install();
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
                keyboardLabel.Text = $"Keyboard Virtual Key Code: {e.VirtualKeyCode}; Flags: {e.Flags:x}";
            });
        }
    }
}
