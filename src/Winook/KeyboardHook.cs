namespace Winook
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    public class KeyboardHook : HookBase
    {
        #region Fields

        private const int HookMessageSizeInBytes = 8;
        private const HookType KeyboardHookType = HookType.Keyboard; // WH_KEYBOARD

        #endregion

        #region Events

        public event EventHandler<KeyboardMessageEventArgs> MessageReceived;

        public delegate void KeyboardEventHandler(object sender, KeyboardMessageEventArgs e);

        #endregion

        #region Constructors

        public KeyboardHook(int processId)
            : base(processId, KeyboardHookType, HookMessageSizeInBytes)
        {
        }

        #endregion

        #region Methods

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            Contract.Requires(e != null);

            var eventArgs = new KeyboardMessageEventArgs
            {
                VirtualKeyCode = BitConverter.ToInt16(e.Bytes, 0),
                ShiftControlAltFlags = BitConverter.ToUInt16(e.Bytes, 2),
                Flags = BitConverter.ToUInt32(e.Bytes, 4),
            };

            Debug.WriteLine($"Keyboard Virtual Key Code: {eventArgs.VirtualKeyCode}; ShiftControlAlt: {eventArgs.ShiftControlAltFlags:x}; Flags: {eventArgs.Flags:x}");

            MessageReceived?.Invoke(this, eventArgs);
        }

        #endregion
    }
}
