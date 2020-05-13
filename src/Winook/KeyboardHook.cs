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

        #endregion

        #region Constructors

        public KeyboardHook(int processId)
            : this(processId, ProcessBitness.Unknown)
        {
        }

        public KeyboardHook(int processId, ProcessBitness processBitness)
            : base(processId, processBitness, KeyboardHookType, HookMessageSizeInBytes)
        {
        }

        #endregion

        #region Methods

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            Contract.Requires(e != null);

            var eventArgs = new KeyboardMessageEventArgs
            {
                VirtualKeyCode = BitConverter.ToInt32(e.Bytes, 0),
                Flags = BitConverter.ToInt32(e.Bytes, 4),
            };

            Debug.WriteLine($"Keyboard Virtual Key Code: {eventArgs.VirtualKeyCode}; Flags: {eventArgs.Flags}");

            MessageReceived?.Invoke(this, eventArgs);
        }

        #endregion
    }
}
