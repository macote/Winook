namespace Winook
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    public class KeyboardHook : HookBase
    {
        #region Fields

        private const int HookMessageSizeInBytes = 8;
        private const HookType KeyboardHookType = HookType.Keyboard; // WH_KEYBOARD

        private readonly Dictionary<uint, KeyboardEventHandler> _messageHandlers = new Dictionary<uint, KeyboardEventHandler>();

        private bool _disposed = false;

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

        public void AddHandler(KeyCode keyCode, KeyboardEventHandler handler)
            => AddHandler((ushort)keyCode, KeyDirection.Up, Modifiers.None, handler);

        public void AddHandler(ushort keyValue, KeyboardEventHandler handler)
            => AddHandler(keyValue, KeyDirection.Up, Modifiers.None, handler);

        public void AddHandler(KeyCode keyCode, Modifiers modifiers, KeyboardEventHandler handler)
            => AddHandler((ushort)keyCode, KeyDirection.Up, modifiers, handler);
        public void AddHandler(ushort keyValue, Modifiers modifiers, KeyboardEventHandler handler)
            => AddHandler(keyValue, KeyDirection.Up, modifiers, handler);

        public void AddHandler(KeyCode keyCode, KeyDirection direction, Modifiers modifiers, KeyboardEventHandler handler)
            => AddHandler((ushort)keyCode, direction, modifiers, handler);

        public void AddHandler(ushort keyValue, KeyDirection direction, Modifiers modifiers, KeyboardEventHandler handler)
        {
            foreach (var key in GetHandlerKeys(keyValue, modifiers, direction))
            {
                AddHandlerInternal(key, handler ?? throw new ArgumentNullException(nameof(handler)));
            }
        }

        public void RemoveHandler(KeyCode keyCode, KeyboardEventHandler handler)
            => RemoveHandler((ushort)keyCode, Modifiers.None, KeyDirection.Up, handler);

        public void RemoveHandler(ushort keyValue, KeyboardEventHandler handler)
            => RemoveHandler(keyValue, Modifiers.None, KeyDirection.Up, handler);

        public void RemoveHandler(KeyCode keyCode, Modifiers modifiers, KeyDirection direction, KeyboardEventHandler handler)
            => RemoveHandler((ushort)keyCode, modifiers, direction, handler);

        public void RemoveHandler(ushort keyValue, Modifiers modifiers, KeyDirection direction, KeyboardEventHandler handler)
        {
            foreach (var key in GetHandlerKeys(keyValue, modifiers, direction))
            {
                RemoveHandlerInternal(key, handler ?? throw new ArgumentNullException(nameof(handler)));
            }
        }

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            Contract.Requires(e != null);

            var keyValue = BitConverter.ToUInt16(e.Bytes, 0);
            var modifiers = BitConverter.ToUInt16(e.Bytes, 2);
            var flags = BitConverter.ToUInt32(e.Bytes, 4);
            var pressed = (flags & 0x80000000) == 0;
            var eventArgs = new KeyboardMessageEventArgs
            {
                KeyValue = keyValue,
                Modifiers = modifiers,
                Flags = flags,
                Shift = (modifiers & 0b100) > 0,
                Control = (modifiers & 0b10) > 0,
                Alt = (modifiers & 0b1) > 0,
                Direction = pressed ? KeyDirection.Down : KeyDirection.Up,
            };

            Debug.Write($"Code: {eventArgs.KeyValue}; Modifiers: {eventArgs.Modifiers:x}; Flags: {eventArgs.Flags:x}; ");
            Debug.WriteLine($"Shift: {eventArgs.Shift}; Control: {eventArgs.Control}; Alt: {eventArgs.Alt}; Direction: {eventArgs.Direction}");

            MessageReceived?.Invoke(this, eventArgs);

            uint handlerKey = GetHandlerKey(keyValue, modifiers, pressed);
            if (_messageHandlers.ContainsKey(handlerKey))
            {
                _messageHandlers[handlerKey]?.Invoke(this, eventArgs);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                MessageReceived = null;
                foreach (var key in _messageHandlers.Keys)
                {
                    _messageHandlers[key] = null;
                }
            }

            _disposed = true;

            base.Dispose(disposing);
        }

        private void AddHandlerInternal(uint key, KeyboardEventHandler handler)
        {
            if (!_messageHandlers.ContainsKey(key))
            {
                _messageHandlers.Add(key, null);
            }

            _messageHandlers[key] += handler;
        }

        private void RemoveHandlerInternal(uint key, KeyboardEventHandler handler)
        {
            if (_messageHandlers.ContainsKey(key))
            {
                _messageHandlers[key] -= handler;
            }
        }

        /// <summary>
        /// Returns a key that can match an entry in the handlers dictionary.
        /// Bits format: 
        /// [31..26]: unused
        /// [25]: 1 when key is being pressed
        /// [24..22]: left shift, control and alt (1 = pressed)
        /// [21..19]: right shift, control and alt (1 = pressed)
        /// [18..16]: shift, control and alt (left or right, 1 = pressed)
        /// [15..0]: virtual key code value
        /// Modifiers that are not side sensitive are optionally applied.
        /// </summary>
        /// <param name="keyValue"></param>
        /// <param name="modifiers"></param>
        /// <param name="pressed"></param>
        /// <param name="insensitiveModifiers"></param>
        /// <returns></returns>
        private static uint GetHandlerKey(ushort keyValue, ushort modifiers, bool pressed, ushort insensitiveModifiers = 0)
            => keyValue | (((uint)modifiers & 0b111_111_000 | insensitiveModifiers | (pressed ? (uint)1 : 0) << 9) << 16);

        private static IEnumerable<uint> GetHandlerKeys(ushort keyValue, Modifiers modifiers, KeyDirection direction)
            => GetHandlerKeys(keyValue, (ushort)modifiers, direction);

        private static IEnumerable<uint> GetHandlerKeys(ushort keyValue, ushort modifiers, KeyDirection direction)
        {
            var keys = new HashSet<uint>();
            if ((modifiers & 0b111) > 0)
            {
                // Add handlers for side insensitive modifiers
                var left = (ushort)((modifiers & 0b111) << 6);
                var right = (ushort)((modifiers & 0b111) << 3);
                var both = (ushort)(left | right);
                if (direction == KeyDirection.Any | direction == KeyDirection.Up)
                {
                    keys.Add(GetHandlerKey(keyValue, modifiers, false, left));
                    keys.Add(GetHandlerKey(keyValue, modifiers, false, right));
                    keys.Add(GetHandlerKey(keyValue, modifiers, false, both));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Down)
                {
                    keys.Add(GetHandlerKey(keyValue, modifiers, true, left));
                    keys.Add(GetHandlerKey(keyValue, modifiers, true, right));
                    keys.Add(GetHandlerKey(keyValue, modifiers, true, both));
                }
            }
            else
            {
                // Add handlers for side sensitive modifiers and unmodified (single) keys
                if (direction == KeyDirection.Any | direction == KeyDirection.Up)
                {
                    keys.Add(GetHandlerKey(keyValue, modifiers, false));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Down)
                {
                    keys.Add(GetHandlerKey(keyValue, modifiers, true));
                }
            }

            return keys;
        }

        #endregion
    }
}
