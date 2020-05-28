namespace Winook
{
    public enum MouseMessageCode
    {
        MouseActivate = 0x0021, // WM_MOUSEACTIVATE
        NCHitTest = 0x0084, // WM_NCHITTEST
        NCMouseMove = 0x00A0, // WM_NCMOUSEMOVE
        NCLeftButtonDown = 0x00A1, // WM_NCLBUTTONDOWN
        NCLeftButtonUp = 0x00A2, // WM_NCLBUTTONUP
        NCLeftButtonDblClk = 0x00A3, // WM_NCLBUTTONDBLCLK
        NCRightButtonDown = 0x00A4, // WM_NCRBUTTONDOWN
        NCRightButtonUp = 0x00A5, // WM_NCRBUTTONUP
        NCRightButtonDblClk = 0x00A6, // WM_NCRBUTTONDBLCLK
        NCMiddleButtonDown = 0x00A7, // WM_NCMBUTTONDOWN
        NCMiddleButtonUp = 0x00A8, // WM_NCMBUTTONUP
        NCMiddleButtonDblClk = 0x00A9, // WM_NCMBUTTONDBLCLK
        MouseMove = 0x0200, // WM_MOUSEMOVE
        LeftButtonDown = 0x0201, // WM_LBUTTONDOWN
        LeftButtonUp = 0x0202, // WM_LBUTTONUP
        LeftButtonDblClk = 0x0203, // WM_LBUTTONDBLCLK
        RightButtonDown = 0x0204, // WM_RBUTTONDOWN
        RightButtonUp = 0x0205, // WM_RBUTTONUP
        RightButtonDblClk = 0x0206, // WM_RBUTTONDBLCLK
        MiddleButtonDown = 0x0207, // WM_MBUTTONDOWN
        MiddleButtonUp = 0x0208, // WM_MBUTTONUP
        MiddleButtonDblClk = 0x0209, // WM_MBUTTONDBLCLK
        MouseWheel = 0x020A, // WM_MOUSEWHEEL
        XButtonDown = 0x020B, // WM_XBUTTONDOWN
        XButtonUp = 0x020C, // WM_XBUTTONUP
        XButtonDblClk = 0x020D, // WM_XBUTTONDBLCLK
        MouseHWheel = 0x020E, // WM_MOUSEHWHEEL
        CaptureChanged = 0x0215, // WM_CAPTURECHANGED
        NCMouseHover = 0x02A0, // WM_NCMOUSEHOVER
        MouseHover = 0x02A1, // WM_MOUSEHOVER
        NCMouseLeave = 0x02A2, // WM_NCMOUSELEAVE
        MouseLeave = 0x02A3, // WM_MOUSELEAVE
    }
}
