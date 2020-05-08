namespace Winook
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    public class MouseHook : Hook
    {
        #region Fields

        private const int HookMessageSizeInBytes = 24;
        private const int HookType = 7; // WH_MOUSE

        private const int LeftButtonUpMessageCode = 0x0202; // WM_LBUTTONUP

        #endregion

        #region Events

        public event EventHandler<MouseMessageEventArgs> MessageReceived;
        public event EventHandler<MouseMessageEventArgs> LeftButtonUp;

        #endregion

        #region Constructors

        public MouseHook(Process targetProcess)
            : base(targetProcess, HookType, HookMessageSizeInBytes)
        {
        }

        #endregion

        #region Methods

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            Contract.Requires(e != null);

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

            if (eventArgs.MessageCode == LeftButtonUpMessageCode)
            {
                LeftButtonUp?.Invoke(this, eventArgs);
            }

            MessageReceived?.Invoke(this, eventArgs);
        }

        #endregion
    }

#pragma warning disable CA1051 // Do not declare visible instance fields
    public class MouseMessageEventArgs : EventArgs
    {
        public int MessageCode;
        public int X;
        public int Y;
        public int Handle;
        public int HitTestCode;
        public short Delta;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
