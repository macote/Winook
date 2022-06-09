#include "Lib.h"
#include "Winook.h"
#include "MessageSender.h"

#if _DEBUG
#include <bitset>
#endif
#include <fstream>
#include <string>
#include <regex>

#if !defined(__MINGW32__)
#pragma comment (lib, "shlwapi.lib")
#endif

#define LOGWINOOKLIB 1
#if _DEBUG && LOGWINOOKLIB
#define LOGWINOOKLIBPATH TEXT("C:\\Temp\\WinookLibHookProc_")
#include "DebugHelper.h"
#include "TimestampLogger.h"
TimestampLogger Logger = TimestampLogger(LOGWINOOKLIBPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

asio::io_context io_context;
MessageSender messagesender(io_context); 
WORD mousemessagetypes;

BOOL WINAPI DllMain(HINSTANCE hinst, DWORD fdwReason, LPVOID lpReserved)
{
    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_PROCESS_ATTACH"));
#endif
        if (!Initialize(hinst))
        {
            return FALSE;
        }

        break;

    case DLL_THREAD_ATTACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_THREAD_ATTACH"));
#endif
        break;

    case DLL_THREAD_DETACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_THREAD_DETACH"));
#endif
        break;

    case DLL_PROCESS_DETACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_PROCESS_DETACH"));
#endif
        break;
    }

    return TRUE;
}

BOOL Initialize(HINSTANCE hinst)
{
    // Look for initialization file stored in %TEMP%

    HMODULE module;
    if (!GetModuleHandleEx(
        GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
        (LPCTSTR)Initialize,
        &module))
    {
        return FALSE;
    }

    TCHAR dllfilepath[kPathBufferSize];
    GetModuleFileName(module, dllfilepath, kPathBufferSize);
    int hooktype{};
    if (StrStrI(dllfilepath, kKeyboardHookLibName.c_str()) != NULL)
    {
        hooktype = WH_KEYBOARD;
    }
    else if (StrStrI(dllfilepath, kMouseHookLibName.c_str()) != NULL)
    {
        hooktype = WH_MOUSE;
    }
    else if (StrStrI(dllfilepath, kGetMsgHookLibName.c_str()) != NULL)
    {
        hooktype = WH_GETMESSAGE;
    }
    else
    {
        return FALSE; // Unsupported hook type
    }

    const auto configfilepath = Winook::FindConfigFilePath(hooktype);
    if (configfilepath.empty())
    {
        return TRUE; // Assume out-of-context initialization
    }

    std::ifstream configfile(configfilepath.c_str());
    std::string port;
    configfile >> port;
    if (hooktype == WH_MOUSE)
    {
        configfile >> mousemessagetypes;
    }

    configfile.close();
    DeleteFile(configfilepath.c_str());
    messagesender.Connect(port);

    return TRUE;
}

WORD GetShiftCtrlAltState()
{
    const auto lshift = GetKeyState(VK_LSHIFT);
    const auto lcontrol = GetKeyState(VK_LCONTROL);
    const auto lalt = GetKeyState(VK_LMENU);
    const auto rshift = GetKeyState(VK_RSHIFT);
    const auto rcontrol = GetKeyState(VK_RCONTROL);
    const auto ralt = GetKeyState(VK_RMENU);
    return (lshift < 0) << kLeftShiftBitIndex
        | (lcontrol < 0) << kLeftControlBitIndex
        | (lalt < 0) << kLeftAltBitIndex
        | (rshift < 0) << kRightShiftBitIndex
        | (rcontrol < 0) << kRightControlBitIndex
        | (ralt < 0) << kRightAltBitIndex
        | (lshift < 0 || rshift < 0) << kShiftBitIndex
        | (lcontrol < 0 || rcontrol < 0) << kControlBitIndex
        | (lalt < 0 || ralt < 0) << kAltBitIndex;
}

LRESULT CALLBACK KeyboardHookProc(int code, WPARAM wParam, LPARAM lParam)
{
#if _DEBUG
    LogDll(DebugHelper::FormatKeyboardHookMessage(code, wParam, lParam));
#endif
    if (code == HC_ACTION)
    {
        HookKeyboardMessage hkm{};
        hkm.keyCode = (WORD)wParam;
        hkm.modifiers = GetShiftCtrlAltState();
        hkm.flags = (DWORD)lParam;
        messagesender.SendMessage(&hkm, sizeof(HookKeyboardMessage));
    }

    return CallNextHookEx(NULL, code, wParam, lParam);
}

