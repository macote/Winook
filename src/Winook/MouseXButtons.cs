namespace Winook
{
    using System;

    [Flags]
    public enum MouseXButtons
    {
        Button1 = 1,
        Button2 = 2,
        AllButtons = Button1 | Button2,
    }
}
