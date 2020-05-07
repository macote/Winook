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
        private Process _process;
        private bool _mouseHookInstalled;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!_mouseHookInstalled)
            {
                if (_process == null)
                {
                    _process = Process.Start(@"c:\windows\notepad.exe");
                    //var process = Process.Start(@"c:\windows\syswow64\notepad.exe");
                    Task.Delay(222).GetAwaiter().GetResult();
                }

                _mouseHook = new MouseHook(_process);
                _mouseHook.MouseMessageReceived += MouseHook_MouseMessageReceived;
                _mouseHook.Install();
                _mouseHookInstalled = true;
                button1.Text = "Unhook";
            }
            else
            {
                _mouseHook.Uninstall();
                _mouseHookInstalled = false;
                button1.Text = "Hook";
            }
        }

        private void MouseHook_MouseMessageReceived(object sender, MouseMessageEventArgs e)
        {
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = $"Mouse Message Code: {e.MessageCode}; X: {e.X}; Y: {e.Y}; Delta: {e.Delta}";
            });
        }
    }
}
