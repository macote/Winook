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

        public void AddHandler(ushort virtualKeyCode, KeyboardHotKeyModifiers hotKey, KeyboardEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (hotKey == KeyboardHotKeyModifiers.None)
            {
                if (!_messageHandlers.ContainsKey(virtualKeyCode))
                {
                    _messageHandlers.Add(virtualKeyCode, null);
                }

                _messageHandlers[virtualKeyCode] += handler;
            }
            else
            {
                var handlerKeys = GetHandlerKeys(virtualKeyCode, hotKey);
                foreach (var key in handlerKeys)
                {
                    if (!_messageHandlers.ContainsKey(key))
                    {
                        _messageHandlers.Add(key, null);
                    }

                    _messageHandlers[key] += handler;
                }
            }
        }

        public void RemoveHandler(ushort virtualKeyCode, KeyboardHotKeyModifiers hotKey, KeyboardEventHandler handler)
        {
            if (hotKey == KeyboardHotKeyModifiers.None)
            {
                if (_messageHandlers.ContainsKey(virtualKeyCode))
                {
                    _messageHandlers[virtualKeyCode] -= handler ?? throw new ArgumentNullException(nameof(handler));
                }
            }
            else
            {
                var handlerKeys = GetHandlerKeys(virtualKeyCode, hotKey);
                foreach (var key in handlerKeys)
                {
                    if (_messageHandlers.ContainsKey(key))
                    {
                        _messageHandlers[key] -= handler ?? throw new ArgumentNullException(nameof(handler));
                    }
                }
            }
        }

        protected override void OnMessageReceived(object sender, MessageEventArgs e)
        {
            Contract.Requires(e != null);

            var eventArgs = new KeyboardMessageEventArgs
            {
                VirtualKeyCode = BitConverter.ToUInt16(e.Bytes, 0),
                ShiftControlAltFlags = BitConverter.ToUInt16(e.Bytes, 2),
                Flags = BitConverter.ToUInt32(e.Bytes, 4),
            };

            Debug.WriteLine($"Keyboard Virtual Key Code: {eventArgs.VirtualKeyCode}; ShiftControlAlt: {eventArgs.ShiftControlAltFlags:x}; Flags: {eventArgs.Flags:x}");

            MessageReceived?.Invoke(this, eventArgs);

            uint handlerKey = GetHandlerKey(eventArgs.VirtualKeyCode, eventArgs.ShiftControlAltFlags);
            if (_messageHandlers.ContainsKey(handlerKey))
            {
                _messageHandlers[handlerKey]?.Invoke(this, eventArgs);
            }
        }

        private static uint GetHandlerKey(ushort virtualKeyCode, KeyboardHotKeyModifiers hotKeyModifiers)
            => GetHandlerKey(virtualKeyCode, (ushort)hotKeyModifiers);

        private static uint GetHandlerKey(ushort virtualKeyCode, ushort hotKeyModValue, ushort applyMod = 0)
            => virtualKeyCode | (((uint)hotKeyModValue & 0b111_111_000 | applyMod) << 16);

        private static IEnumerable<uint> GetHandlerKeys(ushort virtualKeyCode, KeyboardHotKeyModifiers hotKeyModifiers)
            => GetHandlerKeys(virtualKeyCode, (ushort)hotKeyModifiers);

        private static IEnumerable<uint> GetHandlerKeys(ushort virtualKeyCode, ushort hotKeyModValue)
        {
            var keys = new HashSet<uint>();
            if ((hotKeyModValue & 0b100) > 0)
            {
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b100_000_000));
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b000_100_000));
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b100_100_000));
            }

            if ((hotKeyModValue & 0b010) > 0)
            {
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b010_000_000));
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b000_010_000));
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b010_010_000));
            }

            if ((hotKeyModValue & 0b001) > 0)
            {
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b001_000_000));
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b000_001_000));
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue, 0b001_001_000));
            }

            if ((hotKeyModValue & 0b111_111_000) > 0)
            {
                keys.Add(GetHandlerKey(virtualKeyCode, hotKeyModValue));
            }

            return keys;
        }

        #endregion
    }
}
