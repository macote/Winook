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

        private Dictionary<uint, KeyboardEventHandler> _messageHandlers = new Dictionary<uint, KeyboardEventHandler>();

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

        public void AddHandler(ushort keyCode, KeyboardEventHandler handler)
            => AddHandler(keyCode, KeyDirection.Up, HotKeyModifiers.None, handler);

        public void AddHandler(ushort keyCode, HotKeyModifiers modifiers, KeyboardEventHandler handler)
            => AddHandler(keyCode, KeyDirection.Up, modifiers, handler);

        public void AddHandler(ushort keyCode, KeyDirection direction, HotKeyModifiers modifiers, KeyboardEventHandler handler)
        {
            var handlerKeys = GetHandlerKeys(keyCode, modifiers, direction);
            foreach (var key in handlerKeys)
            {
                AddHandlerInternal(key, handler ?? throw new ArgumentNullException(nameof(handler)));
            }
        }

        public void RemoveHandler(ushort keyCode, KeyboardEventHandler handler)
            => RemoveHandler(keyCode, HotKeyModifiers.None, KeyDirection.Up, handler);

        public void RemoveHandler(ushort keyCode, HotKeyModifiers modifiers, KeyDirection direction, KeyboardEventHandler handler)
        {
            var handlerKeys = GetHandlerKeys(keyCode, modifiers, direction);
            foreach (var key in handlerKeys)
            {
                RemoveHandlerInternal(key, handler ?? throw new ArgumentNullException(nameof(handler)));
            }
        }

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            Contract.Requires(e != null);

            var eventArgs = new KeyboardMessageEventArgs
            {
                KeyCode = BitConverter.ToUInt16(e.Bytes, 0),
                Modifiers = BitConverter.ToUInt16(e.Bytes, 2),
                Flags = BitConverter.ToUInt32(e.Bytes, 4),
            };

            Debug.WriteLine($"Keyboard Virtual Key Code: {eventArgs.KeyCode}; Modifiers: {eventArgs.Modifiers:x}; Flags: {eventArgs.Flags:x}");

            MessageReceived?.Invoke(this, eventArgs);

            uint handlerKey = GetHandlerKey(eventArgs.KeyCode, eventArgs.Modifiers, (eventArgs.Flags & 0x80000000) == 0);
            if (_messageHandlers.ContainsKey(handlerKey))
            {
                _messageHandlers[handlerKey]?.Invoke(this, eventArgs);
            }
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
        /// [15..0]: virtual key code
        /// Modifiers that are not side sensitive are applied through the extended modifiers parameter.
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="modifiers"></param>
        /// <param name="pressed"></param>
        /// <param name="extendedModifiers"></param>
        /// <returns></returns>
        private static uint GetHandlerKey(ushort keyCode, ushort modifiers, bool pressed, ushort extendedModifiers = 0)
            => keyCode | (((uint)modifiers & 0b111_111_000 | extendedModifiers | (pressed ? (uint)1 : 0) << 9) << 16);

        private static IEnumerable<uint> GetHandlerKeys(ushort keyCode, HotKeyModifiers modifiers, KeyDirection direction)
            => GetHandlerKeys(keyCode, (ushort)modifiers, direction);

        private static IEnumerable<uint> GetHandlerKeys(ushort keyCode, ushort modifiers, KeyDirection direction)
        {
            var keys = new HashSet<uint>();
            if ((modifiers & 0b111) > 0)
            {
                var left = (ushort)((modifiers & 0b111) << 6);
                var right = (ushort)((modifiers & 0b111) << 3);
                var both = (ushort)(left | right);
                if (direction == KeyDirection.Any | direction == KeyDirection.Up)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, false, left));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Down)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, true, left));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Up)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, false, right));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Down)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, true, right));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Up)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, false, both));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Down)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, true, both));
                }
            }
            else if ((modifiers & 0b111_111_000) > 0)
            {
                if (direction == KeyDirection.Any | direction == KeyDirection.Up)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, false));
                }

                if (direction == KeyDirection.Any | direction == KeyDirection.Down)
                {
                    keys.Add(GetHandlerKey(keyCode, modifiers, true));
                }
            }

            return keys;
        }

        #endregion
    }
}
