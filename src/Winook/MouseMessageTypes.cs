namespace Winook
{
    using System;

    [Flags]
    public enum MouseMessageTypes
    {
        All = 0,
        Click = 1,
        Move = 2,
        Other = 4,
        NCClick = 8,
        NCMove = 16,
        NCOther = 32,
        IgnoreMove = Click | Other,
        NCIgnoreMove = NCClick | NCOther
    }
}