LRESULT CALLBACK MouseHookProc(int code, WPARAM wParam, LPARAM lParam) 
{
#if _DEBUG
    LogDll(DebugHelper::FormatMouseHookMessage(code, wParam, lParam));
#endif
    if (code == HC_ACTION)
    {
        DWORD messagecode = (DWORD)wParam;
        WORD messagetype = (messagecode == WM_NCHITTEST
                || messagecode == WM_NCMOUSEHOVER
                || messagecode == WM_NCMOUSELEAVE) << 5
            | (messagecode == WM_NCMOUSEMOVE) << 4
            | (messagecode == WM_NCLBUTTONDOWN
                || messagecode == WM_NCLBUTTONUP
                || messagecode == WM_NCLBUTTONDBLCLK
                || messagecode == WM_NCRBUTTONDOWN
                || messagecode == WM_NCRBUTTONUP
                || messagecode == WM_NCRBUTTONDBLCLK
                || messagecode == WM_NCMBUTTONDOWN
                || messagecode == WM_NCMBUTTONUP
                || messagecode == WM_NCMBUTTONDBLCLK
                || messagecode == WM_NCXBUTTONDOWN
                || messagecode == WM_NCXBUTTONUP
                || messagecode == WM_NCXBUTTONDBLCLK) << 3
            | (messagecode == WM_MOUSEACTIVATE
                || messagecode == WM_MOUSEWHEEL
                || messagecode == WM_MOUSEHWHEEL
                || messagecode == WM_CAPTURECHANGED
                || messagecode == WM_MOUSEHOVER
                || messagecode == WM_MOUSELEAVE) << 2
            | (messagecode == WM_MOUSEMOVE) << 1
            | (messagecode == WM_LBUTTONDOWN
                || messagecode == WM_LBUTTONUP
                || messagecode == WM_LBUTTONDBLCLK
                || messagecode == WM_RBUTTONDOWN
                || messagecode == WM_RBUTTONUP
                || messagecode == WM_RBUTTONDBLCLK
                || messagecode == WM_MBUTTONDOWN
                || messagecode == WM_MBUTTONUP
                || messagecode == WM_MBUTTONDBLCLK
                || messagecode == WM_XBUTTONDOWN
                || messagecode == WM_XBUTTONUP
                || messagecode == WM_XBUTTONDBLCLK);
#if _DEBUG
        std::bitset<16> mousemessagetypesbits(mousemessagetypes);
        std::bitset<16> messagetypebits(messagetype);
        LogDll(std::string("mousemessagetypesbits: ") + mousemessagetypesbits.to_string());
        LogDll(std::string("messagetypebits: ") + messagetypebits.to_string());
#endif
        if (mousemessagetypes == 0 || (mousemessagetypes & messagetype) > 0)
        {
            HookMouseMessage hmm{};
            hmm.messageCode = messagecode;
            hmm.modifiers = GetShiftCtrlAltState();
            if (wParam == WM_MOUSEWHEEL
                || wParam == WM_XBUTTONDOWN
                || wParam == WM_XBUTTONUP
                || wParam == WM_XBUTTONDBLCLK
                || wParam == WM_NCXBUTTONDOWN
                || wParam == WM_NCXBUTTONUP
                || wParam == WM_NCXBUTTONDBLCLK)
            {
                auto pmhsx = (PMOUSEHOOKSTRUCTEX)lParam;
                hmm.pointX = pmhsx->pt.x;
                hmm.pointY = pmhsx->pt.y;
                hmm.hwnd = (DWORD)PtrToInt(pmhsx->hwnd);
                hmm.hitTestCode = (DWORD)pmhsx->wHitTestCode;
                hmm.extra = HIWORD(pmhsx->mouseData);
            }
            else
            {
                auto pmhs = (PMOUSEHOOKSTRUCT)lParam;
                hmm.pointX = pmhs->pt.x;
                hmm.pointY = pmhs->pt.y;
                hmm.hwnd = (DWORD)PtrToInt(pmhs->hwnd);
                hmm.hitTestCode = (DWORD)pmhs->wHitTestCode;
            }

            messagesender.SendMessage(&hmm, sizeof(HookMouseMessage));
        }
    }

    return CallNextHookEx(NULL, code, wParam, lParam);
}

LRESULT CALLBACK GetMsgHookProc(int code, WPARAM wParam, LPARAM lParam)
{
#if _DEBUG
    LogDll(DebugHelper::FormatGetMsgHookMessage(code, wParam, lParam));
#endif
    if (code == HC_ACTION)
    {
        HookMsgMessage hmm{};
        hmm.removed = (WORD)wParam;
        auto pmsg = (PMSG)lParam;
        hmm.hwnd = pmsg->hwnd;
        hmm.message = pmsg->message;
        hmm.wParam = pmsg->wParam;
        hmm.lParam = pmsg->lParam;
        hmm.time = pmsg->time;
        hmm.pt = pmsg->pt;
        messagesender.SendMessage(&hmm, sizeof(HookMsgMessage));
    }

    return CallNextHookEx(NULL, code, wParam, lParam);
}

#if _DEBUG
void LogDll(std::string message)
{
#if LOGWINOOKLIB
    Logger.WriteLine(message);
#endif
}

void LogDll(std::wstring message)
{
#if LOGWINOOKLIB
    Logger.WriteLine(message);
#endif
}

void LogDllMain(HINSTANCE hinst, std::wstring reason)
{
#if LOGWINOOKLIB
    std::wstringstream wss;
    wss << std::setw(16) << std::setfill(L'0') << std::hex << hinst;
    TCHAR procInfo[256];
    swprintf(procInfo, sizeof(procInfo), TEXT("Instance: %lx; Reason: %ls; ProcessId: %d; ThreadId: %d"),
        PtrToLong(hinst), reason.c_str(), GetCurrentProcessId(), GetThreadId(GetCurrentThread()));
    Logger.WriteLine(procInfo);
#endif
}
#endif