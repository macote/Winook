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
        }

        private void mouseButton_Click(object sender, EventArgs e)
        {
            if (!_mouseHookInstalled)
            {
                if (_process == null || _process.HasExited)
                {
                    _process = Process.Start(@"c:\windows\notepad.exe");
                    //var process = Process.Start(@"c:\windows\syswow64\notepad.exe");
                    _process.WaitForInputIdle();
                }

                _mouseHook = new MouseHook(_process);
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
                if (_process == null)
                {
                    _process = Process.Start(@"c:\windows\notepad.exe");
                    //var process = Process.Start(@"c:\windows\syswow64\notepad.exe");
                    Task.Delay(222).GetAwaiter().GetResult();
                }

                _keyboardHook = new KeyboardHook(_process);
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
                keyboardLabel.Text = $"Keyboard Virtual Key Code: {e.VirtualKeyCode}; Flags: {e.Flags}";
            });
        }
    }
}
