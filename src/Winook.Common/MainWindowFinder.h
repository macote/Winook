#pragma once

#include <Windows.h>

class MainWindowFinder
{
public:
    MainWindowFinder(DWORD processid) : processid_(processid) { }
    HWND FindMainWindow()
    {
        if (!EnumWindows(EnumWindowsCallback, (LONG_PTR)this))
        {
            lasterror_ = GetLastError();
        }

        return besthandle_;
    }
    DWORD lasterror() const { return lasterror_; }
private:
    static BOOL CALLBACK EnumWindowsCallback(HWND handle, LONG_PTR mwf)
    {
        auto self = reinterpret_cast<MainWindowFinder*>(mwf);
        DWORD processid{};
        GetWindowThreadProcessId(handle, &processid);
        if (processid == self->processid_ && MainWindowFinder::IsMainWindow(handle))
        {
            self->besthandle_ = handle;

            return FALSE;
        }

        return TRUE;
    }
    static BOOL IsMainWindow(HWND handle)
    {
        return GetWindow(handle, GW_OWNER) == (HWND)0 && IsWindowVisible(handle);
    }
private:
    DWORD processid_{};
    HWND besthandle_{ NULL };
    DWORD lasterror_{};
};