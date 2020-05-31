namespace Winook
{
    using System;

    [Flags]
    public enum HotKeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        RightAlt = 8,
        RightControl = 16,
        RightShift = 32,
        LeftAlt = 64,
        LeftControl = 128,
        LeftShift = 256,
        AltControl = Alt | Control,
        AltShift = Alt | Shift,
        ControlShift = Control | Shift,
        ShiftControlAlt = Shift | Control | Alt,
    }
}
