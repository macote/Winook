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

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var process = Process.Start(@"c:\windows\notepad.exe");
            //var process = Process.Start(@"c:\windows\syswow64\notepad.exe");
            Task.Delay(222).GetAwaiter().GetResult();

            _mouseHook = new MouseHook(process);
            _mouseHook.MouseMessageReceived += MouseHook_MouseMessageReceived;
            _mouseHook.Install();
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
