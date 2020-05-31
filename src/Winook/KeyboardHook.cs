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
            => AddHandler(keyCode, HotKeyModifiers.None, handler);

        public void AddHandler(ushort keyCode, HotKeyModifiers modifiers, KeyboardEventHandler handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (modifiers == HotKeyModifiers.None)
            {
                if (!_messageHandlers.ContainsKey(keyCode))
                {
                    _messageHandlers.Add(keyCode, null);
                }

                _messageHandlers[keyCode] += handler;
            }
            else
            {
                var handlerKeys = GetHandlerKeys(keyCode, modifiers);
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

        public void RemoveHandler(ushort keyCode, KeyboardEventHandler handler)
            => RemoveHandler(keyCode, HotKeyModifiers.None, handler);

        public void RemoveHandler(ushort keyCode, HotKeyModifiers hotKey, KeyboardEventHandler handler)
        {
            if (hotKey == HotKeyModifiers.None)
            {
                if (_messageHandlers.ContainsKey(keyCode))
                {
                    _messageHandlers[keyCode] -= handler ?? throw new ArgumentNullException(nameof(handler));
                }
            }
            else
            {
                var handlerKeys = GetHandlerKeys(keyCode, hotKey);
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
                KeyCode = BitConverter.ToUInt16(e.Bytes, 0),
                Modifiers = BitConverter.ToUInt16(e.Bytes, 2),
                Flags = BitConverter.ToUInt32(e.Bytes, 4),
            };

            Debug.WriteLine($"Keyboard Virtual Key Code: {eventArgs.KeyCode}; Modifiers: {eventArgs.Modifiers:x}; Flags: {eventArgs.Flags:x}");

            MessageReceived?.Invoke(this, eventArgs);

            uint handlerKey = GetHandlerKey(eventArgs.KeyCode, eventArgs.Modifiers);
            if (_messageHandlers.ContainsKey(handlerKey))
            {
                _messageHandlers[handlerKey]?.Invoke(this, eventArgs);
            }
        }

        private static uint GetHandlerKey(ushort keyCode, HotKeyModifiers modifiers)
            => GetHandlerKey(keyCode, (ushort)modifiers);

        private static uint GetHandlerKey(ushort keyCode, ushort hotKeyModValue, ushort applyMod = 0)
            => keyCode | (((uint)hotKeyModValue & 0b111_111_000 | applyMod) << 16);

        private static IEnumerable<uint> GetHandlerKeys(ushort keyCode, HotKeyModifiers modifiers)
            => GetHandlerKeys(keyCode, (ushort)modifiers);

        private static IEnumerable<uint> GetHandlerKeys(ushort keyCode, ushort modValue)
        {
            var keys = new HashSet<uint>();
            if ((modValue & 0b111) > 0)
            {
                var left = (ushort)((modValue & 0b111) << 6);
                var right = (ushort)((modValue & 0b111) << 3);
                var both = (ushort)(left | right);
                keys.Add(GetHandlerKey(keyCode, modValue, left));
                keys.Add(GetHandlerKey(keyCode, modValue, right));
                keys.Add(GetHandlerKey(keyCode, modValue, both));
            }

            if ((modValue & 0b111_111_000) > 0)
            {
                keys.Add(GetHandlerKey(keyCode, modValue));
            }

            return keys;
        }

        #endregion
    }
}
